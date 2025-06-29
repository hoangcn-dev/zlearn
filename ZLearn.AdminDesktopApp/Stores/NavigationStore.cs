using CommunityToolkit.Mvvm.ComponentModel;
using ZLearn.AdminDesktopApp.Enums;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp.Stores
{
    public class NavigationStore : ObservableObject
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel 
        { 
            get => _currentViewModel;
            set
            {
                _currentViewModel?.Dispose();
                SetProperty(ref _currentViewModel, value);
            }
        }

        private NavDestination _currentDestination;
        public NavDestination CurrentDestination 
        { 
            get => _currentDestination; 
            set => SetProperty(ref _currentDestination, value); 
        }
    }
}
