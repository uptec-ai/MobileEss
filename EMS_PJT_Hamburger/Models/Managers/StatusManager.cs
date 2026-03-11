using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public class StatusManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public PCSStatus CurrentPCS_Status { get; set; } // PCS Status
        public BMSStatus CurrentBMS_Status { get; set; } // BMS Status
        public CommStatus CurrentComm_Status { get; set; } // Common Status
        public SystemStatus CurrentSystem_Status { get; set; } // System Status

        #region # 변수 정의

        // 시스템 동작
        public enum SystemStatus
        {
            Start,
            Stop,
        }
        // PCS 접속 상태
        public enum PCSStatus
        {
            None,
            TryConnect, // 장비
            Connected,
            Disconnected,
            Error,
        }
        // BMS 접속 상태
        public enum BMSStatus
        {
            None,
            TryConnect, // 장비
            Connected,
            Disconnected,
            Error,
        }
        // 상태 이벤트 메세지(1초유지)
        public enum CommStatus
        {
            None,
            Downloading,
            DownloadError,
            Downloaded,
            Connected,
            Connecting,
            UserConnecting,
            DisConnecting,
            UserDisConnecting,
            Updated,
            UpdateError,
            Uploaded,
            Uploading,
            UploadError,
            TimeSync,
            TimeSyncComplete,
            TimeSyncError,
            Timeout,
            Saved,
            SavedError,
            ErrorCode,
        }

        #endregion

        #region < Use Variable >
        /// <summary>
        /// Public variable
        /// </summary>
        public string PCS { get; set; } // PCS 정보
        public string BMS { get; set; } // BMS 정보
        public bool KeepConnection { get; set; } = true;
        public string CommState { get; set; } // HMI 상태
        public string SystemState { get; set; } // 시스템 상태
        public string CurrentTime { get; set; } // 현재시간

        /// <summary>
        /// Private variable
        /// </summary>
        private DispatcherTimer dt_timer; // 현재시간 타이머 
        private int PCSErrorCounter = 0;

        #endregion

        public void Init() // 초기 실행 함수
        {
            dt_timer = new DispatcherTimer();
            dt_timer.Interval = new TimeSpan(0, 0, 1);  //1초간격 동작
            dt_timer.Tick += (s, e) =>

            {
                CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            };
            dt_timer.Start();

            PropertyChanged += StatusManager_PropertyChanged;
        }
        private void StatusManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentSystem_Status")
            {
                UpdateSystem_Status();
            }
            if (e.PropertyName == "CurrentComm_Status")
            {
                UpdateComm_Status();
            }

            if (e.PropertyName == "CurrentPCS_Status")
            {
                Update_PCS_Status();
            }

            if (e.PropertyName == "CurrentBMS_Status")
            {
                Update_BMS_Status();
            }
        }
        private void UpdateSystem_Status()     // 시스템 동작 관련 상태 메세지
        {
            switch (this.CurrentSystem_Status)
            {
                case SystemStatus.Start:
                    this.SystemState = Properties.Resources.App_Status_System_Start;
                    break;

                case SystemStatus.Stop:
                    this.SystemState = Properties.Resources.App_Status_System_Stop;
                    break;
            }
        }
        private void UpdateComm_Status()        // 통신 관련 상태 메세지
        {
            switch (this.CurrentComm_Status)
            {
                case CommStatus.None:
                    this.CommState = Properties.Resources.App_Status_Na;
                    break;
                case CommStatus.Downloading:
                    this.CommState = Properties.Resources.App_Status_Downloading;
                    break;
                case CommStatus.DownloadError:
                    this.CommState = Properties.Resources.App_Status_Download_Error;
                    break;
                case CommStatus.Downloaded:
                    this.CommState = Properties.Resources.App_Status_Downloaded;
                    break;
                case CommStatus.Connected:
                    this.CommState = Properties.Resources.App_Status_Connected;
                    break;
                case CommStatus.Connecting:
                    this.CommState = Properties.Resources.App_Status_Connected;
                    break;
                case CommStatus.DisConnecting:
                    this.CommState = Properties.Resources.App_Status_Connected;
                    break;
                case CommStatus.Updated:
                    this.CommState = Properties.Resources.App_Status_Updated;
                    break;
                case CommStatus.UpdateError:
                    this.CommState = Properties.Resources.App_Status_Update_Error;
                    break;
                case CommStatus.Uploaded:
                    this.CommState = Properties.Resources.App_Status_Uploaded;
                    break;
                case CommStatus.Uploading:
                    this.CommState = Properties.Resources.App_Status_Uploading;
                    break;
                case CommStatus.UploadError:
                    this.CommState = Properties.Resources.App_Status_Upload_Error;
                    break;
                case CommStatus.TimeSync:
                    this.CommState = Properties.Resources.App_Status_TimeSync;
                    break;
                case CommStatus.TimeSyncComplete:
                    this.CommState = Properties.Resources.App_Status_TimeSyncComplete;
                    break;
                case CommStatus.TimeSyncError:
                    this.CommState = Properties.Resources.App_Status_TimeSyncError;
                    break;
                case CommStatus.Timeout:
                    this.CommState = Properties.Resources.App_Status_Timeout;
                    break;
                case CommStatus.Saved:
                    this.CommState = Properties.Resources.App_Status_Saved;
                    break;
                case CommStatus.SavedError:
                    this.CommState = Properties.Resources.App_Status_SavedError;
                    break;
            }
        }
        private void Update_PCS_Status()    // PCS 관련 상태 메세지
        {
            switch (this.CurrentPCS_Status)
            {
                case PCSStatus.None:
                    this.PCS = "N/A";
                    break;
                case PCSStatus.TryConnect:
                    this.PCS = Properties.Resources.App_Status_PCS_TryConnect;
                    break;
                case PCSStatus.Connected:
                    this.PCS = Properties.Resources.App_Status_PCS_Connected;
                    this.PCSErrorCounter = 0;
                    break;
                case PCSStatus.Disconnected:
                    this.PCS = Properties.Resources.App_Status_PCS_Disconnected;
                    this.PCSErrorCounter++;
                    break;
                case PCSStatus.Error:
                    this.PCS = Properties.Resources.App_Status_PCS_Error;
                    this.PCSErrorCounter++;
                    break;
            }

        }
        private void Update_BMS_Status()    // BMS 관련 상태 메세지
        {
            switch (this.CurrentBMS_Status)
            {
                case BMSStatus.None:
                    this.BMS = "N/A";
                    break;
                case BMSStatus.TryConnect:
                    this.BMS = Properties.Resources.App_Status_BMS_TryConnect;
                    break;
                case BMSStatus.Connected:
                    this.BMS = Properties.Resources.App_Status_BMS_Connected;
                    break;
                case BMSStatus.Disconnected:
                    this.BMS = Properties.Resources.App_Status_BMS_Disconnected;
                    break;
                case BMSStatus.Error:
                    this.BMS = Properties.Resources.App_Status_BMS_Error;
                    break;
            }
        }
    }
}
