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

namespace EMS_PJT_Hamburger.Views
{
    /// <summary>
    /// HomeView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();
        }
        private void PCS_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.PCSView;
                mainWindow.Btn_PcsStatus.IsSelected = true;
            }
        }

        private void PCS_TouchDown(object sender, TouchEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.PCSView;
                mainWindow.Btn_PcsStatus.IsSelected = true;
            }
        }

        private void BMS_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.BMSView;
                mainWindow.Btn_BmsStatus.IsSelected = true;
            }
        }

        private void BMS_TouchDown(object sender, TouchEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                var app = (App)Application.Current;

                mainWindow.NaviFrame.Content = app.BMSView;
                mainWindow.Btn_BmsStatus.IsSelected = true;
            }

        }
    }
}
