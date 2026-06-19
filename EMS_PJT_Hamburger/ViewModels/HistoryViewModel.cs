using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client.PCS;
using EMS_PJT_Hamburger.Models.Managers;
using Microsoft.Win32;
using Npgsql;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.ViewModels
{
    public sealed class HistoryViewModel : ViewModelBase
    {
        private const int MaxHourlyPayloadRows = 720;
        private const int MaxMinutePayloadRows = 1440;
        private const int MaxHistoryDays = 30;

        // PCS/BMS Trend 버킷 간격(분단위/시간단위). 가시 범위 계산에 사용.
        private TimeSpan _pcsTrendInterval = TimeSpan.FromHours(1);
        private TimeSpan _bmsTrendInterval = TimeSpan.FromHours(1);

        public ObservableCollection<HistoryDataRow> PcsRows { get; } = new ObservableCollection<HistoryDataRow>();
        public ObservableCollection<HistoryDataRow> BmsRows { get; } = new ObservableCollection<HistoryDataRow>();
        public ObservableCollection<HistoryAlarmRow> AlarmRows { get; } = new ObservableCollection<HistoryAlarmRow>();
        public ObservableCollection<HistoryMetricCard> MetricCards { get; } = new ObservableCollection<HistoryMetricCard>();

        public DelegateCommand Cmd_Refresh { get; private set; }
        public DelegateCommand Cmd_Today { get; private set; }
        public DelegateCommand Cmd_Week { get; private set; }
        public DelegateCommand Cmd_ExportRaw { get; private set; }

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

        // PCS Total Export Energy 변화량(Δ) 시리즈 (초록)
        public XyDataSeries<DateTime, double> PcsExportTrendSeries
        {
            get => GetProperty(() => PcsExportTrendSeries);
            set => SetProperty(() => PcsExportTrendSeries, value);
        }

        // PCS Total Import Energy 변화량(Δ) 시리즈 (파랑)
        public XyDataSeries<DateTime, double> PcsImportTrendSeries
        {
            get => GetProperty(() => PcsImportTrendSeries);
            set => SetProperty(() => PcsImportTrendSeries, value);
        }

        public XyDataSeries<DateTime, double> BmsTrendSeries
        {
            get => GetProperty(() => BmsTrendSeries);
            set => SetProperty(() => BmsTrendSeries, value);
        }

        public string TrendXAxisTextFormatting
        {
            get => GetProperty(() => TrendXAxisTextFormatting);
            set => SetProperty(() => TrendXAxisTextFormatting, value);
        }

        // PCS Trend X축 기본 가시 범위(마지막 10개 버킷). View가 로드/더블클릭 시 이 값으로 리셋한다.
        public DateRange PcsTrendDefaultVisibleRange
        {
            get => GetProperty(() => PcsTrendDefaultVisibleRange);
            set => SetProperty(() => PcsTrendDefaultVisibleRange, value);
        }

        // BMS Trend X축 기본 가시 범위(마지막 10개 버킷).
        public DateRange BmsTrendDefaultVisibleRange
        {
            get => GetProperty(() => BmsTrendDefaultVisibleRange);
            set => SetProperty(() => BmsTrendDefaultVisibleRange, value);
        }

        public HistoryViewModel()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
            TrendXAxisTextFormatting = "HH:mm";
            PcsExportTrendSeries = CreateTrendSeries("Export Δ");
            PcsImportTrendSeries = CreateTrendSeries("Import Δ");
            BmsTrendSeries = CreateTrendSeries("BMS Trend");
            PcsTrendDefaultVisibleRange = new DateRange(StartDate, EndDate);
            BmsTrendDefaultVisibleRange = new DateRange(StartDate, EndDate);

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
            Cmd_ExportRaw = new DelegateCommand(ExportRawData);

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

                if (!ValidateSearchPeriod())
                    return;

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
            // Today(단일 일자) 조회는 분단위 최대값, 다중 일자 조회는 시간단위 최대값.
            // truncUnit은 내부 상수('minute'/'hour')라 SQL 주입 위험이 없다.
            var isSingleDay = (EndDate.Date - StartDate.Date).Days == 0;
            var truncUnit = isSingleDay ? "minute" : "hour";
            _pcsTrendInterval = isSingleDay ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(1);
            var limit = isSingleDay ? MaxMinutePayloadRows : MaxHourlyPayloadRows;

            // 버킷별 누적 Total Export/Import Energy(kWh)의 증가분(Δ = max - min)
            var ds = db.GetDataSetByQuery($@"
select collected_at, export_delta, import_delta
from
(
    select
        date_trunc('{truncUnit}', collected_at) as collected_at,
        max(pcs_total_export_kwh) - min(pcs_total_export_kwh) as export_delta,
        max(pcs_total_import_kwh) - min(pcs_total_import_kwh) as import_delta
    from public.tb_ems_raw_data
    where source = 'PCS'
      and collected_at >= @start_at
      and collected_at <= @end_at
      and (pcs_total_export_kwh is not null or pcs_total_import_kwh is not null)
    group by date_trunc('{truncUnit}', collected_at)
) bucket_metric
order by collected_at desc
limit @limit;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@start_at", QueryStartDate());
                    cmd.Parameters.AddWithValue("@end_at", QueryEndDate());
                    cmd.Parameters.AddWithValue("@limit", limit);
                });

            AddPcsDeltaRows(FirstTable(ds));

            LoadSystemStateRows(db, "PCS", PcsRows);
        }

        private void LoadBmsRows(DbManager db)
        {
            // PCS Trend와 동일 규칙: 단일 일자(Today)는 분단위, 다중 일자(Week)는 시간단위 SOC 최대값.
            var isSingleDay = (EndDate.Date - StartDate.Date).Days == 0;
            var truncUnit = isSingleDay ? "minute" : "hour";
            _bmsTrendInterval = isSingleDay ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(1);
            var limit = isSingleDay ? MaxMinutePayloadRows : MaxHourlyPayloadRows;

            var ds = db.GetDataSetByQuery($@"
select collected_at, max_value
from
(
    select
        date_trunc('{truncUnit}', collected_at) as collected_at,
        max(bms_soc) as max_value
    from public.tb_ems_raw_data
    where source = 'BMS'
      and collected_at >= @start_at
      and collected_at <= @end_at
      and bms_soc is not null
    group by date_trunc('{truncUnit}', collected_at)
) bucket_metric
order by collected_at desc
limit @limit;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@start_at", QueryStartDate());
                    cmd.Parameters.AddWithValue("@end_at", QueryEndDate());
                    cmd.Parameters.AddWithValue("@limit", limit);
                });

            AddHourlyMetricRows(FirstTable(ds), BmsRows, "BMS", "Max SOC", "%");

            LoadSystemStateRows(db, "BMS", BmsRows);
        }

        private void AddHourlyMetricRows(
            DataTable table,
            ObservableCollection<HistoryDataRow> target,
            string source,
            string metricName,
            string unit)
        {
            if (table == null)
                return;

            foreach (DataRow row in table.Rows)
            {
                var occurredAt = ReadDate(row, "collected_at", DateTime.MinValue);
                if (occurredAt == DateTime.MinValue)
                    continue;

                var value = ReadDouble(row, "max_value", 0);

                target.Add(new HistoryDataRow
                {
                    OccurredAt = occurredAt,
                    IsPayloadTrendPoint = true,
                    Time = FormatTime(occurredAt),
                    Source = source,
                    Name = metricName,
                    Value1Name = "Max",
                    Value1 = FormatNumber(value),
                    Value2Name = "Unit",
                    Value2 = unit,
                    Value3Name = "Summary",
                    Value3 = "Hourly Max",
                    ChartValue = value
                });
            }
        }

        // PCS Total Export/Import Energy 버킷별 변화량(Δ) 행 추가. 음수는 0으로 클램프.
        private void AddPcsDeltaRows(DataTable table)
        {
            if (table == null)
                return;

            foreach (DataRow row in table.Rows)
            {
                var occurredAt = ReadDate(row, "collected_at", DateTime.MinValue);
                if (occurredAt == DateTime.MinValue)
                    continue;

                var exportDelta = Math.Max(0, ReadDouble(row, "export_delta", 0));
                var importDelta = Math.Max(0, ReadDouble(row, "import_delta", 0));

                PcsRows.Add(new HistoryDataRow
                {
                    OccurredAt = occurredAt,
                    IsPayloadTrendPoint = true,
                    Time = FormatTime(occurredAt),
                    Source = "PCS",
                    Name = "Energy Δ",
                    Value1Name = "Export",
                    Value1 = FormatNumber(exportDelta),
                    Value2Name = "Import",
                    Value2 = FormatNumber(importDelta),
                    Value3Name = "Unit",
                    Value3 = "kWh",
                    ExportDelta = exportDelta,
                    ImportDelta = importDelta,
                    ChartValue = exportDelta
                });
            }
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
                    cmd.Parameters.AddWithValue("@start_at", QueryStartDate());
                    cmd.Parameters.AddWithValue("@end_at", QueryEndDate());
                });

            var table = FirstTable(ds);
            if (table == null) return;

            foreach (DataRow row in table.Rows)
            {
                var readyValue = ReadString(row, "ready_value", "-");
                target.Add(new HistoryDataRow
                {
                    OccurredAt = ReadDate(row, "collected_at", DateTime.MinValue),
                    IsPayloadTrendPoint = false,
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
                    cmd.Parameters.AddWithValue("@start_at", QueryStartDate());
                    cmd.Parameters.AddWithValue("@end_at", QueryEndDate());
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

        // 조회 기간의 PCS 측정 raw 데이터를 파일로 내보낸다.
        // tb_ems_raw_data.payload_json(Deflate 압축 JSON)을 해제해 필드별 컬럼의 탭 구분 텍스트로 저장.
        private void ExportRawData()
        {
            try
            {
                var app = Application.Current as App;
                var db = app?.DbManager;
                if (db == null)
                {
                    StatusMessage = "DB manager is not ready.";
                    return;
                }

                if (!ValidateSearchPeriod())
                    return;

                var ds = db.GetDataSetByQuery(@"
select collected_at, message_name, payload_json
from public.tb_ems_raw_data
where source = 'PCS'
  and collected_at >= @start_at
  and collected_at <= @end_at
order by collected_at;",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@start_at", QueryStartDate());
                        cmd.Parameters.AddWithValue("@end_at", QueryEndDate());
                    });

                var table = FirstTable(ds);
                if (table == null || table.Rows.Count == 0)
                {
                    StatusMessage = "Export: 해당 기간에 PCS 데이터가 없습니다.";
                    return;
                }

                // PcsTime*(연/월일/시분/초밀리초)는 export에서 제외(collected_at로 대체).
                var excludedCols = new HashSet<string>(PcsSpecs.TimeData.Select(x => x.Name), StringComparer.Ordinal);

                // 각 행의 payload 압축 해제 -> JSON -> 필드. 컬럼은 등장 순서 유지.
                var records = new List<KeyValuePair<DateTime, Dictionary<string, string>>>();
                var columns = new List<string>();
                var seen = new HashSet<string>(StringComparer.Ordinal);

                foreach (DataRow row in table.Rows)
                {
                    var time = ReadDate(row, "collected_at", DateTime.MinValue);
                    var bytes = ReadBytea(row, "payload_json");
                    if (bytes == null)
                        continue;

                    var json = Encoding.UTF8.GetString(db.Decompress(bytes));
                    var fields = ParseJsonToStringDict(json);
                    foreach (var key in fields.Keys)
                        if (!excludedCols.Contains(key) && seen.Add(key)) columns.Add(key);

                    records.Add(new KeyValuePair<DateTime, Dictionary<string, string>>(time, fields));
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    FileName = $"PCS_raw_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.xlsx"
                };

                if (dialog.ShowDialog() != true)
                    return;

                var sheetDefs = BuildCategorySheets(columns);
                WriteXlsx(dialog.FileName, sheetDefs, records);
                StatusMessage = $"Export 완료: {records.Count}행, 시트 {sheetDefs.Count}개 -> {System.IO.Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Export 실패: {ex.Message}";
            }
        }

        private static byte[] ReadBytea(DataRow row, string name)
        {
            if (!HasColumn(row, name)) return null;
            var raw = row[name];
            if (raw == null || raw == DBNull.Value) return null;
            // Npgsql은 bytea를 byte[]로 반환한다.
            return raw as byte[];
        }

        private static Dictionary<string, string> ParseJsonToStringDict(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(json)) return result;

            using (var doc = JsonDocument.Parse(json))
            {
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                        result[prop.Name] = prop.Value.ToString();
                }
            }
            return result;
        }

        // 측정 컬럼을 PcsSpecs 카테고리(Grid/Inverter/Load/Battery/Etc)별 시트로 분류한다.
        // 미분류 컬럼은 Etc로 묶고, 비어있는 카테고리는 생략한다.
        private static List<KeyValuePair<string, List<string>>> BuildCategorySheets(List<string> columns)
        {
            var present = new HashSet<string>(columns, StringComparer.Ordinal);
            var sheets = new List<KeyValuePair<string, List<string>>>();

            void AddCat(string name, IEnumerable<string> specNames)
            {
                var cols = specNames.Where(present.Contains).ToList();
                if (cols.Count > 0)
                    sheets.Add(new KeyValuePair<string, List<string>>(name, cols));
            }

            AddCat("Grid", PcsSpecs.GridData.Select(x => x.Name));
            AddCat("Inverter", PcsSpecs.InvData.Select(x => x.Name));
            AddCat("Load", PcsSpecs.LoadData.Select(x => x.Name));
            AddCat("Battery", PcsSpecs.BatteryData.Select(x => x.Name));
            AddCat("Etc", PcsSpecs.EtcData.Select(x => x.Name));

            var categorized = new HashSet<string>(sheets.SelectMany(s => s.Value), StringComparer.Ordinal);
            var leftovers = columns.Where(c => !categorized.Contains(c)).ToList();
            if (leftovers.Count > 0)
            {
                var etc = sheets.FirstOrDefault(s => s.Key == "Etc");
                if (etc.Value != null) etc.Value.AddRange(leftovers);
                else sheets.Add(new KeyValuePair<string, List<string>>("Etc", leftovers));
            }

            if (sheets.Count == 0)
                sheets.Add(new KeyValuePair<string, List<string>>("PCS", columns));

            return sheets;
        }

        // 외부 라이브러리 없이 System.IO.Compression으로 최소 OpenXML(.xlsx) 패키지를 직접 생성한다.
        // sheets: (시트명, 컬럼목록). 각 시트는 collected_at + 해당 컬럼으로 구성된다.
        private static void WriteXlsx(
            string path,
            List<KeyValuePair<string, List<string>>> sheets,
            List<KeyValuePair<DateTime, Dictionary<string, string>>> records)
        {
            const string rootRels =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
                "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
                "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>" +
                "</Relationships>";

            var ct = new StringBuilder();
            ct.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            ct.Append("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
            ct.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
            ct.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
            ct.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
            for (int i = 0; i < sheets.Count; i++)
                ct.Append("<Override PartName=\"/xl/worksheets/sheet" + (i + 1) + ".xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
            ct.Append("</Types>");

            var wb = new StringBuilder();
            wb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            wb.Append("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
            for (int i = 0; i < sheets.Count; i++)
                wb.Append("<sheet name=\"" + XlsxEscape(sheets[i].Key) + "\" sheetId=\"" + (i + 1) + "\" r:id=\"rId" + (i + 1) + "\"/>");
            wb.Append("</sheets></workbook>");

            var wbRels = new StringBuilder();
            wbRels.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            wbRels.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (int i = 0; i < sheets.Count; i++)
                wbRels.Append("<Relationship Id=\"rId" + (i + 1) + "\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet" + (i + 1) + ".xml\"/>");
            wbRels.Append("</Relationships>");

            var utf8 = new UTF8Encoding(false);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                AddXlsxEntry(zip, "[Content_Types].xml", ct.ToString(), utf8);
                AddXlsxEntry(zip, "_rels/.rels", rootRels, utf8);
                AddXlsxEntry(zip, "xl/workbook.xml", wb.ToString(), utf8);
                AddXlsxEntry(zip, "xl/_rels/workbook.xml.rels", wbRels.ToString(), utf8);

                for (int i = 0; i < sheets.Count; i++)
                    WriteSheet(zip, "xl/worksheets/sheet" + (i + 1) + ".xml", sheets[i].Value, records, utf8);
            }
        }

        private static void WriteSheet(
            ZipArchive zip,
            string partName,
            List<string> columns,
            List<KeyValuePair<DateTime, Dictionary<string, string>>> records,
            Encoding utf8)
        {
            var entry = zip.CreateEntry(partName);
            using (var w = new StreamWriter(entry.Open(), utf8))
            {
                w.Write("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                w.Write("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");

                var rowNum = 1;
                w.Write("<row r=\"" + rowNum + "\">");
                WriteCell(w, 0, rowNum, "collected_at");
                for (int c = 0; c < columns.Count; c++)
                    WriteCell(w, c + 1, rowNum, columns[c]);
                w.Write("</row>");
                rowNum++;

                foreach (var rec in records)
                {
                    w.Write("<row r=\"" + rowNum + "\">");
                    WriteCell(w, 0, rowNum, rec.Key == DateTime.MinValue ? string.Empty : rec.Key.ToString("yyyy-MM-dd HH:mm:ss"));
                    for (int c = 0; c < columns.Count; c++)
                    {
                        var v = rec.Value.TryGetValue(columns[c], out var vv) ? vv : string.Empty;
                        WriteCell(w, c + 1, rowNum, v);
                    }
                    w.Write("</row>");
                    rowNum++;
                }

                w.Write("</sheetData></worksheet>");
            }
        }

        private static void AddXlsxEntry(ZipArchive zip, string name, string content, Encoding enc)
        {
            var entry = zip.CreateEntry(name);
            using (var w = new StreamWriter(entry.Open(), enc))
                w.Write(content);
        }

        // 숫자면 number 셀(<v>), 아니면 inline 문자열 셀로 기록.
        private static void WriteCell(StreamWriter w, int colIndex, int rowNum, string value)
        {
            var cellRef = XlsxColumnLetter(colIndex) + rowNum;
            if (!string.IsNullOrEmpty(value) &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
            {
                w.Write("<c r=\"" + cellRef + "\"><v>" + num.ToString(CultureInfo.InvariantCulture) + "</v></c>");
            }
            else
            {
                w.Write("<c r=\"" + cellRef + "\" t=\"inlineStr\"><is><t xml:space=\"preserve\">" + XlsxEscape(value) + "</t></is></c>");
            }
        }

        private static string XlsxColumnLetter(int index)
        {
            var sb = new StringBuilder();
            index++;
            while (index > 0)
            {
                var rem = (index - 1) % 26;
                sb.Insert(0, (char)('A' + rem));
                index = (index - 1) / 26;
            }
            return sb.ToString();
        }

        private static string XlsxEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
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
            TrendXAxisTextFormatting = BuildTrendXAxisTextFormatting();
            var pcsPoints = PcsRows.Where(x => x.IsPayloadTrendPoint).Reverse().ToArray();
            PcsExportTrendSeries = BuildTrendSeries("Export Δ", pcsPoints, r => r.ExportDelta);
            PcsImportTrendSeries = BuildTrendSeries("Import Δ", pcsPoints, r => r.ImportDelta);
            BmsTrendSeries = BuildTrendSeries("BMS Trend", BmsRows.Where(x => x.IsPayloadTrendPoint).Reverse().ToArray(), r => r.ChartValue);
            PcsTrendDefaultVisibleRange = BuildTrendDefaultVisibleRange(PcsRows, _pcsTrendInterval);
            BmsTrendDefaultVisibleRange = BuildTrendDefaultVisibleRange(BmsRows, _bmsTrendInterval);
        }

        // Trend의 기본(초기화) 가시 범위: 전체 데이터 범위(첫 버킷 ~ 마지막 버킷)를 보여준다.
        // 세부 구간은 마우스 드래그(RubberBand)로 확대하고, 더블클릭으로 이 전체 범위로 되돌린다.
        private static DateRange BuildTrendDefaultVisibleRange(
            ObservableCollection<HistoryDataRow> rows,
            TimeSpan interval)
        {
            var times = rows
                .Where(x => x.IsPayloadTrendPoint && x.OccurredAt != DateTime.MinValue)
                .Select(x => x.OccurredAt)
                .OrderBy(t => t)
                .ToList();

            if (times.Count == 0)
                return new DateRange(DateTime.Now - interval, DateTime.Now);

            var start = times[0];
            var end = times[times.Count - 1].Add(interval);
            return new DateRange(start, end);
        }

        private static XyDataSeries<DateTime, double> CreateTrendSeries(string name)
        {
            return new XyDataSeries<DateTime, double> { SeriesName = name };
        }

        private static XyDataSeries<DateTime, double> BuildTrendSeries(
            string name,
            HistoryDataRow[] rows,
            Func<HistoryDataRow, double> valueSelector)
        {
            var series = CreateTrendSeries(name);
            if (rows == null || rows.Length == 0)
                return series;

            foreach (var row in rows)
            {
                if (row.OccurredAt == DateTime.MinValue)
                    continue;

                series.Append(row.OccurredAt, valueSelector(row));
            }

            return series;
        }

        private string BuildTrendXAxisTextFormatting()
        {
            var days = Math.Max(1, (EndDate.Date - StartDate.Date).Days + 1);
            return days > 1 ? "MM-dd" : "HH:mm";
        }

        private string BuildPeriodText()
        {
            var days = Math.Max(1, (EndDate.Date - StartDate.Date).Days + 1);
            return days == 1 ? "1 Day" : $"{days} Days";
        }

        private bool ValidateSearchPeriod()
        {
            if (StartDate.Date > EndDate.Date)
            {
                StatusMessage = "History load failed: Start date must be before end date.";
                return false;
            }

            var days = (EndDate.Date - StartDate.Date).Days + 1;
            if (days > MaxHistoryDays)
            {
                StatusMessage = $"History load failed: 조회 기간은 최대 {MaxHistoryDays}일까지 가능합니다.";
                return false;
            }

            return true;
        }

        private DateTime QueryStartDate()
        {
            return StartDate.Date;
        }

        private DateTime QueryEndDate()
        {
            return EndDate.Date.AddDays(1).AddTicks(-1);
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
        public DateTime OccurredAt { get; set; }
        public bool IsPayloadTrendPoint { get; set; }
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
        public double ExportDelta { get; set; }
        public double ImportDelta { get; set; }
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
