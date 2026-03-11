using EasyModbus;
using EMS_PJT_Hamburger.Models.Client;
using EMS_PJT_Hamburger.Models.Client.BMS;
using EMS_PJT_Hamburger.Models.Managers;
using EMS_PJT_Hamburger.Views;
using NLog;
using SciChart.Charting.Visuals;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EMS_PJT_Hamburger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public HomeView HomeView { get; private set; }
        public DashBoardView DashBoardView { get; private set; }
        public PCSView PCSView { get; private set; }
        public BMSView BMSView { get; private set; }
        public SystemView SystemView { get; private set; }
        public HistoryView HistoryView { get; private set; }


        public EMSStatusManager EMSStatusManager { get; private set; } = new EMSStatusManager();
        public ConvertManager ConvertManager { get; private set; }
        public StatusManager StatusManager { get; private set; }
        public DbManager DbManager { get; private set; }

        public ModbusTcpConnectionService _pcsClient { get; set; }
        //public AlarmService AlarmService { get; set; } = new AlarmService();

        public readonly Logger nlog = LogManager.GetLogger("");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            nlog.Info($"EMS started.");
        }
        public App()
        {
            // kjy@up-tec.co.kr 라이센스
            SciChartSurface.SetRuntimeLicenseKey("oQqSSSFezvsiR7L83SsiuyoBu8UASGj8vmo0ktqoUc61utvVK/jS1fATkXCYmwrEko6hI54S1V/vnMOYCFDWxeLkwbQADkghe8//G+PUXWAdOR8AccZLWUVEIULeejsGsnFV/dcOmPvKLpzp4YeplOQuVUpZCSMZ24nO5BNku7ot2OeGrsUW2yMrmyTh7ksheKLoboRE1fw0bJ69njYSj6U+le6qDF7TXAueZs4Q6FrynbPblBtSNJTIlaNcVwWAiI/qwSor7Xw1f1+kB7nO5js7X2OzV4aPDopITSUL1T+EMdBp06RNdRG1ToNoSvmRMnCLGPDxVBj6fCDC68mhuntz0B6YoKMK/60fFYS5NxbrMcuxGtYg0cR19zH5OHTWW9hoY2OecYIPklWUBDn9eZSALf5czy6robZAle/nEOmCQ5zY7GVCJfnE7yHeD2Aj1t/jUm+1MiiAojZO+E+Kl3LTZjFW7369GJG4VrIJfA==");
            
            InitManagers();

            InitViews();
        }

        private void InitManagers()
        {
            ConvertManager = new ConvertManager();
            StatusManager = new StatusManager();
            StatusManager.Init();
            DbManager = new DbManager();

            // Communication Code
            //CommonSetDataModel = new CommonSetDataModel();
            //CommonSetDataModel.BMS_IP = "192.168.1.50";
            //CommonSetDataModel.BMS_Port = 5007;

            PCSEthernetClientSet();
            BMSEthernetClientSet();

        }

        private void BMSEthernetClientSet()
        {
            //if (_bmsClient != null) return;
            //_bmsClient = new ModbusCanConnectionService("172.30.1.47", 502, 1, 5000); // ip, port, unitId, timeOut            

            
        }

        private void InitViews()
        {
            HomeView = new HomeView();
            DashBoardView = new DashBoardView();
            PCSView = new PCSView();
            BMSView = new BMSView();
            SystemView = new SystemView();
            HistoryView = new HistoryView();
        }

        private void PCSEthernetClientSet()
        {
            _pcsClient = new ModbusTcpConnectionService();
        }

        // 저장
        public async void UpdatingVariableSavedSignal(int time)
        {
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.Saved;
            await Task.Delay(time);
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.None;
        }
        // 업데이트
        public async void UpdatingVariableUpdatedSignal(int time)
        {
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.Updated;
            await Task.Delay(time);
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.None;
        }
        // 업로드
        public async void UpdatingVariableUploadSignal(int time)
        {
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.Uploaded;
            await Task.Delay(time);
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.None;
        }
        // 에러코드  - 리셋
        public async void UpdatingVariableResetErrorCodeSignal(int time)
        {
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.ErrorCode;
            await Task.Delay(time);
            StatusManager.CurrentComm_Status = StatusManager.CommStatus.None;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Debug.WriteLine("App Exit Called");
            base.OnExit(e);
        }

    }
}
