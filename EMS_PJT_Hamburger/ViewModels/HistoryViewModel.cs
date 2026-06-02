using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Managers;
using Npgsql;
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
        private const double ChartWidth = 720d;
        private const double ChartHeight = 190d;

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

        public PointCollection PcsChartPoints
        {
            get => GetProperty(() => PcsChartPoints);
            set => SetProperty(() => PcsChartPoints, value);
        }

        public PointCollection BmsChartPoints
        {
            get => GetProperty(() => BmsChartPoints);
            set => SetProperty(() => BmsChartPoints, value);
        }

        public HistoryViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            PcsChartPoints = new PointCollection();
            BmsChartPoints = new PointCollection();

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
select *
from public.tb_pcs_grid
order by 1 desc
limit 200;");

            var table = FirstTable(ds);
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                var occurredAt = ReadDate(row, "collected_at", DateTime.MinValue);
                if (!IsInRangeOrUnknown(occurredAt)) continue;

                var imported = ReadDouble(row, "total_imported", ReadDouble(row, "grid_total_imported_energy", 0));
                var exported = ReadDouble(row, "total_exported", ReadDouble(row, "grid_total_exported_energy", 0));
                PcsRows.Add(new HistoryDataRow
                {
                    Time = FormatTime(occurredAt),
                    Source = "PCS",
                    Name = "Grid Energy",
                    Value1Name = "Imported",
                    Value1 = FormatNumber(imported),
                    Value2Name = "Exported",
                    Value2 = FormatNumber(exported),
                    Value3Name = "Unit",
                    Value3 = "kWh",
                    ChartValue = imported + exported
                });
            }
        }

        private void LoadBmsRows(DbManager db)
        {
            var ds = db.GetDataSetByQuery(@"
select *
from public.tb_bms
order by 1 desc
limit 200;");

            var table = FirstTable(ds);
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                var occurredAt = ReadDate(row, "collected_at", ReadDate(row, "occurred_at", DateTime.MinValue));
                if (!IsInRangeOrUnknown(occurredAt)) continue;

                var soc = ReadDouble(row, "disp_soc", ReadDouble(row, "soc", 0));
                var voltage = ReadDouble(row, "total_volt", ReadDouble(row, "total_voltage", 0));
                var current = ReadDouble(row, "total_curr", ReadDouble(row, "total_current", 0));

                BmsRows.Add(new HistoryDataRow
                {
                    Time = FormatTime(occurredAt),
                    Source = "BMS",
                    Name = ReadString(row, "status", "Battery"),
                    Value1Name = "SOC",
                    Value1 = FormatNumber(soc),
                    Value2Name = "Voltage",
                    Value2 = FormatNumber(voltage),
                    Value3Name = "Current",
                    Value3 = FormatNumber(current),
                    ChartValue = soc
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
                    IsReset = ReadString(row, "is_reset", "false")
                });
            }
        }

        private void UpdateSummary()
        {
            PcsCount = PcsRows.Count;
            BmsCount = BmsRows.Count;
            AlarmCount = AlarmRows.Count;

            MetricCards.Add(new HistoryMetricCard { Title = "PCS DATA", Value = PcsCount.ToString(), Accent = "#FF76F7A8" });
            MetricCards.Add(new HistoryMetricCard { Title = "BMS DATA", Value = BmsCount.ToString(), Accent = "#FF87FFFF" });
            MetricCards.Add(new HistoryMetricCard { Title = "ALARMS", Value = AlarmCount.ToString(), Accent = "#FFFF8C8C" });
            MetricCards.Add(new HistoryMetricCard { Title = "PERIOD", Value = $"{StartDate:MM-dd} ~ {EndDate:MM-dd}", Accent = "#FFFFE2A6" });
        }

        private void UpdateCharts()
        {
            PcsChartPoints = BuildPoints(PcsRows.Reverse().Select(x => x.ChartValue).ToArray());
            BmsChartPoints = BuildPoints(BmsRows.Reverse().Select(x => x.ChartValue).ToArray());
        }

        private static PointCollection BuildPoints(double[] values)
        {
            var points = new PointCollection();
            if (values == null || values.Length == 0)
                return points;

            var min = values.Min();
            var max = values.Max();
            var span = Math.Max(1d, max - min);

            for (var i = 0; i < values.Length; i++)
            {
                var x = values.Length == 1 ? 0 : (ChartWidth / (values.Length - 1)) * i;
                var y = ChartHeight - ((values[i] - min) / span * ChartHeight);
                points.Add(new Point(x, y));
            }

            return points;
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
    }

    public sealed class HistoryMetricCard
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Accent { get; set; }
    }
}
