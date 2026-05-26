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
        public bool IsTouchKeyboardEnabled
        {
            get => GetProperty(() => IsTouchKeyboardEnabled);
            set
            {
                SetProperty(() => IsTouchKeyboardEnabled, value);
                TouchKeyboardService.SetEnabled(value);
            }
        }
        public HomeViewModel()
        {
            IsTouchKeyboardEnabled = TouchKeyboardService.IsEnabled;
            StartLoop();
        }
        public void StartLoop()
        {
            if (!_isLoopRunning) _isLoopRunning = true;
            if (_loopCts == null) _loopCts = new CancellationTokenSource();
            
            _ = WaitChangeAsync(_loopCts.Token); // fire-and-forget
            //_ = ConnectDataAsync(_loopCts.Token);
        }
        public void StopLoop()
        {
            if (!_isLoopRunning) return;
            _loopCts?.Cancel();     // 진행 작업에 '중단 요청' 신호 - 정리
            _loopCts?.Dispose();    // 내부 리소스 해제 - dispose
            _loopCts = null;        // null 
            _isLoopRunning = false;
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
                            // charge on
                            ChargingStatus = HomeStatus.Charging;
                            LoadTarget = LoadStatus.Waiting;

                            PcsBorderBrush = Brushes.Lime;
                            BmsBorderBrush = Brushes.Lime;
                            PChargeBorderBrush = Brushes.Lime; // PCS charge ellipse
                            BChargeBorderBrush = Brushes.Lime; // BMS charge ellipse
                            OperationModeBrush = Brushes.Lime;

                            ChargeOffGrid = Brushes.Gray;
                            ChargeVihicle = Brushes.Gray;
                            PDischargeBorderBrush = Brushes.Gray; // PCS Discharge ellipse
                            BDischargeBorderBrush = Brushes.Gray; // BMS discharge ellipse

                            emsMode = 2;
                            await Task.Delay(5000, ct);
                            break;
                        case 1: // discharge mode
                            // charge off
                            ChargingStatus = HomeStatus.Discharging;

                            PcsBorderBrush = Brushes.Lime;
                            BmsBorderBrush = Brushes.Orange;
                            PChargeBorderBrush = Brushes.Gray; // PCS charge ellipse
                            BChargeBorderBrush = Brushes.Gray; // BMS charge ellipse
                            OperationModeBrush = Brushes.Orange;

                            // discharge on
                            Random rand = new Random();
                            var random = rand.Next(0, 3);
                            if (random == 0)
                            {
                                LoadTarget = LoadStatus.OnGrid;
                            }
                            else if(random == 1)
                            {
                                LoadTarget = LoadStatus.OffGrid;
                            }
                            else
                            {
                                LoadTarget = LoadStatus.Vehicle;
                            }
                            ChargeOnGrid = (LoadTarget == LoadStatus.OnGrid) ? Brushes.Orange : Brushes.Gray;
                            ChargeOffGrid = (LoadTarget == LoadStatus.OffGrid) ? Brushes.Orange : Brushes.Gray;
                            ChargeVihicle = (LoadTarget == LoadStatus.Vehicle) ? Brushes.Orange : Brushes.Gray;

                            PDischargeBorderBrush = Brushes.Orange; // PCS Discharge ellipse
                            BDischargeBorderBrush = Brushes.Orange; // BMS discharge ellipse

                            emsMode = 0;
                            await Task.Delay(5000, ct);
                            break;
                        case 2: // none
                            // charge off
                            ChargingStatus = HomeStatus.Waiting;
                            LoadTarget = LoadStatus.Waiting;

                            PcsBorderBrush = Brushes.Gray;
                            BmsBorderBrush = Brushes.Gray;
                            PChargeBorderBrush = Brushes.Gray; // PCS charge ellipse
                            BChargeBorderBrush = Brushes.Gray; // BMS charge ellipse
                            OperationModeBrush = Brushes.Gray;

                            // discharge off
                            ChargeOffGrid = Brushes.Gray;
                            ChargeVihicle = Brushes.Gray;
                            PDischargeBorderBrush = Brushes.Gray; // PCS Discharge ellipse
                            BDischargeBorderBrush = Brushes.Gray; // BMS discharge ellipse

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
