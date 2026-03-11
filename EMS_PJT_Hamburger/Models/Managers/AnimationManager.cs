using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Models.Managers
{
    public static class AnimationManager
    {
        #region IsRunning

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.RegisterAttached(
                "IsRunning", typeof(bool), typeof(AnimationManager),
                new PropertyMetadata(false, OnIsRunningChanged));
        public static void SetIsRunning(DependencyObject d, bool value) => d.SetValue(IsRunningProperty, value);
        public static bool GetIsRunning(DependencyObject d) => (bool)d.GetValue(IsRunningProperty);

        #endregion

        #region From

        public static readonly DependencyProperty FromProperty =
            DependencyProperty.RegisterAttached(
                "From", typeof(double), typeof(AnimationManager),
                new PropertyMetadata(-24.0));
        public static void SetFrom(DependencyObject d, double v) => d.SetValue(FromProperty, v);
        public static double GetFrom(DependencyObject d) => (double)d.GetValue(FromProperty);

        #endregion

        #region To

        public static readonly DependencyProperty ToProperty =
            DependencyProperty.RegisterAttached(
                "To", typeof(double), typeof(AnimationManager),
                new PropertyMetadata(0.0));
        public static void SetTo(DependencyObject d, double v) => d.SetValue(ToProperty, v);
        public static double GetTo(DependencyObject d) => (double)d.GetValue(ToProperty);

        #endregion

        #region Speed

        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.RegisterAttached(
                "Speed", typeof(double), typeof(AnimationManager),
                new PropertyMetadata(1.2));
        public static void SetSpeed(DependencyObject d, double v) => d.SetValue(SpeedProperty, v);
        public static double GetSpeed(DependencyObject d) => (double)d.GetValue(SpeedProperty);

        #endregion


        #region StartDelaySec

        public static readonly DependencyProperty StartDelaySecProperty =
            DependencyProperty.RegisterAttached(
                "StartDelaySec", typeof(double), typeof(AnimationManager),
                new PropertyMetadata(0.0));
        public static void SetStartDelaySec(DependencyObject d, double v) => d.SetValue(StartDelaySecProperty, v);
        public static double GetStartDelaySec(DependencyObject d) => (double)d.GetValue(StartDelaySecProperty);

        #endregion

        // ---- handler ----
        private static async void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var path = d as Path;
            if (path == null) return;

            //EnsureHookLoaded(path);

            bool isRunning = (bool)e.NewValue;

            if (!isRunning)
            {
                // Stop: 애니메이션 해제 + 숨김
                path.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
                path.BeginAnimation(UIElement.OpacityProperty, null);
                path.BeginAnimation(UIElement.VisibilityProperty, null);
                // Dash 기본값 보강
                path.StrokeDashArray = null;
                path.StrokeDashOffset = 0;
                path.Opacity = 1.0;
                path.Visibility = Visibility.Visible;
                return;
            }

            double startDelay = GetStartDelaySec(path);
            if (startDelay > 0)
            {
                // UI 컨텍스트 유지
                await Task.Delay(TimeSpan.FromSeconds(startDelay)).ConfigureAwait(true);
            }

            await path.Dispatcher.InvokeAsync(() =>
            {
                // 초기 상태
                path.Visibility = Visibility.Visible;
                path.Opacity = 0;

                // Dash 기본값 보강
                if (path.StrokeDashArray == null || path.StrokeDashArray.Count == 0)
                {
                    path.StrokeDashArray = new DoubleCollection { 2, 2 };
                }

                // 성능 캐시(드롭섀도 사용 시 특히 권장)
                if (path.CacheMode == null)
                {
                    path.CacheMode = new BitmapCache();
                }

                // Fade-in
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
                {
                    FillBehavior = FillBehavior.HoldEnd
                };
                path.BeginAnimation(UIElement.OpacityProperty, fade);

                // 흐름 애니메이션
                var flow = new DoubleAnimation
                {
                    From = GetFrom(path),
                    To = GetTo(path),
                    Duration = TimeSpan.FromSeconds(GetSpeed(path)),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                path.BeginAnimation(Shape.StrokeDashOffsetProperty, flow);

            }, DispatcherPriority.Loaded);
        }
    }
}
