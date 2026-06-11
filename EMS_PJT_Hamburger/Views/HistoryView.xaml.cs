using System;
using System.Collections.Generic;
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

        public HistoryView()
        {
            InitializeComponent();
        }

        // 더블클릭: X축은 ZoomExtentsModifier(XDirection)가 데이터에 맞추고,
        // Y축은 편집기 값(기본 0~100)으로 결정적으로 재고정한다.
        private void PcsTrendChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ApplyPcsYRange), DispatcherPriority.Background);
        }

        // Y Min/Max 편집기 값이 바뀌면 즉시 Y축 범위에 반영한다.
        private void PcsYRange_EditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            ApplyPcsYRange();
        }

        private void ApplyPcsYRange()
        {
            if (PowerTrendYAxis == null)
                return;

            var min = ToDouble(PcsYMinEdit?.EditValue, DefaultPcsYMin);
            var max = ToDouble(PcsYMaxEdit?.EditValue, DefaultPcsYMax);

            // 잘못된 범위 방어: max는 항상 min보다 커야 한다.
            if (max <= min)
                max = min + 1;

            PowerTrendYAxis.VisibleRange = new DoubleRange(min, max);
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
