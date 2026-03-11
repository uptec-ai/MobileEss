using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client.BMS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class AlarmDetailWindowViewModel : ViewModelBase
    {
        public readonly AlarmService _service;

        private int OccurredAlarmCnt { get; set; }
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
        {
            _service = service;
            OccurredAlarmCnt = cnt;

            Init();

            CommandInit();

            //LoadTestData();


        }
        private void Init()
        {
            Alarms = new ObservableCollection<AlarmItems>();
        }
        private void CommandInit()
        {
            Cmd_LoadAlarm = new DelegateCommand(() => LoadAlarm());
            Cmd_ExportAlarm = new DelegateCommand(() => ExportAlarm());
        }
        private void LoadAlarm()
        {
            Alarms.Clear();

            App app = Application.Current as App;
            DataSet ds = new DataSet();
            if (SelectAll)
                ds = app.DbManager.SelectBmsAlarmData(0, 0);
            else if (SelectTop100)
                ds = app.DbManager.SelectBmsAlarmData(1, 100);
            else if (SelectCurrent)
            {
                if (OccurredAlarmCnt == 0) return;
                ds = app.DbManager.SelectBmsAlarmData(2, OccurredAlarmCnt);
            }

            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                Alarms.Add(new AlarmItems
                {
                    Code = Convert.ToInt32(ds.Tables[0].Rows[i][1].ToString()),
                    Alarm = ds.Tables[0].Rows[i][2].ToString(),
                    Hour = ds.Tables[0].Rows[i][3].ToString(),
                });
            }
        }
        private void ExportAlarm()
        {

        }
    }
}
