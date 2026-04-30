using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public class EMSStatusManager : BindableBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public DashboardStatus DashboardStatus { get; set; } = new DashboardStatus();
        public PcsStatus PcsStatus { get; set; } = new PcsStatus();
        public SystemComm SystemComm { get; set; } = new SystemComm();
        public EssStatus EssStatus { get; set; } = new EssStatus();
    }

    public class DashboardStatus : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string VihicleStatus { get; set; }
        public LinearGradientBrush VihicleColor { get; set; }
        public float Power { get; set; } // main power
        public int SOC { get; set; }
        public int SOH { get; set; }
        public float BatteryVoltage { get; set; }
        public string CommunicationStatus { get; set; }

    }

    public class PcsStatus : INotifyPropertyChanged
    {
        public float SetPower { get; set; }
        public float PresentPower { get; set; }
        public float PresentCurrent { get; set; }
        public int PresentDCVoltage { get; set; }
        public float Temperature { get; set; }
        public string ErrorCode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class SystemComm
    {
        public int StateOfCharge { get; set; }
        public int StateOfHealth { get; set; }
        public string CellTemperatureRange { get; set; }
    }
    public class EssStatus
    {
        public float CumulativeCharge { get; set; }
        public float CumulativeDischarge { get; set; }
    }
}
