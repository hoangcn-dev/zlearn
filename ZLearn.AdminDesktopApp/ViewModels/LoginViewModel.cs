using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IManageWindowService _manageWindowService;
        private readonly TaskStatusStore _taskStatusStore;

        private string _userName;
        public string UserName 
        { 
            get => _userName;
            set
            {
                SetProperty(ref _userName, value);
                LoginCommand.NotifyCanExecuteChanged();
            } 
        }

        private string _password;
        public string Password 
        { 
            get => _password; 
            set 
            {
                SetProperty(ref _password, value); 
                LoginCommand.NotifyCanExecuteChanged();
            } 
        }

        private string _remember;
        public string Remember { get => _remember; set => SetProperty(ref _remember, value); }


        public IAsyncRelayCommand LoginCommand { get; }
        public ICommand CloseWindowCommand { get; }
        public ICommand NavigateToMainWindowCommand { get; }


        public LoginViewModel(
            VariableStore store,
            IManageWindowService manageWindowService, 
            TaskStatusStore taskStatusStore) : base(taskStatusStore, store)
        {
            _manageWindowService = manageWindowService;

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            NavigateToMainWindowCommand = new RelayCommand(() => _manageWindowService.ShowWindow<MainWindow>());
            CloseWindowCommand = new RelayCommand(() => _manageWindowService.CloseWindow<LoginWindow>());
        }

        private async Task LoginAsync()
        {
            await Task.Delay(1500);
            NavigateToMainWindowCommand.Execute(null);
            CloseWindowCommand.Execute(null);
        }

        private bool CanLogin() =>
                !_taskStatusStore.Loading &&
                !string.IsNullOrEmpty(UserName) &&
                !string.IsNullOrEmpty(Password);
    }
}
