using System.Threading.Tasks;
using System.Windows;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            Loaded += SplashWindow_Loaded;
        }

        private async void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is SplashViewModel vm && vm.LoadDataCommand.CanExecute(null)) 
                await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
