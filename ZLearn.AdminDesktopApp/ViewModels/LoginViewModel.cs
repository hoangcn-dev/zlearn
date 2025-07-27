using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views;
using ZLearn.Application.Auth.DTOs;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IManageWindowService _manageWindowService;
        private readonly IAuthApiService _authApiService;
        private readonly VariableStore _varStore;

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

        public bool IsLoading => _taskStatusStore.Loading;

        public IAsyncRelayCommand LoginCommand { get; }
        public ICommand CloseWindowCommand { get; }
        public ICommand NavigateToMainWindowCommand { get; }


        public LoginViewModel(
            VariableStore store,
            IManageWindowService manageWindowService,
            TaskStatusStore taskStatusStore,
            IAuthApiService authApiService,
            VariableStore varStore) : base(taskStatusStore, store)
        {
            _manageWindowService = manageWindowService;
            _authApiService = authApiService;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;
            _varStore = varStore;

            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
            NavigateToMainWindowCommand = new RelayCommand(() => _manageWindowService.ShowWindow<MainWindow>());
            CloseWindowCommand = new RelayCommand(() => _manageWindowService.CloseWindow<LoginWindow>());
            UserName = "admin";
            Password = "Admin@123";
        }

        private void TaskStatusStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskStatusStore.Loading)) 
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async Task LoginAsync()
        {
            var data = new SignInRequestDto
            {
                Password = Password,
                UserName = UserName,
            };
            var res = await ExecuteAsync(() => _authApiService.SignInAsync(data));
            if (res is not null && res.Succeeded)
            {
                DialogHelper.ShowSuccessMess($"Đăng nhập thành công, xin chào {res.Data!.UserName}");
                _varStore.Add(VariableStore.Keys.AccessToken, res.Data.Token.AccessToken);
                _varStore.Add(VariableStore.Keys.RefreshToken, res.Data.Token.RefreshToken);
                _varStore.Add(VariableStore.Keys.UserName, res.Data.UserName);
                _varStore.Add(VariableStore.Keys.Role, res.Data.Roles.First());
                _varStore.Add(VariableStore.Keys.UserId, res.Data.Id);
                _varStore.Add(VariableStore.Keys.ImageUrl, res.Data.ImagePath);
                NavigateToMainWindowCommand.Execute(null);
                CloseWindowCommand.Execute(null);
            }
            else
            {
                DialogHelper.ShowErrorMess($"Đăng nhập thất bại: {res!.Message}");
            }
        }

        private bool CanLogin() =>
                !_taskStatusStore.Loading &&
                !string.IsNullOrEmpty(UserName) &&
                !string.IsNullOrEmpty(Password);
    }
}
