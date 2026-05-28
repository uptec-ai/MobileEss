using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.Models
{
    public enum HomeStatus
    {
        Charging,
        Discharging,
        Waiting,
    }
    public enum LoadStatus
    {
        OnGrid,
        OffGrid,
        Vehicle,
        Waiting,
    }
    public class HomeModel : ViewModelBase, INotifyPropertyChanged
    {
        // 
        public CancellationTokenSource _loopCts;
        public bool _isLoopRunning;
        public Brush PChargeBorderBrush { get; set; } = Brushes.Gray;         // ems status is charging. (t:greenyellow, f:gray)
        public Brush BChargeBorderBrush { get; set; } = Brushes.Gray;         // ems status is charging. (t:greenyellow, f:gray)
        public Brush PDischargeBorderBrush { get; set; } = Brushes.Gray;      // pcs status is discharging. (t:orange, f:gray) 
        public Brush BDischargeBorderBrush { get; set; } = Brushes.Gray;      // battery status is discharging. (t:orange, f:gray) 
        public Brush DischargeBorderBrush { get; set; } = Brushes.Gray;      // status is discharging. (t:orange, f:gray) 
        public Brush ChargeOnGrid { get; set; } = Brushes.Gray;
        public Brush ChargeOffGrid { get; set; } = Brushes.Gray;
        public Brush ChargeVihicle { get; set; } = Brushes.Gray;
        public Brush PcsBorderBrush { get; set; } = Brushes.Gray;
        public Brush BmsBorderBrush { get; set; } = Brushes.Gray;
        public Brush OperationModeBrush { get; set; } = Brushes.Gray;

        /// <summary>
        /// UI flag
        /// </summary>
        public int emsMode { get; set; } = 0;
        public HomeStatus ChargingStatus { get; set; } = HomeStatus.Waiting;  // charge status
        public LoadStatus LoadTarget { get; set; } = LoadStatus.Waiting;  // load charge target
        public bool CouplingStatus { get; set; } = false;

        //public string ConnectPCS { get; set; } = "Disable";
        //public string ConnectBMS { get; set; } = "Disable";


        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
