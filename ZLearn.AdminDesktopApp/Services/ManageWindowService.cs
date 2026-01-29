using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.AdminDesktopApp.Views;

namespace ZLearn.AdminDesktopApp.Services
{
    public interface IManageWindowService
    {
        void CloseWindow<TWindow>() where TWindow : Window;
        Window ShowWindow<TWindow>(Action? onClose = null) where TWindow : Window;
        Window ShowSubWindow<TWindow, TSubWindow>(Action? onClose = null) where TWindow : Window where TSubWindow : Window;
    }
    public class ManageWindowService : IManageWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public ManageWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void CloseWindow<TWindow>() where TWindow : Window
        {
            var window = System.Windows.Application.Current.Windows
                .OfType<TWindow>()
                .FirstOrDefault();
            window?.Close();
        }

        public Window ShowWindow<TWindow>(Action? onClose = null) where TWindow : Window
        {
            var win = _serviceProvider.GetRequiredService<TWindow>();
            if (onClose is not null) win.Closed += (s, e) => onClose();
            win.Show();
            return win;
        }

        public Window ShowSubWindow<TWindow, TSubWindow>(Action? onClose = null) where TWindow : Window where TSubWindow : Window
        {
            var win = _serviceProvider.GetRequiredService<TSubWindow>();
            win.Owner = System.Windows.Application.Current.Windows.
                OfType<TWindow>()
                .FirstOrDefault();
            if (onClose is not null) win.Closed += (s, e) => onClose();
            win.ShowDialog();
            return win;
        }
    }
}
