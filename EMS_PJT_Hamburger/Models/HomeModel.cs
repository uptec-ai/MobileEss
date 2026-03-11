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
    public class HomeModel : ViewModelBase, INotifyPropertyChanged
    {
        // 
        public CancellationTokenSource _loopCts;
        public bool _isLoopRunning;
        public bool Flag { get; set; }
        public Brush Charge_Status_BMS { get; set; }        // BMS(ESS) enable charge
        public Brush Discharge_Status_BMS { get; set; }     // BMS(ESS) enable discharge 

        /// <summary>
        /// UI flag
        /// </summary>
        public bool IsChargingEnergy { get; set; }          // using ENERGY border
        public bool IsChargingPCS1 { get; set; }            // using PCS01 border
        public bool IsChargingBMS { get; set; }             // using BMS(ESS) border
        public bool IsDischargingPCS2 { get; set; }         // using PCS01 border
        public bool IsDischargingTransformer { get; set; }  // using PCS01 to Transformer path 
        public bool IsDischargingOffGrid { get; set; }      // using Transformer to Off grid path
        public bool IsDischargingVihicle { get; set; }      // using Transformer to vihicle path


        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
