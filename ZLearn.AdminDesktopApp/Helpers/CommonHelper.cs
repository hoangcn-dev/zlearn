using System.Windows;

namespace ZLearn.AdminDesktopApp.Helpers
{
    public class CommonHelper
    {
        public static void ShowError(string mess)
        {
            MessageBox.Show(
                "Lỗi",
                mess,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
