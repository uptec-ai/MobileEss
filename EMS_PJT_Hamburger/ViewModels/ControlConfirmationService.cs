using System.Windows;

namespace EMS_PJT_Hamburger.ViewModels
{
    internal static class ControlConfirmationService
    {
        public static bool Confirm(string target, string action)
        {
            var message = $"{target} {action} 제어를 실행하시겠습니까?\n실행하려면 Yes를 선택하세요.";
            var result = MessageBox.Show(
                message,
                "제어 실행 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            return result == MessageBoxResult.Yes;
        }
    }
}
