using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Services;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Views;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Auth.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.ViewModels
{
    public partial class UpdateUserViewModel : ViewModelBase
    {
        private readonly IUserApiService _userApiService;
        private readonly IFileApiService _fileApiService;
        private readonly IManageWindowService _windowManager;
        private string _uploadFilePath = string.Empty;
        private string? _uploadFileId = null;


        [ObservableProperty]
        private UserDetailDto data;
        [ObservableProperty]
        private string thumbnailUrl;
        [ObservableProperty]
        private ObservableCollection<RoleListItemDto> roles = new();


        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"Đã tải {Path.GetFileName(_uploadFilePath)}";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(_uploadFilePath) ? Visibility.Visible : Visibility.Collapsed;


        public IAsyncRelayCommand UpdateCommand { get; }
        public ICommand UploadThumbnailFileCommand { get; }


        public UpdateUserViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IUserApiService userApiService,
            IFileApiService fileApiService,
            IManageWindowService windowManager) : base(taskStatusStore, store)
        {
            _userApiService = userApiService;
            _fileApiService = fileApiService;
            _windowManager = windowManager;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            UploadThumbnailFileCommand = new RelayCommand(BrowserFile);
            UpdateCommand = new AsyncRelayCommand(Update);
            LoadData();
        }

        private async Task Update()
        {
            var result = MessageBox.Show("Xác nhận cập nhật?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //Save file to server
                if (string.IsNullOrEmpty(_uploadFileId) && !string.IsNullOrEmpty(_uploadFilePath))
                {
                    var saveFileRes = await ExecuteAsync(() => _fileApiService.SaveFilesAsync(new List<string> { _uploadFilePath }));
                    if (saveFileRes is null || !saveFileRes.Succeeded)
                    {
                        MessageBox.Show($"Tải lên ảnh đại diện thất bại: {saveFileRes?.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    _uploadFileId = saveFileRes.Data!.Files.First().Id;
                }

                var data = new UserUpdateContentDto
                {
                    UserName = Data.UserName,
                    PhoneNumber = Data.PhoneNumber,
                    EmailConfirmed = Data.EmailConfirmed,
                    FirstName = Data.FirstName,
                    LastName = Data.LastName,
                    IsActive = Data.IsActive,
                    NickName = Data.NickName,
                    ImageId = _uploadFileId,
                    Roles = Roles.Where(r => r.IsSelected).Select(r => r.Name).ToList()
                };

                var res = await ExecuteAsync(() => _userApiService.UpdateUser(Data.Id, data));
                if (res is not null && res.Succeeded)
                {
                    DialogHelper.ShowSuccessMess("Cập nhật thành công.");
                    _windowManager.CloseWindow<UpdateUserWindow>();
                    return;
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại");
                }
            }
        }

        private async Task LoadData()
        {
            var userId = _store.Get<string>(VariableStore.Keys.SelectedUserId, true);
            if (string.IsNullOrEmpty(userId))
            {
                _windowManager.CloseWindow<UpdateUserWindow>();
                _taskStatusStore.SetErrorStatus("ID user không hợp lệ");
                return;
            }

            var res = await ExecuteAsync(() => _userApiService.GetUserDetailById(userId));
            if (res is not null && res.Succeeded)
            {
                Data = res.Data!;
                ThumbnailUrl = Data.ImagePath ?? string.Empty;
                OnPropertyChanged(nameof(ThumbnailUrl));
                _uploadFilePath = string.Empty;
                _uploadFileId = null;
            }
            else
            {
                _taskStatusStore.SetErrorStatus(res?.Message ?? "Tải thông tin người dùng thất bại.");
                _windowManager.CloseWindow<UpdateUserWindow>();
            }

            // Load roles
            var rolesRes = await ExecuteAsync(() => _userApiService.GetListRoles());
            if (rolesRes is not null && rolesRes.Succeeded)
            {
                Roles.Clear();
                foreach (var role in rolesRes.Data!)
                {
                    Roles.Add(new()
                    {
                        IsSelected = Data.Roles?.Contains(role) ?? false,
                        Name = role
                    });
                }
            }
        }

        private void BrowserFile()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.svg)|*.png;*.jpg;*.jpeg;*.gif;*.svg",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            };
            if (dialog.ShowDialog() == true)
            {
                _uploadFilePath = dialog.FileName;
                _uploadFileId = null;
                ThumbnailUrl = dialog.FileName;
                OnPropertyChanged(nameof(ThumbnailUrl));
                OnPropertyChanged(nameof(UploadFileName));
                OnPropertyChanged(nameof(ShowFileUploadedMessage));
            }
        }

        private void TaskStatusStore_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskStatusStore.Loading))
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }
    }

    public class RoleListItemDto
    {
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }
}
