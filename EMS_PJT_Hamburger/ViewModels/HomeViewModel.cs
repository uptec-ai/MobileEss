using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Behaviors;
using EMS_PJT_Hamburger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.ViewModels
{
    
    public class HomeViewModel : HomeModel, IDisposable
    {
        private bool _disposed;
        public DelegateCommand<LoadStatus> Cmd_SelectLoadTarget { get; private set; }
        public bool IsTouchKeyboardEnabled
        {
            get => GetProperty(() => IsTouchKeyboardEnabled);
            set
            {
                if (SetProperty(() => IsTouchKeyboardEnabled, value))
                {
                    TouchKeyboardService.SetEnabled(value);
                }
            }
        }
        public HomeViewModel()
        {
            TouchKeyboardService.SetEnabled(false);
            Cmd_SelectLoadTarget = new DelegateCommand<LoadStatus>(SelectLoadTarget);
            StartLoop();
        }
        public void StartLoop()
        {
            if (!_isLoopRunning) _isLoopRunning = true;
            if (_loopCts == null) _loopCts = new CancellationTokenSource();
            
            _ = SyncSystemModeAsync(_loopCts.Token); // fire-and-forget
        }
        public void StopLoop()
        {
            if (!_isLoopRunning) return;
            _loopCts?.Cancel();     // 진행 작업에 '중단 요청' 신호 - 정리
            _loopCts?.Dispose();    // 내부 리소스 해제 - dispose
            _loopCts = null;        // null 
            _isLoopRunning = false;
        }

        private async Task SyncSystemModeAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var app = Application.Current as App;
                    var pcsVm = app?.PcsVm;

                    if (pcsVm?.IsChargeModeActive == true)
                    {
                        ApplyChargingUi();
                    }
                    else if (pcsVm?.IsDischargeModeActive == true)
                    {
                        ApplyDischargingUi();
                    }
                    else
                    {
                        ApplyWaitingUi();
                    }

                    await Task.Delay(200, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            finally
            {
                _isLoopRunning = false;
            }
        }

        private void ApplyChargingUi()
        {
            ChargingStatus = HomeStatus.Charging;
            LoadTarget = LoadStatus.Waiting;
            CouplingStatus = false;

            PcsBorderBrush = Brushes.Lime;
            BmsBorderBrush = Brushes.Lime;
            PChargeBorderBrush = Brushes.Lime;
            BChargeBorderBrush = Brushes.Lime;
            OperationModeBrush = Brushes.Lime;

            UpdateLoadTargetUi(false);
            DischargeBorderBrush = Brushes.Gray;
            PDischargeBorderBrush = Brushes.Gray;
            BDischargeBorderBrush = Brushes.Gray;
        }

        private void ApplyDischargingUi()
        {
            ChargingStatus = HomeStatus.Discharging;

            PcsBorderBrush = Brushes.Orange;
            BmsBorderBrush = Brushes.Orange;
            DischargeBorderBrush = Brushes.Orange;
            PChargeBorderBrush = Brushes.Gray;
            BChargeBorderBrush = Brushes.Gray;
            OperationModeBrush = Brushes.Orange;

            CouplingStatus = true;
            UpdateLoadTargetUi(true);

            PDischargeBorderBrush = Brushes.Orange;
            BDischargeBorderBrush = Brushes.Orange;
        }

        private void ApplyWaitingUi()
        {
            ChargingStatus = HomeStatus.Waiting;
            LoadTarget = LoadStatus.Waiting;
            CouplingStatus = false;

            PcsBorderBrush = Brushes.Gray;
            BmsBorderBrush = Brushes.Gray;
            DischargeBorderBrush = Brushes.Gray;
            PChargeBorderBrush = Brushes.Gray;
            BChargeBorderBrush = Brushes.Gray;
            OperationModeBrush = Brushes.Gray;

            UpdateLoadTargetUi(false);
            PDischargeBorderBrush = Brushes.Gray;
            BDischargeBorderBrush = Brushes.Gray;
        }

        private void SelectLoadTarget(LoadStatus target)
        {
            if (target == LoadStatus.Waiting) return;

            SelectedLoadTarget = target;
            RaisePropertyChanged(nameof(SelectedLoadTarget));
            UpdateLoadTargetUi(ChargingStatus == HomeStatus.Discharging);
        }

        private void UpdateLoadTargetUi(bool isActive)
        {
            LoadTarget = isActive ? SelectedLoadTarget : LoadStatus.Waiting;
            ChargeOnGrid = (isActive && SelectedLoadTarget == LoadStatus.OnGrid) ? Brushes.Orange : Brushes.Gray;
            ChargeOffGrid = (isActive && SelectedLoadTarget == LoadStatus.OffGrid) ? Brushes.Orange : Brushes.Gray;
            ChargeVihicle = (isActive && SelectedLoadTarget == LoadStatus.Vehicle) ? Brushes.Orange : Brushes.Gray;
        }

        public async Task WaitChangeAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    switch (emsMode)
                    {
                        case 0: // charge mode
                            ApplyChargingUi();
                            emsMode = 2;
                            await Task.Delay(5000, ct);
                            break;
                        case 1: // discharge mode
                            ApplyDischargingUi();
                            emsMode = 0;
                            await Task.Delay(5000, ct);
                            break;
                        case 2: // none
                            ApplyWaitingUi();
                            emsMode = 1;
                            await Task.Delay(2000, ct);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            finally
            {
                _isLoopRunning = false;
            }
        }
        //public async Task ConnectDataAsync(CancellationToken ct)
        //{
        //    while (!ct.IsCancellationRequested)
        //    {
        //        App app = Application.Current as App;

        //        ConnectPCS = app.PcsVm.IsConnected ? "Enable" : "Disable";
        //        ConnectBMS = app.BmsVm.StatusMsg01.Ready == "Open" ? "Enable" : "Disable";

        //        await Task.Delay(200, ct);
        //    }
        //}

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopLoop();
        }
    }
}
