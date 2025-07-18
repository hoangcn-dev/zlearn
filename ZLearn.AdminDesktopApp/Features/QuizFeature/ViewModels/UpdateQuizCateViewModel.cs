using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Categories.DTOs;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public partial class UpdateQuizCateViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;
        private readonly IFileApiService _fileApiService;
        private string _uploadFilePath = string.Empty;
        private string? _uploadFileId = null;


        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"Đã tải {Path.GetFileName(_uploadFilePath)}";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(_uploadFilePath) ? Visibility.Visible : Visibility.Collapsed;


        private CateDetailDto? _data;
        public CateDetailDto? Data 
        { 
            get => _data; 
            set => SetProperty(ref _data, value); 
        }


        private string _updatingName;
        public string UpdatingName 
        { 
            get => _updatingName;
            set
            {
                SetProperty(ref _updatingName, value);
                UpdateCommand.NotifyCanExecuteChanged();
            }
        }

        [ObservableProperty]
        private string thumbnailUrl;

        public IAsyncRelayCommand UpdateCommand { get; }
        public ICommand UploadThumbnailFileCommand { get; }


        public UpdateQuizCateViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IQuizApiService quizApiService,
            IManageWindowService windowManager,
            IFileApiService fileApiService) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _windowManager = windowManager;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            UpdateCommand = new AsyncRelayCommand(UpdateCate, CanUpdateCate);
            UploadThumbnailFileCommand = new RelayCommand(BrowserFile);
            LoadData();
            _fileApiService = fileApiService;
        }

        private bool CanUpdateCate() => true;

        private async Task UpdateCate()
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

                var res = await ExecuteAsync(() => _quizApiService.UpdateCategoryAsync(Data!.Id, UpdatingName, _uploadFileId));
                if (res is not null && res.Succeeded)
                {
                    MessageBox.Show("Cập nhật thành công");
                    _windowManager.CloseWindow<UpdateQuizCateWindow>();
                    return;
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại");
                }
            }
        }


        private void TaskStatusStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskStatusStore.Loading))
            {
                OnPropertyChanged(nameof(IsLoading));
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

        private async void LoadData()
        {
            var id = _store.Get<string>("UpdateCateId");
            if (string.IsNullOrEmpty(id))
            {
                _windowManager.CloseWindow<UpdateQuizCateWindow>();
                MessageBox.Show("Tham số lỗi");
                return;
            }

            var res = await ExecuteAsync(() => _quizApiService.GetCategoryDetailAsync(id));
            if (res is not null && res.Succeeded)
            {
                Data = res.Data!;
                UpdatingName = Data.Name;
                ThumbnailUrl = Data.ThumbnailUrl;
            }
            else
            {
                _windowManager.CloseWindow<UpdateQuizCateWindow>();
                MessageBox.Show("Tải xuống dữ liệu thất bại");
            }
        }
    }
}
