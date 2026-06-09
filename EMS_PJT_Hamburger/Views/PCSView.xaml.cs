using EMS_PJT_Hamburger.Models.Client.PCS;
using SciChart.Data.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EMS_PJT_Hamburger.Views
{
    /// <summary>
    /// Interaction logic for PCSView.xaml.
    /// </summary>
    public partial class PCSView : UserControl
    {
        private INotifyPropertyChanged _trendNotifySource;
        private bool _isPowerTrendUserNavigating;

        public PCSView()
        {
            InitializeComponent();
            Loaded += PCSView_Loaded;
            DataContextChanged += PCSView_DataContextChanged;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var app = Application.Current as App;
            if (app?.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateHome();
            }
        }

        private void PCSView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeTrendSource(DataContext);
            ResetPowerTrendChartRange();
        }

        private void PCSView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SubscribeTrendSource(e.NewValue);
            _isPowerTrendUserNavigating = false;
            ResetPowerTrendChartRange();
        }

        private void SubscribeTrendSource(object source)
        {
            if (_trendNotifySource != null)
                _trendNotifySource.PropertyChanged -= TrendNotifySource_PropertyChanged;

            _trendNotifySource = source as INotifyPropertyChanged;

            if (_trendNotifySource != null)
                _trendNotifySource.PropertyChanged += TrendNotifySource_PropertyChanged;
        }

        private void TrendNotifySource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(PcsModel.PowerTrendDefaultVisibleRange))
                return;

            if (!_isPowerTrendUserNavigating)
                Dispatcher.BeginInvoke(new Action(ResetPowerTrendChartRange));
        }

        private void PowerTrendChart_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _isPowerTrendUserNavigating = true;
        }

        private void PowerTrendChart_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount < 2)
                _isPowerTrendUserNavigating = true;
        }

        private void PowerTrendChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _isPowerTrendUserNavigating = false;
            ResetPowerTrendChartRange();
            e.Handled = true;
        }

        private void ResetPowerTrendChartRange()
        {
            if (DataContext is PcsModel model && model.PowerTrendDefaultVisibleRange != null)
                PowerTrendXAxis.VisibleRange = model.PowerTrendDefaultVisibleRange;

            PowerTrendYAxis.VisibleRange = new DoubleRange(0, 100);
        }
    }
}
