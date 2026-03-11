using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Models.Client.BMS
{
    public class AlarmService
    {
        // 싱글톤 (App 종료까지 사라지지 않음.)
        //public static AlarmService Instance { get; } = new AlarmService();

        private readonly DispatcherTimer _timer;
        private int lastCnt = 0; // 마지막 조회 횟수

        public AlarmService()
        {
            //_timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            //_timer.Tick += (_, __) => Sync();
        }
        public void Start()
        {
            //_timer.Start();
        }
        public void Stop()
        {
            //_timer.Stop();
        }
        /*
        private async void Sync()
        {
            // 서버/DB 조회 후 Alarms 갱신 (예시)
            // Alarms.Add / Update ...
            await Application.Current.MainWindow.Dispatcher.BeginInvoke(new Action(() => SearchAlarmData()));
        }
        private void SearchAlarmData()
        {
            App app = Application.Current as App;
            string sql = $"select * from tb_bms_alarm order by occurred_at desc";
            DataSet ds = app.DbManager.GetDataSetByQuery(sql);
            if (ds.Tables[0].Rows.Count > lastCnt) lastCnt = ds.Tables[0].Rows.Count - lastCnt;
            else return;

            // 테스트 데이터
            for (int i = 0; i < lastCnt; i++)
            {
                Alarms.Add(new AlarmItems
                {
                    Code = Convert.ToInt32(ds.Tables[0].Rows[i][1].ToString()),
                    Alarm = ds.Tables[0].Rows[i][2].ToString(),
                    Hour = ds.Tables[0].Rows[i][3].ToString(),
                });
            }
        }
        */
    }

    public class AlarmItems : ViewModelBase
    {
        public int Code { get; set; } // 알람 Code
        public string Alarm { get; set; } // 알람 내용
        public string Hour { get; set; } // 알람 발생 시간

    }
}
