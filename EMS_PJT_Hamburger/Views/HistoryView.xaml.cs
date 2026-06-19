using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevExpress.Xpf.Editors;
using EMS_PJT_Hamburger.ViewModels;
using SciChart.Data.Model;

namespace EMS_PJT_Hamburger.Views
{
    /// <summary>
    /// HistoryView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HistoryView : UserControl
    {
        // PCS Trend Y축 기본 범위 (편집기 미입력/오류 시 사용)
        private const double DefaultPcsYMin = 0;
        private const double DefaultPcsYMax = 100;

        // PCS Trend X축 기본 가시 범위 변경을 감지하기 위한 구독 소스(=ViewModel)
        private INotifyPropertyChanged _trendNotifySource;

        public HistoryView()
        {
            InitializeComponent();
            Loaded += HistoryView_Loaded;
            DataContextChanged += HistoryView_DataContextChanged;
        }

        private void HistoryView_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeTrendSource(DataContext);
            ResetPcsTrendRange();
            ResetBmsTrendRange();
        }

        private void HistoryView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SubscribeTrendSource(e.NewValue);
            ResetPcsTrendRange();
            ResetBmsTrendRange();
        }

        private void SubscribeTrendSource(object source)
        {
            if (_trendNotifySource != null)
                _trendNotifySource.PropertyChanged -= TrendNotifySource_PropertyChanged;

            _trendNotifySource = source as INotifyPropertyChanged;

            if (_trendNotifySource != null)
                _trendNotifySource.PropertyChanged += TrendNotifySource_PropertyChanged;
        }

        // 조회(Refresh/Today/Week) 시 ViewModel이 기본 가시 범위를 갱신하면 차트를 리셋한다.
        private void TrendNotifySource_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HistoryViewModel.PcsTrendDefaultVisibleRange))
                Dispatcher.BeginInvoke(new Action(ResetPcsTrendRange));
            else if (e.PropertyName == nameof(HistoryViewModel.BmsTrendDefaultVisibleRange))
                Dispatcher.BeginInvoke(new Action(ResetBmsTrendRange));
        }

        // 더블클릭: X축은 마지막 10개 윈도로, Y축은 편집기 값(기본 0~100)으로 리셋한다.
        private void PcsTrendChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ResetPcsTrendRange), DispatcherPriority.Background);
            e.Handled = true;
        }

        private void BmsTrendChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ResetBmsTrendRange), DispatcherPriority.Background);
            e.Handled = true;
        }

        // Y Min/Max 편집기 값이 바뀌면 즉시 Y축 범위에 반영한다. (BMS 전용)
        private void BmsYRange_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            ApplyBmsYRange();
        }

        private void ResetPcsTrendRange()
        {
            // PCS Y축은 AutoRange=Always(데이터에 맞춰 자동). X축만 기본 범위로 리셋.
            if (PcsTrendXAxis != null
                && DataContext is HistoryViewModel vm
                && vm.PcsTrendDefaultVisibleRange != null)
            {
                PcsTrendXAxis.VisibleRange = vm.PcsTrendDefaultVisibleRange;
            }
        }

        private void ResetBmsTrendRange()
        {
            if (BmsTrendXAxis != null
                && DataContext is HistoryViewModel vm
                && vm.BmsTrendDefaultVisibleRange != null)
            {
                BmsTrendXAxis.VisibleRange = vm.BmsTrendDefaultVisibleRange;
            }

            ApplyBmsYRange();
        }

        private void ApplyBmsYRange()
        {
            if (BmsTrendYAxis == null)
                return;

            var min = ToDouble(BmsYMinEdit?.EditValue, DefaultPcsYMin);
            var max = ToDouble(BmsYMaxEdit?.EditValue, DefaultPcsYMax);

            // 잘못된 범위 방어: max는 항상 min보다 커야 한다.
            if (max <= min)
                max = min + 1;

            BmsTrendYAxis.VisibleRange = new DoubleRange(min, max);
        }

        private static double ToDouble(object value, double fallback)
        {
            if (value == null || value == DBNull.Value)
                return fallback;

            if (value is double d)
                return d;

            try
            {
                return Convert.ToDouble(value, CultureInfo.CurrentCulture);
            }
            catch
            {
                return double.TryParse(
                    value.ToString(),
                    NumberStyles.Any,
                    CultureInfo.CurrentCulture,
                    out var parsed)
                    ? parsed
                    : fallback;
            }
        }
    }
}
