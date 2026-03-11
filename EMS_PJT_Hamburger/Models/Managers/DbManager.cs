using DevExpress.Mvvm;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        private string strConn = @"
                        HOST=localhost;
                        PORT=5432;
                        USERNAME=postgres;
                        PASSWORD=12345678;
                        DATABASE=DB_BMS_TEST;";
        public void ExecuteNonQuery(string sql)
        {
            try
            {
                using (var conn = new NpgsqlConnection(strConn))
                {
                    conn.Open();
                    using (var dataand = new NpgsqlCommand(sql, conn))
                    {
                        dataand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("==============================================================================================");
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString());
                System.Diagnostics.Debug.WriteLine("Message : " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Trace   : " + ex.StackTrace + ex.StackTrace.Substring(ex.StackTrace.LastIndexOf(' ')));
                System.Diagnostics.Debug.WriteLine("==============================================================================================");
            }
        }
        public DataSet GetDataSetByQuery(string sql)
        {
            DataSet ds = new DataSet();
            try
            {
                using (var conn = new NpgsqlConnection(strConn))
                {
                    using (var adpt = new NpgsqlDataAdapter(sql, conn))
                    {
                        adpt.Fill(ds);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("==============================================================================================");
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString());
                System.Diagnostics.Debug.WriteLine("Message : " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Trace   : " + ex.StackTrace + ex.StackTrace.Substring(ex.StackTrace.LastIndexOf(' ')));
                System.Diagnostics.Debug.WriteLine("==============================================================================================");
            }
            return ds;
        }

        public void DeleteTableData(string tableName)
        {
            string sql = $"TRUNCATE {tableName} RESTART IDENTITY";
            
            ExecuteNonQuery(sql);
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
            string sql = string.Empty;
            switch (set)
            {
                case 0: // bms 동작/중지 명령시 사용
                    sql = $"insert into tb_bms(status, total_curr, total_volt, mbms_state, disp_soc) values({data.Status}, {data.TotalCurrent}, {data.TotalVoltage}, {data.MBMS_State}, {data.DisplaySOC})";
                    break;
                case 1: // snapshot 주기마다 실행
                    sql = $"insert into tb_bms(total_curr, total_volt, mbms_state, disp_soc) values({data.TotalCurrent}, {data.TotalVoltage}, {data.MBMS_State}, {data.DisplaySOC})";
                    break;
            }
            ExecuteNonQuery(sql);
        }

        public void InsertBmsAlarmData((int code, string name)fault, int set)
        {
            string sql = string.Empty;
            switch (set)
            {
                case 0: // bms 알람 발생시 실행
                    sql = $"insert into tb_bms_alarm(alarm_code, alarm_name) values({fault.code}, '{fault.name}')";
                    break;
            }
            ExecuteNonQuery(sql);
        }

        public DataSet SelectBmsAlarmData(int set, int cnt)
        {
            DataSet ds = new DataSet();
            try
            {
                string sql = string.Empty;
                switch (set)
                {
                    case 0:
                        sql = $"select * from tb_bms_alarm order by occurred_at desc";
                        break;
                    case 1:
                        sql = $"select * from tb_bms_alarm order by occurred_at desc limit {cnt}";
                        break;
                    case 2:
                        sql = $"select * from tb_bms_alarm order by occurred_at desc limit {cnt}";
                        break;
                }
                ds = GetDataSetByQuery(sql);
                return ds;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ds;
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
