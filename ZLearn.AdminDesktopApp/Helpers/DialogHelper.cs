using System.Windows;
using ZLearn.AdminDesktopApp.Views;

namespace ZLearn.AdminDesktopApp.Helpers
{
    public class DialogHelper
    {
        public static void ShowConfirm(string message, Action onConfirm, Action? onReject = null)
        {
            var res = MessageBox.Show(message, "Xác nhận hành động", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (res == MessageBoxResult.Yes) 
            { 
                onConfirm(); 
            }
            else if (onReject is not null)
            {
                onReject();
            }
        }

        public static void ShowSuccessMess(string message)
        {
            MessageBox.Show(message, "Thông báo thành công", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowErrorMess(string message)
        {
            MessageBox.Show(message, "Thông báo thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowErrors(List<string> errors)
        {
            new ErrorListWindow(errors).ShowDialog();
        }
    }
}
