using DevExpress.Mvvm;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Models.Client.BMS
{
    // tb_ems_alarm 주기 폴링 서비스.
    // Start() 후 첫 폴링에서 현재 최대 alarm_id를 기준점으로 잡고,
    // 이후 신규 행(alarm_id > 기준점)만 AlarmsArrived로 발행한다.
    // DispatcherTimer Tick(UI 스레드)에서 DB 조회는 Task.Run으로 내려 UI 블로킹을 피한다.
    public class AlarmService
    {
        private const int MaxRowsPerPoll = 200;

        private readonly DispatcherTimer _timer;
        private readonly string _source;
        private long _lastAlarmId = -1; // -1 = 기준점 미설정(첫 폴링에서 max id로 초기화)
        private bool _isPolling;        // Tick 재진입 방지

        // 신규 알람 목록(오래된 순). UI 스레드에서 호출된다.
        public event Action<IReadOnlyList<AlarmItems>> AlarmsArrived;

        public AlarmService(string source = "BMS", double intervalSeconds = 5)
        {
            _source = source;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(intervalSeconds) };
            _timer.Tick += async (_, __) => await PollAsync();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private async Task PollAsync()
        {
            if (_isPolling) return;
            _isPolling = true;

            var app = Application.Current as App;
            try
            {
                var db = app?.DbManager;
                if (db == null) return;

                long lastId = _lastAlarmId;

                if (lastId < 0)
                {
                    // 첫 폴링: 기존 알람은 발행하지 않고 기준점만 잡는다.
                    _lastAlarmId = await Task.Run(() =>
                    {
                        db.EnsureEmsAlarmTable();
                        var ds = db.GetDataSetByQuery(
                            "select coalesce(max(alarm_id), 0) as max_id from public.tb_ems_alarm where source = @source",
                            cmd => cmd.Parameters.AddWithValue("@source", _source));
                        return ReadMaxId(ds);
                    });
                    return;
                }

                var newAlarms = await Task.Run(() =>
                {
                    var ds = db.GetDataSetByQuery(
                        "select * from public.tb_ems_alarm where source = @source and alarm_id > @last_id order by alarm_id asc limit @limit",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@source", _source);
                            cmd.Parameters.AddWithValue("@last_id", lastId);
                            cmd.Parameters.AddWithValue("@limit", MaxRowsPerPoll);
                        });
                    return ParseAlarms(ds, ref _lastAlarmId);
                });

                if (newAlarms.Count > 0)
                    AlarmsArrived?.Invoke(newAlarms);
            }
            catch (Exception ex)
            {
                app?.nlog?.Warn(ex, "Alarm polling failed.");
            }
            finally
            {
                _isPolling = false;
            }
        }

        private static long ReadMaxId(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return 0;

            var raw = ds.Tables[0].Rows[0]["max_id"];
            return raw == DBNull.Value ? 0 : Convert.ToInt64(raw);
        }

        private static List<AlarmItems> ParseAlarms(DataSet ds, ref long lastAlarmId)
        {
            var items = new List<AlarmItems>();
            if (ds == null || ds.Tables.Count == 0) return items;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                long alarmId = ReadLong(row, "alarm_id");
                if (alarmId > lastAlarmId) lastAlarmId = alarmId;

                items.Add(new AlarmItems
                {
                    OccurredAt = ReadDate(row, "occurred_at"),
                    Source = ReadString(row, "source"),
                    Category = ReadString(row, "category"),
                    Bit = ReadInt(row, "bit"),
                    Code = ReadInt(row, "alarm_code"),
                    Alarm = ReadString(row, "alarm_name"),
                    FaultMessage = ReadString(row, "fault_message"),
                    RawValue = ReadString(row, "raw_value"),
                });
            }

            return items;
        }

        private static string ReadString(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? row[columnName].ToString()
                : string.Empty;
        }

        private static int ReadInt(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            int value;
            return int.TryParse(row[columnName].ToString(), out value) ? value : 0;
        }

        private static long ReadLong(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return 0;

            long value;
            return long.TryParse(row[columnName].ToString(), out value) ? value : 0;
        }

        private static DateTime ReadDate(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return default(DateTime);

            DateTime value;
            return DateTime.TryParse(row[columnName].ToString(), out value) ? value : default(DateTime);
        }
    }

    public class AlarmItems : ViewModelBase
    {
        public DateTime OccurredAt { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public int Bit { get; set; }
        public int Code { get; set; } // 알람 Code
        public string Alarm { get; set; } // 알람 내용
        public string FaultMessage { get; set; }
        public string RawValue { get; set; }

    }
}
