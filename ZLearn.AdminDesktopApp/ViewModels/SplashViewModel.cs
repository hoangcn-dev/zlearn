using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class SplashViewModel : ViewModelBase
    {
        private readonly IManageWindowService _manageWindowService;

        private string _loadingMessage = "Initializing application...";
        public string LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        public ICommand NavigateToLoginWindowCommand { get; }
        public IAsyncRelayCommand LoadDataCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public SplashViewModel(
            VariableStore store,
            TaskStatusStore taskStatusStore,
            IManageWindowService manageWindowService) : base(taskStatusStore, store)
        {
            _manageWindowService = manageWindowService;
            LoadDataCommand = new AsyncRelayCommand(LoadApplicationData);
            CloseWindowCommand = new RelayCommand(() => _manageWindowService.CloseWindow<SplashWindow>());
            NavigateToLoginWindowCommand = new RelayCommand(() => _manageWindowService.ShowWindow<LoginWindow>());
        }

        private async Task LoadApplicationData()
        {
            // Simulate loading tasks
            _taskStatusStore.Loading = true;
            LoadingMessage = "Setting up services...";
            await Task.Delay(1000);
            LoadingMessage = "Connecting to API...";
            await Task.Delay(1000);
            LoadingMessage = "Loading UI...";
            await Task.Delay(500);
            _taskStatusStore.Loading = false;
            NavigateToLoginWindowCommand.Execute(null);
            CloseWindowCommand.Execute(null);
        }
    }
}
