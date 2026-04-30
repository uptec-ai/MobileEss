using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace EMS_PJT_DeploymentInstaller
{
    public partial class MainWindow : Window
    {
        private const string ReleaseZipResourceName = "Payload.Release_260417.zip";
        private const string DbSqlResourceName = "Payload.DB_EMS.sql";

        private const string ReleaseZipFileName = "Release_260417.zip";
        private const string DbSqlFileName = "DB_EMS.sql";

        private const string DefaultFolderName = "EMS_Distribution";
        private const string DbConnectionValue = "Host=localhost;Port=5432;Username=postgres;Password=12345678;Database=DB_EMS;";
        private const string SciChartLicenseValue =
            "oQqSSSFezvsiR7L83SsiuyoBu8UASGj8vmo0ktqoUc61utvVK/jS1fATkXCYmwrEko6hI54S1V/vnMOYCFDWxeLkwbQADkghe8//G+PUXWAdOR8" +
            "AccZLWUVEIULeejsGsnFV/dcOmPvKLpzp4YeplOQuVUpZCSMZ24nO5BNku7ot2OeGrsUW2yMrmyTh7ksheKLoboRE1fw0bJ69njYSj6U+le6qDF" +
            "7TXAueZs4Q6FrynbPblBtSNJTIlaNcVwWAiI/qwSor7Xw1f1+kB7nO5js7X2OzV4aPDopITSUL1T+EMdBp06RNdRG1ToNoSvmRMnCLGPDxVBj6f" +
            "CDC68mhuntz0B6YoKMK/60fFYS5NxbrMcuxGtYg0cR19zH5OHTWW9hoY2OecYIPklWUBDn9eZSALf5czy6robZAle/nEOmCQ5zY7GVCJfnE7yHe" +
            "D2Aj1t/jUm+1MiiAojZO+E+Kl3LTZjFW7369GJG4VrIJfA==";

        public MainWindow()
        {
            InitializeComponent();
            TargetPathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            AppendLog("설치 준비가 완료되었습니다.");
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "배포 파일을 저장할 폴더를 선택하세요.";
                dialog.ShowNewFolderButton = true;
                dialog.SelectedPath = TargetPathTextBox.Text;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TargetPathTextBox.Text = dialog.SelectedPath;
                    AppendLog(string.Format("저장 경로 선택: {0}", dialog.SelectedPath));
                }
            }
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDirectory = TargetPathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(selectedDirectory))
            {
                System.Windows.MessageBox.Show("저장할 폴더 경로를 먼저 입력하거나 선택하세요.", "경로 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                SetUiEnabled(false);
                AppendLog("설치를 시작합니다.");

                var targetDirectory = GetInstallDirectory(selectedDirectory);
                Directory.CreateDirectory(targetDirectory);
                AppendLog(string.Format("폴더 준비 완료: {0}", targetDirectory));

                await WriteEmbeddedFileAsync(ReleaseZipResourceName, Path.Combine(targetDirectory, ReleaseZipFileName));
                AppendLog(string.Format("{0} 저장 완료", ReleaseZipFileName));

                await WriteEmbeddedFileAsync(DbSqlResourceName, Path.Combine(targetDirectory, DbSqlFileName));
                AppendLog(string.Format("{0} 저장 완료", DbSqlFileName));

                SaveUserEnvironmentVariable("EMS_DB_CONN", DbConnectionValue);
                AppendLog("환경변수 저장 완료: EMS_DB_CONN");

                SaveUserEnvironmentVariable("EMS_SCICHART_LICENSE_KEY", SciChartLicenseValue);
                AppendLog("환경변수 저장 완료: EMS_SCICHART_LICENSE_KEY");

                BroadcastEnvironmentChange();
                AppendLog("환경변수 변경 브로드캐스트 완료");

                OpenFolderButton.IsEnabled = true;
                System.Windows.MessageBox.Show("설치가 완료되었습니다.", "완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendLog(string.Format("오류: {0}", ex.Message));
                System.Windows.MessageBox.Show(
                    string.Format("설치 중 오류가 발생했습니다.{0}{1}", Environment.NewLine, ex.Message),
                    "오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SetUiEnabled(true);
            }
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDirectory = TargetPathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(selectedDirectory))
            {
                System.Windows.MessageBox.Show("열 수 있는 폴더가 없습니다.", "폴더 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var targetDirectory = GetInstallDirectory(selectedDirectory);
            if (!Directory.Exists(targetDirectory))
            {
                System.Windows.MessageBox.Show("열 수 있는 폴더가 없습니다.", "폴더 확인", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = targetDirectory,
                UseShellExecute = true
            });
        }

        private static async Task WriteEmbeddedFileAsync(string resourceName, string destinationPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
            {
                throw new InvalidOperationException(string.Format("내장 리소스를 찾을 수 없습니다: {0}", resourceName));
            }

            using (resourceStream)
            using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await resourceStream.CopyToAsync(fileStream);
            }
        }

        private static string GetInstallDirectory(string selectedDirectory)
        {
            var normalizedPath = selectedDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.Equals(Path.GetFileName(normalizedPath), DefaultFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return normalizedPath;
            }

            return Path.Combine(normalizedPath, "EmsDistribution");
        }

        private static void SaveUserEnvironmentVariable(string variableName, string variableValue)
        {
            Environment.SetEnvironmentVariable(variableName, variableValue, EnvironmentVariableTarget.User);
        }

        private void SetUiEnabled(bool enabled)
        {
            BrowseButton.IsEnabled = enabled;
            InstallButton.IsEnabled = enabled;
            TargetPathTextBox.IsEnabled = enabled;
        }

        private void AppendLog(string message)
        {
            var line = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] {1}", DateTime.Now, message);
            if (string.IsNullOrWhiteSpace(LogTextBox.Text))
            {
                LogTextBox.Text = line;
            }
            else
            {
                LogTextBox.AppendText(Environment.NewLine + line);
            }

            LogTextBox.ScrollToEnd();
        }

        private static void BroadcastEnvironmentChange()
        {
            IntPtr result;
            SendMessageTimeout(
                new IntPtr(HWND_BROADCAST),
                WM_SETTINGCHANGE,
                IntPtr.Zero,
                "Environment",
                SMTO_ABORTIFHUNG,
                5000,
                out result);
        }

        private const int HWND_BROADCAST = 0xffff;
        private const int WM_SETTINGCHANGE = 0x001A;
        private const int SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam,
            int fuFlags,
            int uTimeout,
            out IntPtr lpdwResult);
    }
}
