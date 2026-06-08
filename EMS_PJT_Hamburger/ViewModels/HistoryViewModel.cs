using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Managers;
using Npgsql;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.ViewModels
{
    public sealed class HistoryViewModel : ViewModelBase
    {
        public ObservableCollection<HistoryDataRow> PcsRows { get; } = new ObservableCollection<HistoryDataRow>();
        public ObservableCollection<HistoryDataRow> BmsRows { get; } = new ObservableCollection<HistoryDataRow>();
        public ObservableCollection<HistoryAlarmRow> AlarmRows { get; } = new ObservableCollection<HistoryAlarmRow>();
        public ObservableCollection<HistoryMetricCard> MetricCards { get; } = new ObservableCollection<HistoryMetricCard>();

        public DelegateCommand Cmd_Refresh { get; private set; }
        public DelegateCommand Cmd_Today { get; private set; }
        public DelegateCommand Cmd_Week { get; private set; }

        public DateTime StartDate
        {
            get => GetProperty(() => StartDate);
            set => SetProperty(() => StartDate, value);
        }

        public DateTime EndDate
        {
            get => GetProperty(() => EndDate);
            set => SetProperty(() => EndDate, value);
        }

        public string StatusMessage
        {
            get => GetProperty(() => StatusMessage);
            set => SetProperty(() => StatusMessage, value);
        }

        public int PcsCount
        {
            get => GetProperty(() => PcsCount);
            set => SetProperty(() => PcsCount, value);
        }

        public int BmsCount
        {
            get => GetProperty(() => BmsCount);
            set => SetProperty(() => BmsCount, value);
        }

        public int AlarmCount
        {
            get => GetProperty(() => AlarmCount);
            set => SetProperty(() => AlarmCount, value);
        }

        public XyDataSeries<DateTime, double> PcsTrendSeries
        {
            get => GetProperty(() => PcsTrendSeries);
            set => SetProperty(() => PcsTrendSeries, value);
        }

        public XyDataSeries<DateTime, double> BmsTrendSeries
        {
            get => GetProperty(() => BmsTrendSeries);
            set => SetProperty(() => BmsTrendSeries, value);
        }

        public HistoryViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            PcsTrendSeries = CreateTrendSeries("PCS Trend");
            BmsTrendSeries = CreateTrendSeries("BMS Trend");

            Cmd_Refresh = new DelegateCommand(LoadHistory);
            Cmd_Today = new DelegateCommand(() =>
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                LoadHistory();
            });
            Cmd_Week = new DelegateCommand(() =>
            {
                StartDate = DateTime.Today.AddDays(-6);
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                LoadHistory();
            });

            LoadHistory();
        }

        private void LoadHistory()
        {
            try
            {
                PcsRows.Clear();
                BmsRows.Clear();
                AlarmRows.Clear();
                MetricCards.Clear();

                var app = Application.Current as App;
                var db = app?.DbManager;
                if (db == null)
                {
                    StatusMessage = "DB manager is not ready.";
                    return;
                }

                LoadPcsRows(db);
                LoadBmsRows(db);
                LoadAlarmRows(db);
                UpdateSummary();
                UpdateCharts();

                StatusMessage = $"Loaded PCS {PcsCount}, BMS {BmsCount}, Alarm {AlarmCount}.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"History load failed: {ex.Message}";
            }
        }

        private void LoadPcsRows(DbManager db)
        {
            var ds = db.GetDataSetByQuery(@"
select collected_at, message_name, payload_length, compressed_length, summary
from public.tb_ems_raw_data
where source = 'PCS'
  and collected_at >= @start_at
  and collected_at <= @end_at
order by collected_at desc
limit 200;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@start_at", StartDate);
                    cmd.Parameters.AddWithValue("@end_at", EndDate);
                });

            var table = FirstTable(ds);
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    var occurredAt = ReadDate(row, "collected_at", DateTime.MinValue);
                    var payloadLength = ReadDouble(row, "payload_length", 0);
                    var compressedLength = ReadDouble(row, "compressed_length", 0);

                    PcsRows.Add(new HistoryDataRow
                    {
                        Time = FormatTime(occurredAt),
                        Source = "PCS",
                        Name = ReadString(row, "message_name", "Raw"),
                        Value1Name = "Raw",
                        Value1 = FormatNumber(payloadLength),
                        Value2Name = "Compressed",
                        Value2 = FormatNumber(compressedLength),
                        Value3Name = "Summary",
                        Value3 = ReadString(row, "summary", "-"),
                        ChartValue = compressedLength
                    });
                }
            }

            LoadSystemStateRows(db, "PCS", PcsRows);
        }

        private void LoadBmsRows(DbManager db)
        {
            var ds = db.GetDataSetByQuery(@"
select collected_at, message_name, payload_length, compressed_length, summary
from public.tb_ems_raw_data
where source = 'BMS'
  and collected_at >= @start_at
  and collected_at <= @end_at
order by collected_at desc
limit 200;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@start_at", StartDate);
                    cmd.Parameters.AddWithValue("@end_at", EndDate);
                });

            var table = FirstTable(ds);
            if (table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    var occurredAt = ReadDate(row, "collected_at", DateTime.MinValue);
                    var payloadLength = ReadDouble(row, "payload_length", 0);
                    var compressedLength = ReadDouble(row, "compressed_length", 0);

                    BmsRows.Add(new HistoryDataRow
                    {
                        Time = FormatTime(occurredAt),
                        Source = "BMS",
                        Name = ReadString(row, "message_name", "Raw"),
                        Value1Name = "Raw",
                        Value1 = FormatNumber(payloadLength),
                        Value2Name = "Compressed",
                        Value2 = FormatNumber(compressedLength),
                        Value3Name = "Summary",
                        Value3 = ReadString(row, "summary", "-"),
                        ChartValue = compressedLength
                    });
                }
            }

            LoadSystemStateRows(db, "BMS", BmsRows);
        }

        private void LoadSystemStateRows(DbManager db, string source, ObservableCollection<HistoryDataRow> target)
        {
            var ds = db.GetDataSetByQuery(@"
select collected_at, ready_name, ready_text, ready_value, raw_value
from public.tb_ems_system_state
where source = @source
  and collected_at >= @start_at
  and collected_at <= @end_at
order by collected_at desc
limit 100;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@source", source);
                    cmd.Parameters.AddWithValue("@start_at", StartDate);
                    cmd.Parameters.AddWithValue("@end_at", EndDate);
                });

            var table = FirstTable(ds);
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                var readyValue = ReadString(row, "ready_value", "-");
                target.Add(new HistoryDataRow
                {
                    Time = FormatTime(ReadDate(row, "collected_at", DateTime.MinValue)),
                    Source = source,
                    Name = ReadString(row, "ready_name", "Ready"),
                    Value1Name = "Ready",
                    Value1 = ReadString(row, "ready_text", readyValue),
                    Value2Name = "Raw",
                    Value2 = ReadString(row, "raw_value", "-"),
                    Value3Name = "Type",
                    Value3 = "System",
                    ChartValue = string.Equals(readyValue, "true", StringComparison.OrdinalIgnoreCase) ? 1 : 0
                });
            }
        }

        private void LoadAlarmRows(DbManager db)
        {
            var ds = db.GetDataSetByQuery(@"
select *
from public.tb_ems_alarm
where occurred_at >= @start_at
  and occurred_at <= @end_at
order by occurred_at desc
limit 300;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@start_at", StartDate);
                    cmd.Parameters.AddWithValue("@end_at", EndDate);
                });

            var table = FirstTable(ds);
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                AlarmRows.Add(new HistoryAlarmRow
                {
                    Time = FormatTime(ReadDate(row, "occurred_at", DateTime.MinValue)),
                    Source = ReadString(row, "source", "-"),
                    Category = ReadString(row, "category", "-"),
                    Code = ReadString(row, "alarm_code", "-"),
                    Message = ReadString(row, "fault_message", ReadString(row, "alarm_name", "-")),
                    Raw = ReadString(row, "raw_value", "-"),
                    IsReset = ReadString(row, "is_reset", "false"),
                    Severity = ResolveSeverity(row),
                    SeverityBrush = ResolveSeverityBrush(row)
                });
            }
        }

        private void UpdateSummary()
        {
            PcsCount = PcsRows.Count;
            BmsCount = BmsRows.Count;
            AlarmCount = AlarmRows.Count;

            MetricCards.Add(new HistoryMetricCard { Title = "PCS DATA", Value = PcsCount.ToString(), Unit = "건", Accent = "#FF76F7A8" });
            MetricCards.Add(new HistoryMetricCard { Title = "BMS DATA", Value = BmsCount.ToString(), Unit = "건", Accent = "#FF4EA5FF" });
            MetricCards.Add(new HistoryMetricCard { Title = "ALARMS", Value = AlarmCount.ToString(), Unit = "건", Accent = "#FFFF5F5F" });
            MetricCards.Add(new HistoryMetricCard { Title = "PERIOD", Value = $"{StartDate:MM-dd} ~ {EndDate:MM-dd}", Unit = string.Empty, Accent = "#FFFFC83D", SubValue = BuildPeriodText() });
        }

        private void UpdateCharts()
        {
            PcsTrendSeries = BuildTrendSeries("PCS Trend", PcsRows.Reverse().ToArray());
            BmsTrendSeries = BuildTrendSeries("BMS Trend", BmsRows.Reverse().ToArray());
        }

        private static XyDataSeries<DateTime, double> CreateTrendSeries(string name)
        {
            return new XyDataSeries<DateTime, double> { SeriesName = name };
        }

        private static XyDataSeries<DateTime, double> BuildTrendSeries(string name, HistoryDataRow[] rows)
        {
            var series = CreateTrendSeries(name);
            if (rows == null || rows.Length == 0)
                return series;

            foreach (var row in rows)
            {
                if (!DateTime.TryParse(row.Time, CultureInfo.CurrentCulture, DateTimeStyles.None, out var time))
                    continue;

                series.Append(time, row.ChartValue);
            }

            return series;
        }

        private string BuildPeriodText()
        {
            var days = Math.Max(1, (EndDate.Date - StartDate.Date).Days + 1);
            return days == 1 ? "1 Day" : $"{days} Days";
        }

        private bool IsInRangeOrUnknown(DateTime value)
        {
            if (value == DateTime.MinValue) return true;
            return value >= StartDate && value <= EndDate;
        }

        private static DataTable FirstTable(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0) return null;
            return ds.Tables[0];
        }

        private static bool HasColumn(DataRow row, string name)
        {
            return row?.Table?.Columns.Cast<DataColumn>()
                .Any(c => string.Equals(c.ColumnName, name, StringComparison.OrdinalIgnoreCase)) == true;
        }

        private static string ReadString(DataRow row, string name, string fallback)
        {
            if (!HasColumn(row, name)) return fallback;
            var raw = row[name];
            return raw == null || raw == DBNull.Value ? fallback : raw.ToString();
        }

        private static double ReadDouble(DataRow row, string name, double fallback)
        {
            if (!HasColumn(row, name)) return fallback;
            var raw = row[name];
            if (raw == null || raw == DBNull.Value) return fallback;

            try { return Convert.ToDouble(raw, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private static DateTime ReadDate(DataRow row, string name, DateTime fallback)
        {
            if (!HasColumn(row, name)) return fallback;
            var raw = row[name];
            if (raw == null || raw == DBNull.Value) return fallback;

            try { return Convert.ToDateTime(raw, CultureInfo.CurrentCulture); }
            catch { return fallback; }
        }

        private static string FormatTime(DateTime value)
        {
            return value == DateTime.MinValue ? "-" : value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.###", CultureInfo.CurrentCulture);
        }

        private static string ResolveSeverity(DataRow row)
        {
            var source = ReadString(row, "source", string.Empty);
            var category = ReadString(row, "category", string.Empty);
            var message = ReadString(row, "fault_message", ReadString(row, "alarm_name", string.Empty));
            var merged = $"{source} {category} {message}";

            if (merged.IndexOf("Fault", StringComparison.OrdinalIgnoreCase) >= 0 ||
                merged.IndexOf("BMS", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Critical";

            return "Warning";
        }

        private static string ResolveSeverityBrush(DataRow row)
        {
            return string.Equals(ResolveSeverity(row), "Critical", StringComparison.OrdinalIgnoreCase)
                ? "#FFFF5F5F"
                : "#FFFFC83D";
        }
    }

    public sealed class HistoryDataRow
    {
        public string Time { get; set; }
        public string Source { get; set; }
        public string Name { get; set; }
        public string Value1Name { get; set; }
        public string Value1 { get; set; }
        public string Value2Name { get; set; }
        public string Value2 { get; set; }
        public string Value3Name { get; set; }
        public string Value3 { get; set; }
        public double ChartValue { get; set; }
    }

    public sealed class HistoryAlarmRow
    {
        public string Time { get; set; }
        public string Source { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Raw { get; set; }
        public string IsReset { get; set; }
        public string Severity { get; set; }
        public string SeverityBrush { get; set; }
    }

    public sealed class HistoryMetricCard
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Unit { get; set; }
        public string Accent { get; set; }
        public string SubValue { get; set; }
    }
}
