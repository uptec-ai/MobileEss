using DevExpress.Mvvm;
using System;
using EMS_PJT_Hamburger.Behaviors;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class DashBoardViewModel : ViewModelBase
    {
        public App app = Application.Current as App;

        public ICommand Cmd_Test { get; set; }
        

        public DashBoardViewModel()
        {
            FillVariable();
        }

        void FillVariable()
        {
            app.EMSStatusManager.DashboardStatus.VihicleStatus = "OPERATING";
            app.EMSStatusManager.DashboardStatus.VihicleColor = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#2D8F75"), 0.0),
                new GradientStop((Color)ColorConverter.ConvertFromString("#1E7154"), 0.5),
                new GradientStop((Color)ColorConverter.ConvertFromString("#155B43"), 1.0),
            }, new Point(0, 0), new Point(0, 1));

            app.EMSStatusManager.DashboardStatus.Power = 128;
            app.EMSStatusManager.DashboardStatus.SOC = 84;
            app.EMSStatusManager.DashboardStatus.BatteryVoltage = 714;
            app.EMSStatusManager.DashboardStatus.CommunicationStatus = "NORMAL";

            //PCS
            app.EMSStatusManager.PcsStatus.SetPower = 150;
            app.EMSStatusManager.PcsStatus.PresentPower = 123;
            app.EMSStatusManager.PcsStatus.PresentCurrent = 309;
            app.EMSStatusManager.PcsStatus.PresentDCVoltage = 714;
            app.EMSStatusManager.PcsStatus.Temperature = 45;
            app.EMSStatusManager.PcsStatus.ErrorCode = "None";

            // System & Communication status
            app.EMSStatusManager.SystemComm.StateOfCharge = 84;
            app.EMSStatusManager.SystemComm.StateOfHealth = 95;
            app.EMSStatusManager.SystemComm.CellTemperatureRange = "24℃ - 30℃";

            // Ess Status
            app.EMSStatusManager.EssStatus.CumulativeCharge = 225;
            app.EMSStatusManager.EssStatus.CumulativeDischarge = 310;


        }
    }
}
