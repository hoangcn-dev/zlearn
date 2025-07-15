using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Enums;
using ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels;
using ZLearn.AdminDesktopApp.Stores;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly NavigationStore _navStore;
        private readonly IServiceProvider _serviceProvider;

        public ViewModelBase CurrentViewModel => _navStore.CurrentViewModel;
        public NavDestination CurrentDestination => _navStore.CurrentDestination;
        public ICommand NavigateCommand { get; }

        public Visibility IsBusy => _taskStatusStore.Loading ? Visibility.Visible : Visibility.Collapsed;
        public string Status => _taskStatusStore.CurrentStatus is not null ?
            $"{_taskStatusStore.CurrentStatus.Type.GetDescription()}: {_taskStatusStore.CurrentStatus.Message}" :
            string.Empty;

        public MainViewModel(
            VariableStore store,
            TaskStatusStore taskStatusStore,
            NavigationStore navStore,
            IServiceProvider serviceProvider) : base(taskStatusStore, store)
        {
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;
            _navStore = navStore;
            _navStore.PropertyChanged += NavStore_PropertyChanged;
            _serviceProvider = serviceProvider;
            NavigateCommand = new RelayCommand<NavDestination>(NavigateToView, CanNavigate);
        }

        private void TaskStatusStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskStatusStore.CurrentStatus))
            {
                OnPropertyChanged(nameof(Status));
            }
            else if (e.PropertyName == nameof(TaskStatusStore.Loading))
            {
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        private void NavStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavigationStore.CurrentViewModel))
            {
                OnPropertyChanged(nameof(CurrentViewModel));
            }
            else if (e.PropertyName == nameof(NavigationStore.CurrentDestination))
            {
                OnPropertyChanged(nameof(CurrentDestination));
            }
        }

        private bool CanNavigate(NavDestination dest) => true;

        private void NavigateToView(NavDestination destination)
        {
            _navStore.CurrentDestination = destination;
            if (destination == NavDestination.QuizCate)
            {
                _navStore.CurrentViewModel = _serviceProvider.GetRequiredService<QuizCateViewModel>();
            }
            else if (destination == NavDestination.QuizAnal)
            {
                _navStore.CurrentViewModel = _serviceProvider.GetRequiredService<QuizStatViewModel>();
            }
            else if (destination == NavDestination.QuizManage)
            {
                _navStore.CurrentViewModel = _serviceProvider.GetRequiredService<ListQuizViewModel>();
            }
        }
    }
}
