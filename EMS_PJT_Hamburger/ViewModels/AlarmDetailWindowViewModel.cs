using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client.BMS;
using EMS_PJT_Hamburger.Models.Client.PCS;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class AlarmDetailWindowViewModel : ViewModelBase
    {
        private const string SourceBms = "BMS";
        private const string SourcePcs = "PCS";

        public readonly AlarmService _service;
        private readonly ObservableCollection<PcsFaultItem> _pcsCurrentFaults;

        private int OccurredAlarmCnt { get; set; }

        public string SourceName
        {
            get => GetProperty(() => SourceName);
            set => SetProperty(() => SourceName, value);
        }

        public bool SelectAll
        {
            get => GetProperty(() => SelectAll);
            set => SetProperty(() => SelectAll, value);
        }

        protected virtual void OnSelectAllChanged()
        {
            if (SelectAll)
            {
                SelectTop100 = false;
                SelectCurrent = false;
            }
        }

        public bool SelectTop100
        {
            get => GetProperty(() => SelectTop100);
            set => SetProperty(() => SelectTop100, value);
        }

        protected virtual void OnSelectTop100Changed()
        {
            if (SelectTop100)
            {
                SelectAll = false;
                SelectCurrent = false;
            }
        }

        public bool SelectCurrent
        {
            get => GetProperty(() => SelectCurrent);
            set => SetProperty(() => SelectCurrent, value);
        }

        protected virtual void OnSelectCurrentChanged()
        {
            if (SelectCurrent)
            {
                SelectTop100 = false;
                SelectAll = false;
            }
        }

        public ObservableCollection<AlarmItems> Alarms { get; private set; }
        public ICommand Cmd_LoadAlarm { get; set; }
        public ICommand Cmd_ExportAlarm { get; set; }

        public AlarmDetailWindowViewModel(AlarmService service, int cnt)
            : this(SourceBms, service, cnt, null)
        {
        }

        public AlarmDetailWindowViewModel(
            string sourceName,
            AlarmService service,
            int cnt,
            ObservableCollection<PcsFaultItem> pcsCurrentFaults)
        {
            SourceName = string.IsNullOrWhiteSpace(sourceName) ? SourceBms : sourceName;
            _service = service;
            OccurredAlarmCnt = cnt;
            _pcsCurrentFaults = pcsCurrentFaults;

            Init();
            CommandInit();
        }

        private bool IsPcs => string.Equals(SourceName, SourcePcs, StringComparison.OrdinalIgnoreCase);

        private void Init()
        {
            Alarms = new ObservableCollection<AlarmItems>();
            SelectTop100 = true;
        }

        private void CommandInit()
        {
            Cmd_LoadAlarm = new DelegateCommand(LoadAlarm);
            Cmd_ExportAlarm = new DelegateCommand(ExportAlarm);
        }

        private void LoadAlarm()
        {
            Alarms.Clear();

            var app = Application.Current as App;
            app?.DbManager?.EnsureEmsAlarmTable();

            if (SelectCurrent)
            {
                LoadCurrentAlarms();
                return;
            }

            DataSet ds = null;
            if (SelectAll)
                ds = app?.DbManager?.SelectEmsAlarmData(SourceName, 0, 0);
            else if (SelectTop100)
                ds = app?.DbManager?.SelectEmsAlarmData(SourceName, 2, 100);

            LoadFromDataSet(ds);
        }

        private void LoadCurrentAlarms()
        {
            if (IsPcs)
            {
                if (_pcsCurrentFaults == null) return;

                foreach (var item in _pcsCurrentFaults)
                {
                    var alarm = CreatePcsAlarmItem(item);
                    if (alarm != null) Alarms.Add(alarm);
                }

                return;
            }

            if (OccurredAlarmCnt == 0) return;

            var app = Application.Current as App;
            var ds = app?.DbManager?.SelectEmsAlarmData(SourceName, 2, OccurredAlarmCnt);
            LoadFromDataSet(ds);
        }

        private void LoadFromDataSet(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0) return;

            foreach (DataRow row in ds.Tables[0].Rows)
            {
                Alarms.Add(new AlarmItems
                {
                    OccurredAt = ReadDate(row, "occurred_at"),
                    Source = ReadString(row, "source"),
                    Category = ReadString(row, "category"),
                    Bit = ReadInt(row, "bit"),
                    Code = ReadInt(row, "alarm_code"),
                    Alarm = ReadString(row, "alarm_name"),
                    FaultMessage = ReadString(row, "fault_message"),
                    RawValue = ReadString(row, "raw_value"),
                    Hour = ReadString(row, "duration_hour"),
                });
            }
        }

        private void ExportAlarm()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"{SourceName}_alarms_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("Time\tSource\tCategory\tBit\tCode\tFault Message\tRaw\tHour");

            foreach (var item in Alarms)
            {
                sb.AppendLine(string.Join("\t", new[]
                {
                    item.OccurredAt == default(DateTime) ? string.Empty : item.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    item.Source ?? string.Empty,
                    item.Category ?? string.Empty,
                    item.Bit.ToString(),
                    item.Code.ToString(),
                    item.FaultMessage ?? item.Alarm ?? string.Empty,
                    item.RawValue ?? string.Empty,
                    item.Hour ?? string.Empty
                }));
            }

            System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
        }

        private static AlarmItems CreatePcsAlarmItem(PcsFaultItem item)
        {
            if (item == null) return null;

            return new AlarmItems
            {
                OccurredAt = item.OccurredAt,
                Source = SourcePcs,
                Category = item.Category,
                Bit = item.Bit,
                Code = item.Bit,
                Alarm = item.Message,
                FaultMessage = item.Message,
                RawValue = item.RawValue.ToString()
            };
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

        private static DateTime ReadDate(DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return default(DateTime);

            DateTime value;
            return DateTime.TryParse(row[columnName].ToString(), out value) ? value : default(DateTime);
        }
    }
}
