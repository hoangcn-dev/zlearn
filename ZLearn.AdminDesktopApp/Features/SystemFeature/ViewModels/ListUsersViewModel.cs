using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Models;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Services;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Views;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Auth.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.ViewModels
{
    public class ListUsersViewModel : ViewModelBase
    {
        private readonly IUserApiService _userApiService;
        private readonly IManageWindowService _manageWindowService;


        private string? _key;
        private bool _isSearching = false;


        public ObservableCollection<DataGridItem<UserListItemDto>> Users { get; } = new();
        public ObservableCollection<string> SelectedIds { get; } = new();


        public string? Key
        {
            get => _key;
            set
            {
                SetProperty(ref _key, value);
                SearchCommand.NotifyCanExecuteChanged();
            }
        }


        public Visibility ShowUpdateButton => SelectedIds.Count == 1 ? Visibility.Visible : Visibility.Hidden;
        public string SearchButtonText => _isSearching ? "Hủy tìm kiếm" : "Tìm kiếm";
        public bool CanEditSearchText => !_isSearching;
        public bool IsLoading => _taskStatusStore.Loading;


        public IAsyncRelayCommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowUpdateDialogCommand { get; }


        public ListUsersViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IManageWindowService manageWindowService,
            IUserApiService userApiService) : base(taskStatusStore, store)
        {
            _manageWindowService = manageWindowService;
            _userApiService = userApiService;
            SearchCommand = new AsyncRelayCommand(Search, CanSearch);
            ResetCommand = new RelayCommand(ResetSearch);
            ShowUpdateDialogCommand = new AsyncRelayCommand(UpdateUserDetail);

            LoadUsersData();
        }

        private void ResetSearch()
        {
            Key = null;
            _isSearching = false;
            SelectedIds.Clear();
            OnPropertyChanged(nameof(ShowUpdateButton));
            OnPropertyChanged(nameof(SearchButtonText));
            OnPropertyChanged(nameof(CanEditSearchText));
        }

        private async Task UpdateUserDetail()
        {
            var userId = SelectedIds[0];
            _store.Add(VariableStore.Keys.SelectedUserId, userId);
            _manageWindowService.ShowSubWindow<UpdateUserWindow>(LoadUsersData);
        }

        private bool CanSearch() => !string.IsNullOrEmpty(Key);


        private async Task Search()
        {
            if (!_store.ContainsKey("AccessToken")) return;
            if (_isSearching)
            {
                Key = null;
                LoadUsersData();
                _isSearching = false;
            }
            else
            {
                LoadUsersData();
                _isSearching = true;
            }
            OnPropertyChanged(nameof(SearchButtonText));
            OnPropertyChanged(nameof(CanEditSearchText));
        }

        private async void LoadUsersData()
        {
            var users = await ExecuteAsync(() => _userApiService.GetListUsers(new()
            {
                Email = Key ?? string.Empty,
                PageIndex = 1,
                PageSize = 100,
            }));
            if (users is not null && users.Succeeded)
            {
                Users.Clear();
                var newItems = DataGridItem<UserListItemDto>.MapFromList(users.Data.Items);
                foreach (var item in newItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    Users.Add(item);
                }
            }
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataGridItem<QuizListItemDto>.IsSelected))
            {
                SelectedIds.Clear();
                foreach (var item in Users.Where(i => i.IsSelected))
                {
                    SelectedIds.Add(item.Data.Id);
                }
                OnPropertyChanged(nameof(ShowUpdateButton));
            }
        }
    }
}
