using DevExpress.Mvvm;
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
    public class HomeViewModel : HomeModel
    {
        public HomeViewModel()
        {
            StartLoop();
        }
        public void StartLoop()
        {
            if (_isLoopRunning) return;
            _loopCts = new CancellationTokenSource();
            _isLoopRunning = true;
            _ = WaitChangeAsync(_loopCts.Token); // fire-and-forget
        }

        public void StopLoop()
        {
            if (!_isLoopRunning) return;
            _loopCts.Cancel();
            _isLoopRunning = false;
        }
        public async Task WaitChangeAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                //await Task.Delay(2000);
                //IsChargingPCS = !IsChargingPCS;
                await Task.Delay(2000, ct);
                Application.Current.Dispatcher.Invoke(() =>
                {

                    IsChargingEnergy = !IsChargingEnergy;
                    IsChargingPCS1 = !IsChargingPCS1;
                    IsChargingBMS = !IsChargingBMS;
                    Charge_Status_BMS = IsChargingBMS ? Brushes.GreenYellow : Brushes.Gray;
                });

                //await Task.Delay(2000);
                //IsChargingBMS = !IsChargingBMS;

                await Task.Delay(2000, ct);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsDischargingPCS2 = !IsDischargingPCS2;
                    IsDischargingTransformer = !IsDischargingTransformer;
                    if (Flag)
                    {
                        if (IsDischargingOffGrid) Flag = false;
                        IsDischargingOffGrid = !IsDischargingOffGrid;
                    }
                    else
                    {
                        if (IsDischargingVihicle) Flag = true;
                        IsDischargingVihicle = !IsDischargingVihicle;
                    }
                    Discharge_Status_BMS = IsDischargingPCS2 ? Brushes.Orange : Brushes.Gray;
                });
            }
        }
    }
}
