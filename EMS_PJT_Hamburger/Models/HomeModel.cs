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
    public class HomeModel : ViewModelBase, INotifyPropertyChanged
    {
        // 
        public CancellationTokenSource _loopCts;
        public bool _isLoopRunning;
        public Brush ChargeBorderBrush { get; set; } = Brushes.Gray;         // ems status is charging. (t:greenyellow, f:gray)
        public Brush DischargeBorderBrush { get; set; } = Brushes.Gray;      // ems status is discharging. (t:orange, f:gray) 
        public Brush ChargeOffGrid { get; set; } = Brushes.Gray;      // ems status is discharging. (t:orange, f:gray) 
        public Brush ChargeVihicle { get; set; } = Brushes.Gray;      // ems status is discharging. (t:orange, f:gray) 
        public Brush PcsBorderBrush { get; set; } = Brushes.Gray;
        public Brush BmsBorderBrush { get; set; } = Brushes.Gray;
        public Brush OperationModeBrush { get; set; } = Brushes.Gray;
        public string OperationModeText { get; set; } = "대기";

        /// <summary>
        /// UI flag
        /// </summary>
        public int emsMode { get; set; } = 0;
        public HomeStatus ChargingStatus { get; set; }  // charge status
        public bool IsChargingOffGrid { get; set; }      // using Transformer to Off grid path
        public bool IsChargingVihicle { get; set; }      // using Transformer to vihicle path

        public string ConnectPCS { get; set; } = "Disable";
        public string ConnectBMS { get; set; } = "Disable";


        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
