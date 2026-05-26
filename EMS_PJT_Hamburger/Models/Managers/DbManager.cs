using DevExpress.Mvvm;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Diagnostics;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public class DbManager
    {
        #region Database Connect

        private static readonly HashSet<string> TruncateWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "tb_bms",
            "tb_bms_alarm",
            "tb_ems_alarm"
        };

        private readonly string _connectionString;

        public DbManager()
        {
            var fromEnv = Environment.GetEnvironmentVariable("EMS_DB_CONN");
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                _connectionString = fromEnv.Trim();
            }
            else
            {
                _connectionString = ConfigurationManager.ConnectionStrings["EMS_DB"]?.ConnectionString;
            }
        }

        private NpgsqlConnection CreateConnection()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("DB connection string is not configured. Set EMS_DB_CONN or App.config connectionStrings:EMS_DB.");

            return new NpgsqlConnection(_connectionString);
        }

        private static void LogDbError(Exception ex)
        {
            Debug.WriteLine("==============================================================================================");
            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Debug.WriteLine(ex.ToString());
            Debug.WriteLine("==============================================================================================");
        }

        public void ExecuteNonQuery(string sql)
            => ExecuteNonQuery(sql, null);

        public void ExecuteNonQuery(string sql, Action<NpgsqlCommand> bindParameters)
        {
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        bindParameters?.Invoke(command);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                LogDbError(ex);
            }
        }

        public DataSet GetDataSetByQuery(string sql)
            => GetDataSetByQuery(sql, null);

        public DataSet GetDataSetByQuery(string sql, Action<NpgsqlCommand> bindParameters)
        {
            DataSet ds = new DataSet();
            try
            {
                using (var conn = CreateConnection())
                {
                    conn.Open();
                    using (var command = new NpgsqlCommand(sql, conn))
                    {
                        bindParameters?.Invoke(command);
                        using (var adpt = new NpgsqlDataAdapter(command))
                        {
                            adpt.Fill(ds);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogDbError(ex);
            }

            return ds;
        }

        public void DeleteTableData(string tableName)
        {
            if (!TruncateWhitelist.Contains(tableName))
                throw new ArgumentException($"Table name '{tableName}' is not allowed.", nameof(tableName));

            string sql = $"TRUNCATE {tableName} RESTART IDENTITY";

            ExecuteNonQuery(sql);
        }

        private static int ToInt(string value, int defaultValue = 0)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : defaultValue;

        private static double ToDouble(string value, double defaultValue = 0)
        {
            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var invariantParsed))
                return invariantParsed;

            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out var currentCultureParsed))
                return currentCultureParsed;

            return defaultValue;
        }

        #endregion

        #region ■■■■■■■■■ BMS ■■■■■■■■■

        public class BmsData : ViewModelBase
        {
            public string Status
            {
                get => GetProperty(() => Status);
                set => SetProperty(() => Status, value);
            }

            public string TotalCurrent
            {
                get => GetProperty(() => TotalCurrent);
                set => SetProperty(() => TotalCurrent, value);
            }
            public string TotalVoltage
            {
                get => GetProperty(() => TotalVoltage);
                set => SetProperty(() => TotalVoltage, value);
            }
            public string MBMS_State
            {
                get => GetProperty(() => MBMS_State);
                set => SetProperty(() => MBMS_State, value);
            }
            public string DisplaySOC
            {
                get => GetProperty(() => DisplaySOC);
                set => SetProperty(() => DisplaySOC, value);
            }
        }

        public void InsertBmsData(BmsData data, int set)
        {
            switch (set)
            {
                case 0: // bms 동작/중지 명령시 사용
                    ExecuteNonQuery(
                        "insert into tb_bms(status, total_curr, total_volt, mbms_state, disp_soc) values(@status, @curr, @volt, @state, @soc)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@status", ToInt(data.Status));
                            cmd.Parameters.AddWithValue("@curr", ToDouble(data.TotalCurrent));
                            cmd.Parameters.AddWithValue("@volt", ToDouble(data.TotalVoltage));
                            cmd.Parameters.AddWithValue("@state", ToInt(data.MBMS_State));
                            cmd.Parameters.AddWithValue("@soc", ToDouble(data.DisplaySOC));
                        });
                    break;

                case 1: // snapshot 주기마다 실행
                    ExecuteNonQuery(
                        "insert into tb_bms(total_curr, total_volt, mbms_state, disp_soc) values(@curr, @volt, @state, @soc)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@curr", ToDouble(data.TotalCurrent));
                            cmd.Parameters.AddWithValue("@volt", ToDouble(data.TotalVoltage));
                            cmd.Parameters.AddWithValue("@state", ToInt(data.MBMS_State));
                            cmd.Parameters.AddWithValue("@soc", ToDouble(data.DisplaySOC));
                        });
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported BMS data insert mode.");
            }
        }

        public void InsertBmsAlarmData((int code, string name)fault, int set)
        {
            switch (set)
            {
                case 0: // bms 알람 발생시 실행
                    ExecuteNonQuery(
                        "insert into tb_bms_alarm(alarm_code, alarm_name) values(@code, @name)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@code", fault.code);
                            cmd.Parameters.AddWithValue("@name", fault.name ?? string.Empty);
                        });
                    InsertEmsAlarmData("BMS", "BMS", fault.code, fault.code, fault.name, fault.name, string.Empty, string.Empty, DateTime.Now, false);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported BMS alarm insert mode.");
            }
        }

        public DataSet SelectBmsAlarmData(int set, int cnt)
        {
            switch (set)
            {
                case 0:
                    return GetDataSetByQuery("select * from tb_bms_alarm order by occurred_at desc");

                case 1:
                case 2:
                    return GetDataSetByQuery(
                        "select * from tb_bms_alarm order by occurred_at desc limit @cnt",
                        cmd => cmd.Parameters.AddWithValue("@cnt", Math.Max(0, cnt)));

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported BMS alarm select mode.");
            }
        }

        public void EnsureEmsAlarmTable()
        {
            ExecuteNonQuery(@"
create table if not exists public.tb_ems_alarm
(
    alarm_id bigserial primary key,
    occurred_at timestamp without time zone not null default now(),
    source varchar(16) not null,
    category varchar(64) null,
    bit integer null,
    alarm_code integer null,
    alarm_name text null,
    fault_message text null,
    raw_value text null,
    duration_hour text null,
    reset_at timestamp without time zone null,
    is_reset boolean not null default false
);

create index if not exists ix_tb_ems_alarm_source_time
    on public.tb_ems_alarm(source, occurred_at desc);

create index if not exists ix_tb_ems_alarm_source_reset
    on public.tb_ems_alarm(source, is_reset, occurred_at desc);");
        }

        public void InsertEmsAlarmData(
            string source,
            string category,
            int bit,
            int alarmCode,
            string alarmName,
            string faultMessage,
            string rawValue,
            string durationHour,
            DateTime occurredAt,
            bool isReset)
        {
            EnsureEmsAlarmTable();

            ExecuteNonQuery(@"
insert into public.tb_ems_alarm
(
    occurred_at,
    source,
    category,
    bit,
    alarm_code,
    alarm_name,
    fault_message,
    raw_value,
    duration_hour,
    is_reset
)
values
(
    @occurred_at,
    @source,
    @category,
    @bit,
    @alarm_code,
    @alarm_name,
    @fault_message,
    @raw_value,
    @duration_hour,
    @is_reset
);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@occurred_at", occurredAt == default(DateTime) ? DateTime.Now : occurredAt);
                    cmd.Parameters.AddWithValue("@source", source ?? string.Empty);
                    cmd.Parameters.AddWithValue("@category", (object)category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@bit", bit);
                    cmd.Parameters.AddWithValue("@alarm_code", alarmCode);
                    cmd.Parameters.AddWithValue("@alarm_name", (object)alarmName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fault_message", (object)faultMessage ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@raw_value", (object)rawValue ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@duration_hour", (object)durationHour ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@is_reset", isReset);
                });
        }

        public DataSet SelectEmsAlarmData(string source, int set, int cnt)
        {
            EnsureEmsAlarmTable();

            switch (set)
            {
                case 0:
                    return GetDataSetByQuery(
                        "select * from public.tb_ems_alarm where source = @source order by occurred_at desc",
                        cmd => cmd.Parameters.AddWithValue("@source", source ?? string.Empty));

                case 1:
                    return GetDataSetByQuery(
                        "select * from public.tb_ems_alarm where source = @source and is_reset = false order by occurred_at desc",
                        cmd => cmd.Parameters.AddWithValue("@source", source ?? string.Empty));

                case 2:
                    return GetDataSetByQuery(
                        "select * from public.tb_ems_alarm where source = @source order by occurred_at desc limit @cnt",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@source", source ?? string.Empty);
                            cmd.Parameters.AddWithValue("@cnt", Math.Max(0, cnt));
                        });

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported EMS alarm select mode.");
            }
        }

        #endregion

        #region ■■■■■■■■■ PCS ■■■■■■■■■

        public class PcsData : ViewModelBase
        {
            public string Status
            {
                get => GetProperty(() => Status);
                set => SetProperty(() => Status, value);
            }

            public string TotalCurrent
            {
                get => GetProperty(() => TotalCurrent);
                set => SetProperty(() => TotalCurrent, value);
            }
            public string TotalVoltage
            {
                get => GetProperty(() => TotalVoltage);
                set => SetProperty(() => TotalVoltage, value);
            }
            public string MBMS_State
            {
                get => GetProperty(() => MBMS_State);
                set => SetProperty(() => MBMS_State, value);
            }
            public string DisplaySOC
            {
                get => GetProperty(() => DisplaySOC);
                set => SetProperty(() => DisplaySOC, value);
            }
        }

        public void InsertPmsData(BmsData data, int set)
        {
            switch (set)
            {
                case 0: // bms 동작/중지 명령시 사용
                    ExecuteNonQuery(
                        "insert into tb_bms(status, total_curr, total_volt, mbms_state, disp_soc) values(@status, @curr, @volt, @state, @soc)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@status", ToInt(data.Status));
                            cmd.Parameters.AddWithValue("@curr", ToDouble(data.TotalCurrent));
                            cmd.Parameters.AddWithValue("@volt", ToDouble(data.TotalVoltage));
                            cmd.Parameters.AddWithValue("@state", ToInt(data.MBMS_State));
                            cmd.Parameters.AddWithValue("@soc", ToDouble(data.DisplaySOC));
                        });
                    break;

                case 1: // snapshot 주기마다 실행
                    ExecuteNonQuery(
                        "insert into tb_bms(total_curr, total_volt, mbms_state, disp_soc) values(@curr, @volt, @state, @soc)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@curr", ToDouble(data.TotalCurrent));
                            cmd.Parameters.AddWithValue("@volt", ToDouble(data.TotalVoltage));
                            cmd.Parameters.AddWithValue("@state", ToInt(data.MBMS_State));
                            cmd.Parameters.AddWithValue("@soc", ToDouble(data.DisplaySOC));
                        });
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported BMS data insert mode.");
            }
        }

        public void UpsertPcsGridDailyTotals(double totalImported, double totalExported, DateTime collectedAt)
        {
            var day = collectedAt.Date;

            ExecuteNonQuery(@"
with updated as
(
    update public.tb_pcs_grid
    set total_imported = @total_imported,
        total_exported = @total_exported,
        collected_at = @collected_at
    where collected_at >= @day
      and collected_at < @next_day
    returning gridid
)
insert into public.tb_pcs_grid
(
    total_imported,
    total_exported,
    collected_at
)
select
    @total_imported,
    @total_exported,
    @collected_at
where not exists (select 1 from updated);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@total_imported", totalImported);
                    cmd.Parameters.AddWithValue("@total_exported", totalExported);
                    cmd.Parameters.AddWithValue("@collected_at", collectedAt);
                    cmd.Parameters.AddWithValue("@day", day);
                    cmd.Parameters.AddWithValue("@next_day", day.AddDays(1));
                });
        }

        public void InsertPcsStatusSnapshot(
            int statusGrid,
            int statusInv,
            int statusBatt,
            int statusComm,
            int statusBypass,
            int faultGrid,
            int faultInv,
            int faultLoad,
            int faultComm,
            int fuseGrid,
            int fuseBatt,
            int fuseBypass,
            DateTime collectedAt)
        {
            ExecuteNonQuery(@"
insert into public.tb_pcs_status
(
    status_grid,
    status_inv,
    status_batt,
    status_comm,
    status_bypass,
    fault_grid,
    fault_inv,
    fault_load,
    fault_comm,
    fuse_grid,
    fuse_batt,
    fuse_bypass,
    collected_at
)
values
(
    @status_grid,
    @status_inv,
    @status_batt,
    @status_comm,
    @status_bypass,
    @fault_grid,
    @fault_inv,
    @fault_load,
    @fault_comm,
    @fuse_grid,
    @fuse_batt,
    @fuse_bypass,
    @collected_at
);",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@status_grid", statusGrid);
                    cmd.Parameters.AddWithValue("@status_inv", statusInv);
                    cmd.Parameters.AddWithValue("@status_batt", statusBatt);
                    cmd.Parameters.AddWithValue("@status_comm", statusComm);
                    cmd.Parameters.AddWithValue("@status_bypass", statusBypass);
                    cmd.Parameters.AddWithValue("@fault_grid", faultGrid);
                    cmd.Parameters.AddWithValue("@fault_inv", faultInv);
                    cmd.Parameters.AddWithValue("@fault_load", faultLoad);
                    cmd.Parameters.AddWithValue("@fault_comm", faultComm);
                    cmd.Parameters.AddWithValue("@fuse_grid", fuseGrid);
                    cmd.Parameters.AddWithValue("@fuse_batt", fuseBatt);
                    cmd.Parameters.AddWithValue("@fuse_bypass", fuseBypass);
                    cmd.Parameters.AddWithValue("@collected_at", collectedAt);
                });
        }

        public int ReadInt(Dictionary<string, object> parsed, string key, int defaultValue = 0)
        {
            if (parsed == null || !parsed.TryGetValue(key, out var raw) || raw == null)
                return defaultValue;

            try { return Convert.ToInt32(raw, CultureInfo.InvariantCulture); }
            catch { return defaultValue; }
        }

        public double ReadDouble(Dictionary<string, object> parsed, string key, double defaultValue = 0)
        {
            if (parsed == null || !parsed.TryGetValue(key, out var raw) || raw == null)
                return defaultValue;

            try { return Convert.ToDouble(raw, CultureInfo.InvariantCulture); }
            catch { return defaultValue; }
        }

        public void InsertPcsData(Dictionary<string, object> parsed)
        {
            if (parsed == null) throw new ArgumentNullException(nameof(parsed));

            using (var conn = CreateConnection())
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        long snapshotId;

                        using (var cmd = new NpgsqlCommand(@"
insert into tb_pcs_status 
values
(
    default, @grid_status, @pv_status, @inv_status, @batt_status, @load_status, @comm_status,
    @grid_fault, @pv_fault, @inv_fault, @batt_fault, @load_fault, @comm_fault,
    @grid_cb, @pv_cb, @batt_cb, @load_cb,
    @grid_fuse, @pv_fuse, @batt_fuse, @load_fuse,
    @grid_spd, @pv_spd, @batt_spd, @load_spd,
    @grid_sc, @pv_sc, @batt_sc, @load_sc
)
returning snapshot_id;", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@grid_status", ReadInt(parsed, "Grid_Status"));
                            cmd.Parameters.AddWithValue("@pv_status", ReadInt(parsed, "PV_Status"));
                            cmd.Parameters.AddWithValue("@inv_status", ReadInt(parsed, "INV_Status"));
                            cmd.Parameters.AddWithValue("@batt_status", ReadInt(parsed, "Batt_Status"));
                            cmd.Parameters.AddWithValue("@load_status", ReadInt(parsed, "Load_Status"));
                            cmd.Parameters.AddWithValue("@comm_status", ReadInt(parsed, "Comm_Status"));

                            cmd.Parameters.AddWithValue("@grid_fault", ReadInt(parsed, "Grid_Fault"));
                            cmd.Parameters.AddWithValue("@pv_fault", ReadInt(parsed, "PV_Fault"));
                            cmd.Parameters.AddWithValue("@inv_fault", ReadInt(parsed, "INV_Fault"));
                            cmd.Parameters.AddWithValue("@batt_fault", ReadInt(parsed, "Batt_Fault"));
                            cmd.Parameters.AddWithValue("@load_fault", ReadInt(parsed, "Load_Fault"));
                            cmd.Parameters.AddWithValue("@comm_fault", ReadInt(parsed, "Comm_Fault"));

                            cmd.Parameters.AddWithValue("@grid_cb", ReadInt(parsed, "Grid_CB"));
                            cmd.Parameters.AddWithValue("@pv_cb", ReadInt(parsed, "PV_CB"));
                            cmd.Parameters.AddWithValue("@batt_cb", ReadInt(parsed, "Batt_CB"));
                            cmd.Parameters.AddWithValue("@load_cb", ReadInt(parsed, "Load_CB"));

                            cmd.Parameters.AddWithValue("@grid_fuse", ReadInt(parsed, "Grid_Fuse"));
                            cmd.Parameters.AddWithValue("@pv_fuse", ReadInt(parsed, "PV_Fuse"));
                            cmd.Parameters.AddWithValue("@batt_fuse", ReadInt(parsed, "Batt_Fuse"));
                            cmd.Parameters.AddWithValue("@load_fuse", ReadInt(parsed, "Load_Fuse"));

                            cmd.Parameters.AddWithValue("@grid_spd", ReadInt(parsed, "Grid_SPD"));
                            cmd.Parameters.AddWithValue("@pv_spd", ReadInt(parsed, "PV_SPD"));
                            cmd.Parameters.AddWithValue("@batt_spd", ReadInt(parsed, "Batt_SPD"));
                            cmd.Parameters.AddWithValue("@load_spd", ReadInt(parsed, "Load_SPD"));

                            cmd.Parameters.AddWithValue("@grid_sc", ReadInt(parsed, "Grid_SC"));
                            cmd.Parameters.AddWithValue("@pv_sc", ReadInt(parsed, "PV_SC"));
                            cmd.Parameters.AddWithValue("@batt_sc", ReadInt(parsed, "Batt_SC"));
                            cmd.Parameters.AddWithValue("@load_sc", ReadInt(parsed, "Load_SC"));
                            snapshotId = Convert.ToInt64(cmd.ExecuteScalar());
                        }

                        using (var cmd = new NpgsqlCommand(@"
insert into tb_pcs_grid
values
(
    @snapshot_id,
    @grid_total_imported_energy, @grid_total_exported_energy,
    @grid_daily_imported_energy, @grid_daily_exported_energy,
    @grid_volt_ab, @grid_volt_bc, @grid_volt_ca,
    @grid_curr_ab, @grid_curr_bc, @grid_curr_ca,
    @grid_freq, @grid_pf
);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@snapshot_id", snapshotId);
                            cmd.Parameters.AddWithValue("@grid_total_imported_energy", ReadDouble(parsed, "Grid_Total_ImportedEnergy"));
                            cmd.Parameters.AddWithValue("@grid_total_exported_energy", ReadDouble(parsed, "Grid_Total_ExportedEnergy"));
                            cmd.Parameters.AddWithValue("@grid_daily_imported_energy", ReadDouble(parsed, "Grid_Daily_ImportedEnergy"));
                            cmd.Parameters.AddWithValue("@grid_daily_exported_energy", ReadDouble(parsed, "Grid_Daily_ExportedEnergy"));
                            cmd.Parameters.AddWithValue("@grid_volt_ab", ReadDouble(parsed, "Grid_Volt_AB"));
                            cmd.Parameters.AddWithValue("@grid_volt_bc", ReadDouble(parsed, "Grid_Volt_BC"));
                            cmd.Parameters.AddWithValue("@grid_volt_ca", ReadDouble(parsed, "Grid_Volt_CA"));
                            cmd.Parameters.AddWithValue("@grid_curr_ab", ReadDouble(parsed, "Grid_Curr_AB"));
                            cmd.Parameters.AddWithValue("@grid_curr_bc", ReadDouble(parsed, "Grid_Curr_BC"));
                            cmd.Parameters.AddWithValue("@grid_curr_ca", ReadDouble(parsed, "Grid_Curr_CA"));
                            cmd.Parameters.AddWithValue("@grid_freq", ReadDouble(parsed, "Grid_Freq"));
                            cmd.Parameters.AddWithValue("@grid_pf", ReadDouble(parsed, "Grid_PF"));
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(@"
insert into tb_pcs_load
(
    snapshot_id,
    load_total_energy, load_daily_energy,
    load_volt_ab, load_volt_bc, load_volt_ca,
    load_curr_ab, load_curr_bc, load_curr_ca,
    load_freq, load_pf
)
values
(
    @snapshot_id,
    @load_total_energy, @load_daily_energy,
    @load_volt_ab, @load_volt_bc, @load_volt_ca,
    @load_curr_ab, @load_curr_bc, @load_curr_ca,
    @load_freq, @load_pf
);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@snapshot_id", snapshotId);
                            cmd.Parameters.AddWithValue("@load_total_energy", ReadDouble(parsed, "Load_Total_Energy"));
                            cmd.Parameters.AddWithValue("@load_daily_energy", ReadDouble(parsed, "Load_Daily_Energy"));
                            cmd.Parameters.AddWithValue("@load_volt_ab", ReadDouble(parsed, "Load_Volt_AB"));
                            cmd.Parameters.AddWithValue("@load_volt_bc", ReadDouble(parsed, "Load_Volt_BC"));
                            cmd.Parameters.AddWithValue("@load_volt_ca", ReadDouble(parsed, "Load_Volt_CA"));
                            cmd.Parameters.AddWithValue("@load_curr_ab", ReadDouble(parsed, "Load_Curr_AB"));
                            cmd.Parameters.AddWithValue("@load_curr_bc", ReadDouble(parsed, "Load_Curr_BC"));
                            cmd.Parameters.AddWithValue("@load_curr_ca", ReadDouble(parsed, "Load_Curr_CA"));
                            cmd.Parameters.AddWithValue("@load_freq", ReadDouble(parsed, "Load_Freq"));
                            cmd.Parameters.AddWithValue("@load_pf", ReadDouble(parsed, "Load_PF"));
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(@"
insert into tb_pcs_etc
values
(
    @snapshot_id,
    @inv_ambient_temp01, @inv_ambient_temp02, @inv_ambient_temp03, @inv_ambient_temp04,
    @inv_heatsink_temp01, @inv_heatsink_temp02, @inv_heatsink_temp03, @inv_heatsink_temp04,
    @inv_heatsink_temp05, @inv_heatsink_temp06, @inv_heatsink_temp07, @inv_heatsink_temp08,
    @inv_igbt_temp01, @inv_igbt_temp02, @inv_igbt_temp03, @inv_igbt_temp04,
    @inv_igbt_temp05, @inv_igbt_temp06, @inv_igbt_temp07, @inv_igbt_temp08,
    @inv_igbt_temp09, @inv_igbt_temp10, @inv_igbt_temp11, @inv_igbt_temp12,
    @grid_sc_cnt, @pv_sc_cnt, @battery_sc_cnt, @load_sc_cnt, @heartbeat
);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@snapshot_id", snapshotId);

                            cmd.Parameters.AddWithValue("@inv_ambient_temp01", ReadDouble(parsed, "INV_Ambient_Temp01"));
                            cmd.Parameters.AddWithValue("@inv_ambient_temp02", ReadDouble(parsed, "INV_Ambient_Temp02"));
                            cmd.Parameters.AddWithValue("@inv_ambient_temp03", ReadDouble(parsed, "INV_Ambient_Temp03"));
                            cmd.Parameters.AddWithValue("@inv_ambient_temp04", ReadDouble(parsed, "INV_Ambient_Temp04"));

                            cmd.Parameters.AddWithValue("@inv_heatsink_temp01", ReadDouble(parsed, "INV_Heatsink_Temp01"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp02", ReadDouble(parsed, "INV_Heatsink_Temp02"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp03", ReadDouble(parsed, "INV_Heatsink_Temp03"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp04", ReadDouble(parsed, "INV_Heatsink_Temp04"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp05", ReadDouble(parsed, "INV_Heatsink_Temp05"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp06", ReadDouble(parsed, "INV_Heatsink_Temp06"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp07", ReadDouble(parsed, "INV_Heatsink_Temp07"));
                            cmd.Parameters.AddWithValue("@inv_heatsink_temp08", ReadDouble(parsed, "INV_Heatsink_Temp08"));

                            cmd.Parameters.AddWithValue("@inv_igbt_temp01", ReadDouble(parsed, "INV_IGBT_Temp01"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp02", ReadDouble(parsed, "INV_IGBT_Temp02"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp03", ReadDouble(parsed, "INV_IGBT_Temp03"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp04", ReadDouble(parsed, "INV_IGBT_Temp04"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp05", ReadDouble(parsed, "INV_IGBT_Temp05"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp06", ReadDouble(parsed, "INV_IGBT_Temp06"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp07", ReadDouble(parsed, "INV_IGBT_Temp07"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp08", ReadDouble(parsed, "INV_IGBT_Temp08"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp09", ReadDouble(parsed, "INV_IGBT_Temp09"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp10", ReadDouble(parsed, "INV_IGBT_Temp10"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp11", ReadDouble(parsed, "INV_IGBT_Temp11"));
                            cmd.Parameters.AddWithValue("@inv_igbt_temp12", ReadDouble(parsed, "INV_IGBT_Temp12"));

                            cmd.Parameters.AddWithValue("@grid_sc_cnt", ReadInt(parsed, "Grid_SC_Cnt"));
                            cmd.Parameters.AddWithValue("@pv_sc_cnt", ReadInt(parsed, "PV_SC_Cnt"));
                            cmd.Parameters.AddWithValue("@battery_sc_cnt", ReadInt(parsed, "Battery_SC_Cnt"));
                            cmd.Parameters.AddWithValue("@load_sc_cnt", ReadInt(parsed, "Load_SC_Cnt"));
                            cmd.Parameters.AddWithValue("@heartbeat", ReadInt(parsed, "Heartbeat"));
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        try { tx.Rollback(); } catch { }
                        LogDbError(ex);
                        throw;
                    }
                }
            }
        }

        public DataSet SelectPmsAlarmData(int set, int cnt)
        {
            switch (set)
            {
                case 0:
                    return GetDataSetByQuery("select * from tb_bms_alarm order by occurred_at desc");

                case 1:
                case 2:
                    return GetDataSetByQuery(
                        "select * from tb_bms_alarm order by occurred_at desc limit @cnt",
                        cmd => cmd.Parameters.AddWithValue("@cnt", Math.Max(0, cnt)));

                default:
                    throw new ArgumentOutOfRangeException(nameof(set), set, "Unsupported BMS alarm select mode.");
            }
        }

        #endregion



        #region ■■■■■■■■■ 데이터 검색 ■■■■■■■■■
        /*
        public void LoadHistoryEventData(string startTime, string endTime)
        {
            App app = Application.Current as App;
            try
            {
                ObservableCollection<HistoryEventData> tempEvent = new ObservableCollection<HistoryEventData>();
                ObservableCollection<HistoryChartData> tempChart = new ObservableCollection<HistoryChartData>();
                string sql = string.Empty;

                #region Using Modbus Code

                //if (app.ModbusClient.Connected)
                //{
                //    sql = $"select distinct on (O.operid) O.operid, O.ds, O.dd, O.val" +
                //          $"T.temp1, T.temp2, T.temp3, T.temp4, T.temp5, T.temp6, O.last_update " +
                //          $"from tb_operation O " +
                //          $"left join tb_temperature T on O.last_update = T.last_update " +
                //          $"where O.last_update >= '{startTime}' and O.last_update < '{endTime}'";
                //}
                //else
                //{

                //}

                #endregion

                sql = $"select * from tb_operation where last_update >= '{startTime}' and last_update < '{endTime}'";
                DataSet ds = GetDataSetByQuery(sql);

                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    HistoryEventData data = new HistoryEventData();
                    #region Using Modbus Code

                    //if (app.ModbusClient.Connected)
                    //{
                    //    data.IsSelected = false;
                    //    data.Index = int.Parse(ds.Tables[0].Rows[i][0].ToString());
                    //    data.StartLocation = int.Parse(ds.Tables[0].Rows[i][1].ToString());
                    //    data.EndLocation = int.Parse(ds.Tables[0].Rows[i][2].ToString());
                    //    data.Time = DateTime.Parse(ds.Tables[0].Rows[i][10].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                    //    data.Temp1 = ds.Tables[0].Rows[i][4].ToString();
                    //    data.Temp2 = ds.Tables[0].Rows[i][5].ToString();
                    //    data.Temp3 = ds.Tables[0].Rows[i][6].ToString();
                    //    data.Temp4 = ds.Tables[0].Rows[i][7].ToString();
                    //    data.Temp5 = ds.Tables[0].Rows[i][8].ToString();
                    //    data.Temp6 = ds.Tables[0].Rows[i][9].ToString();
                    //    data.Comment = string.Empty;
                    //}
                    //else
                    //{

                    //}

                    #endregion
                    data.IsSelected = false;
                    data.ID = i + 1;
                    data.Index = int.Parse(ds.Tables[0].Rows[i][0].ToString());
                    data.StartLocation = int.Parse(ds.Tables[0].Rows[i][1].ToString());
                    data.EndLocation = int.Parse(ds.Tables[0].Rows[i][2].ToString());
                    data.Time = DateTime.Parse(ds.Tables[0].Rows[i][6].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    data.Comment = string.Empty;


                    HistoryChartData chart = new HistoryChartData();

                    chart = new HistoryChartData
                    {
                        Value = Decompress(app.ConvertManager.HexToByte(ds.Tables[0].Rows[i][3].ToString())),
                        Temp = Decompress(app.ConvertManager.HexToByte(ds.Tables[0].Rows[i][4].ToString())),
                        DPP = Decompress(app.ConvertManager.HexToByte(ds.Tables[0].Rows[i][5].ToString())),
                    };

                    app.HistoryEventData.Add(data);
                    app.HistoryChartData.Add(chart);
                }

            }
            catch (Exception ex)
            {
                app.nlog.Error(message: $"==============================================================================================\r" +
                                        $"{ex.Message}" +
                                        $"{ex.StackTrace}{ex.StackTrace.Substring(ex.StackTrace.LastIndexOf(' '))}\r" +
                                        $"==============================================================================================");
            }
        }
        */

        #endregion

        #region ■■■■■■■■■ 데이터 압축 ■■■■■■■■■

        public Byte[] Compress(Byte[] buffer)    // 데이터 압축
        {
            Byte[] compressedByte;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(buffer, 0, buffer.Length);
                }

                compressedByte = ms.ToArray();
            }
            return compressedByte;
        }

        public Byte[] Decompress(Byte[] buffer)  // 데이터 압축 풀기
        {
            MemoryStream resultStream = new MemoryStream();

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    ds.CopyTo(resultStream);
                    ds.Close();
                }
            }
            Byte[] decompressedByte = resultStream.ToArray();
            resultStream.Dispose();

            return decompressedByte;
        }

        #endregion
    }
}
