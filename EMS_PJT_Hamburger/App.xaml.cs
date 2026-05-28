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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Npgsql;
using System.Windows.Media;
using EMS_PJT_Hamburger.ViewModels;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Mvvm;

namespace EMS_PJT_Hamburger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public HomeView HomeView { get; private set; }
        //public DashBoardView DashBoardView { get; private set; }
        public PCSView PCSView { get; private set; }
        public BMSView BMSView { get; private set; }
        //public SystemView SystemView { get; private set; }
        public HistoryView HistoryView { get; private set; }

        public HomeViewModel HomeVm { get; private set; }
        //public DashBoardViewModel DashVm { get; private set; }
        public PcsViewModel PcsVm { get; private set; }
        public BMSViewModel BmsVm { get; private set; }


        public EMSStatusManager EMSStatusManager { get; private set; } = new EMSStatusManager();
        public ConvertManager ConvertManager { get; private set; }
        public StatusManager StatusManager { get; private set; }
        public DbManager DbManager { get; private set; }

        public readonly Logger nlog = LogManager.GetLogger("");

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                nlog.Info($"EMS started.");

                SplashScreenManager.Create(() => new FluentSplashScreen(), new DXSplashScreenViewModel
                {
                    IsIndeterminate = true,
                    Title = "EMS Application Started..",
                    Subtitle = "Version 1.0",
                    Logo = new Uri(string.Format(@"pack://application:,,,/Assets/upteclogowhite32.png", UriKind.Relative)),
                    Copyright = "UPTec\r\nwww.up-tec.co.kr",
                }).ShowOnStartup();
                //CredentialStore.Load();
            }
            catch (Exception ex)
            {
                LogFatalException("OnStartup", ex);
                ShowFatalMessage(ex);
                throw;
            }
        }
        public App()
        {
            RegisterGlobalExceptionHandlers();

            try
            {
                ConfigureSciChartLicense();
                
                InitManagers();

                InitViews();
            }
            catch (Exception ex)
            {
                LogFatalException("App constructor", ex);
                ShowFatalMessage(ex);
                throw;
            }
        }

        private void ConfigureSciChartLicense()
        {
            var keyFromEnv = Environment.GetEnvironmentVariable("EMS_SCICHART_LICENSE_KEY");

            if (string.IsNullOrWhiteSpace(keyFromEnv))
            {
                nlog.Warn("SciChart license key is not configured. Set EMS_SCICHART_LICENSE_KEY or App.config: SciChartLicenseKey.");
                return;
            }

            SciChartSurface.SetRuntimeLicenseKey(keyFromEnv);
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
            BMSEthernetClientSet();

            StatusManager.CurrentPCS_Status = StatusManager.PCSStatus.Connected;

        }

        private void BMSEthernetClientSet()
        {
            //if (_bmsClient != null) return;
            //_bmsClient = new ModbusCanConnectionService("172.30.1.47", 502, 1, 5000); // ip, port, unitId, timeOut            

            
        }

        private void InitViews()
        {
            PcsVm = new PcsViewModel();
            PCSView = new PCSView();
            PCSView.DataContext = PcsVm;

            BmsVm = new BMSViewModel();
            BMSView = new BMSView();
            BMSView.DataContext = BmsVm;

            HomeVm = new HomeViewModel();
            HomeView = new HomeView();
            HomeView.DataContext = HomeVm;

            //DashVm = new DashBoardViewModel();
            //DashBoardView = new DashBoardView();
            //DashBoardView.DataContext = DashVm;

            //SystemView = new SystemView();
            HistoryView = new HistoryView();
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

            DisposeViewDataContext(HomeView);
            //DisposeViewDataContext(DashBoardView);
            DisposeViewDataContext(PCSView);
            DisposeViewDataContext(BMSView);
            //DisposeViewDataContext(SystemView);
            DisposeViewDataContext(HistoryView);

            StatusManager?.Dispose();
            LogManager.Shutdown();

            base.OnExit(e);
        }

        private static void DisposeViewDataContext(FrameworkElement view)
        {
            if (view?.DataContext is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    // 종료 경로에서는 예외를 삼켜 앱 종료를 보장한다.
                }
            }
        }

        private void RegisterGlobalExceptionHandlers()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException; // UI 이벤트/XAML 로딩 중 예외 (버튼클릭, Loaded 이벤트, 바인딩 후 실현되는 UI코드 등..)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; // 앱 전체에서 최종 미처리 예외
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException; // await 안 한 Task 내부 예외
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogFatalException("DispatcherUnhandledException", e.Exception);
            ShowFatalMessage(e.Exception);
            e.Handled = true;
            Shutdown(-1);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception");
            LogFatalException("AppDomain.CurrentDomain.UnhandledException", ex);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogFatalException("TaskScheduler.UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private void LogFatalException(string source, Exception ex)
        {
            try
            {
                nlog.Error(ex, $"Fatal exception at {source}");
            }
            catch
            {
                // NLog 실패 시 아래 fallback 파일 기록으로 남긴다.
            }

            try
            {
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logDirectory);

                var logPath = Path.Combine(logDirectory, "fatal_startup.txt");
                var lines =
                    "==============================================================================================" + Environment.NewLine +
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{source}]" + Environment.NewLine +
                    ex + Environment.NewLine;

                File.AppendAllText(logPath, lines);
            }
            catch
            {
                // fallback 기록도 실패하면 더 이상 할 수 있는 작업이 없다.
            }
        }

        private static void ShowFatalMessage(Exception ex)
        {
            try
            {
                MessageBox.Show(
                    "프로그램 실행 중 치명적인 오류가 발생했습니다." + Environment.NewLine +
                    "logs\\fatal_startup.txt 파일을 확인해주세요." + Environment.NewLine + Environment.NewLine +
                    ex.Message,
                    "EMS 실행 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // MessageBox 표시 실패 시 무시한다.
            }
        }

    }
}

