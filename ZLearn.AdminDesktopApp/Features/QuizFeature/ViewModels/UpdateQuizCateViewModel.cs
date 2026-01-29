using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Utils;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public partial class UpdateQuizCateViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;
        private readonly IFileApiService _fileApiService;


        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"Đã tải {Path.GetFileName(UploadFileUrl)}";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(UploadFileUrl) ? Visibility.Visible : Visibility.Collapsed;


        [ObservableProperty]
        private CateDetailDto? data;
        [ObservableProperty]
        private string updatingName;
        [ObservableProperty]
        private string updatingSlug;
        [ObservableProperty]
        private string updatingDesc;
        [ObservableProperty]
        private string thumbnailUrl = string.Empty;
        [ObservableProperty]
        private string? uploadFileUrl;


        public IAsyncRelayCommand UpdateCommand { get; }
        public ICommand UploadThumbnailFileCommand { get; }
        public ICommand GetSlugFromNameCommand { get; }


        public UpdateQuizCateViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IQuizApiService quizApiService,
            IManageWindowService windowManager,
            IFileApiService fileApiService) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _windowManager = windowManager;
            _fileApiService = fileApiService;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            UpdateCommand = new AsyncRelayCommand(UpdateCate);
            UploadThumbnailFileCommand = new RelayCommand(BrowserFile);
            GetSlugFromNameCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(UpdatingName))
                {
                    MessageBox.Show("Vui lòng nhập tên trước khi lấy slug", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                UpdatingSlug = StringHelper.GenerateSlug(UpdatingName);
            });
            LoadData();
        }

        private async Task UpdateCate()
        {
            var result = MessageBox.Show("Xác nhận cập nhật?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Validate inputs
                if (string.IsNullOrEmpty(UpdatingName))
                {
                    MessageBox.Show("Vui lòng không bỏ trống tên danh mục");
                    return;
                }
                if (string.IsNullOrEmpty(UpdatingDesc))
                {
                    MessageBox.Show("Vui lòng không bỏ trống mô tả danh mục");
                    return;
                }


                //Save file to server
                if (!string.IsNullOrEmpty(UploadFileUrl))
                {
                    var saveFileRes = await ExecuteAsync(() => _fileApiService.SaveFilesAsync(new List<string> { UploadFileUrl }));
                    if (saveFileRes is null || !saveFileRes.Succeeded)
                    {
                        MessageBox.Show($"Tải lên ảnh đại diện thất bại: {saveFileRes?.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    ThumbnailUrl = saveFileRes.Data!.Files.First().SourceUrl;
                    UploadFileUrl = null;
                }

                var res = await ExecuteAsync(() => _quizApiService.UpdateCategoryAsync(Data!.Id, new UpdateCateDto
                {
                    Name = UpdatingName,
                    Slug = UpdatingSlug,
                    Description = UpdatingDesc,
                    ThumbnailUrl = ThumbnailUrl
                }));
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
                UploadFileUrl = dialog.FileName;
                ThumbnailUrl = dialog.FileName;
                OnPropertyChanged(nameof(UploadFileName));
                OnPropertyChanged(nameof(ShowFileUploadedMessage));
            }
        }

        private async void LoadData()
        {
            var id = _store.Get<string>("UpdateCateId", clearAfterGet: true);
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
                UpdatingDesc = Data.Description;
                ThumbnailUrl = Data.ThumbnailUrl;
                UpdatingSlug = Data.Slug;
            }
            else
            {
                _windowManager.CloseWindow<UpdateQuizCateWindow>();
                MessageBox.Show("Tải xuống dữ liệu thất bại");
            }
        }
    }
}
