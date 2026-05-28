using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using EMS_PJT_Hamburger.ViewModels;
using EMS_PJT_Hamburger.Views;
using System;
using System.Collections.Generic;
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

namespace EMS_PJT_Hamburger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        App app = Application.Current as App;
        public MainWindow()
        {
            InitializeComponent();

            app.MainWindow = this;
            NaviFrame.Content = app.HomeView;
        }

        private void Btn_Home_Click(object sender, RoutedEventArgs e)
        {
            app.MainWindow = this;
            NaviFrame.Content = app.HomeView;
        }

        private void Btn_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            //app.MainWindow = this;
            //NaviFrame.Content = app.DashBoardView;
        }

        private void Btn_PcsStatus_Click(object sender, RoutedEventArgs e)
        {

            app.MainWindow = this;
            NaviFrame.Content = app.PCSView;
        }

        private void Btn_BmsStatus_Click(object sender, RoutedEventArgs e)
        {
            app.MainWindow = this;
            NaviFrame.Content = app.BMSView;
        }

        //private void Btn_SystemStatus_Click(object sender, RoutedEventArgs e)
        //{
        //    app.MainWindow = this;
        //    NaviFrame.Content = app.SystemView;
        //}

        private void Btn_History_Click(object sender, RoutedEventArgs e)
        {
            app.MainWindow = this;
            NaviFrame.Content = app.HistoryView;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgResult = WinUIMessageBox.Show(
                GetWindow(app.MainWindow),
                "모니터링 중입니다.\r\n그래도 종료 하시겠습니까?",
                null,
                MessageBoxButton.YesNo,
                MessageBoxImage.None,
                MessageBoxResult.None,
                MessageBoxOptions.None,
                FloatingMode.Window);

            if (msgResult == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ThemedWindow_Loaded(object sender, RoutedEventArgs e)
        {
            App app = Application.Current as App;
            // Popup 워밍업
            var frame = new NavigationFrame();
            frame.Navigate(new AlarmDetailWindow());
            frame.Content = null;

            await WarmUpViewAsync(app.PCSView);
            await WarmUpViewAsync(app.BMSView);
        }
        private async Task WarmUpViewAsync(FrameworkElement view)
        {
            if (view == null) return;

            WarmupHost.Content = view;

            view.ApplyTemplate();
            WarmupHost.ApplyTemplate();

            await Dispatcher.Yield(DispatcherPriority.Loaded);
            WarmupHost.UpdateLayout();
            view.UpdateLayout();

            await Dispatcher.Yield(DispatcherPriority.Render);

            WarmupHost.Content = null;
        }
    }
}
