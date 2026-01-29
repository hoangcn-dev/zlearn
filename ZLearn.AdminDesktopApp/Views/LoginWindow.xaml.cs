using System.Windows;
using System.Windows.Controls;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is LoginViewModel vm)
            {
                vm.Password = passwordBox.Password;
            }
        }
    }
}
