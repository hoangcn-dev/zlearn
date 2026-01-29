using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Utils;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public partial class AddQuizCateViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;
        private readonly IFileApiService _fileApiService;

        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string slug;
        [ObservableProperty]
        private string description;
        [ObservableProperty]
        private string thumbnailUrl;
        [ObservableProperty]
        private string? uploadFileUrl;

        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"Đã tải {Path.GetFileName(UploadFileUrl)}";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(UploadFileUrl) ? Visibility.Visible : Visibility.Collapsed;


        public ICommand UploadThumbnailFileCommand { get; }
        public IAsyncRelayCommand AddCateCommand { get; }
        public RelayCommand GetSlugFromNameCommand { get; }


        public AddQuizCateViewModel(
            VariableStore store,
            TaskStatusStore taskStatusStore,
            IQuizApiService quizApiService,
            IManageWindowService windowManager,
            IFileApiService fileApiService) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _windowManager = windowManager;
            _fileApiService = fileApiService;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            AddCateCommand = new AsyncRelayCommand(AddCateAsync);
            GetSlugFromNameCommand = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(Name))
                {
                    MessageBox.Show("Vui lòng nhập tên danh mục trước khi lấy slug", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Slug = StringHelper.GenerateSlug(Name);
                DialogHelper.ShowSuccessMess(Slug);
                OnPropertyChanged(nameof(Slug));
            });
            UploadThumbnailFileCommand = new RelayCommand(BrowserFile);
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
                OnPropertyChanged(nameof(UploadFileName));
                OnPropertyChanged(nameof(ShowFileUploadedMessage));
            }
        }

        private void TaskStatusStore_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TaskStatusStore.Loading))
            {
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private async Task AddCateAsync()
        {
            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show("Vui lòng không bỏ trống tên");
                return;
            }

            if (string.IsNullOrEmpty(Description))
            {
                MessageBox.Show("Vui lòng không bỏ trống mô tả");
                return;
            }

            if (string.IsNullOrEmpty(UploadFileUrl))
            {
                MessageBox.Show("Vui lòng tải lên ảnh đại diện cho danh mục");
                return;
            }

            //Save file to server
            if (string.IsNullOrEmpty(ThumbnailUrl))
            {
                var saveFileRes = await ExecuteAsync(() => _fileApiService.SaveFilesAsync(new List<string> { UploadFileUrl }));
                if (saveFileRes is null || !saveFileRes.Succeeded)
                {
                    MessageBox.Show($"Tải lên ảnh đại diện thất bại: {saveFileRes?.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                ThumbnailUrl = saveFileRes.Data!.Files.First().SourceUrl;
            }

            var res = await ExecuteAsync(() => _quizApiService.CreateNewCategoryAsync(new()
            {
                Name = Name,
                Slug = Slug,
                Description = Description,
                ThumbnailUrl = ThumbnailUrl
            }));
            if (res is not null && res.Succeeded)
            {
                MessageBox.Show("Tạo danh mục mới thành công");
                _windowManager.CloseWindow<AddQuizCateWindow>();
                return;
            }
            else
            {
                MessageBox.Show($"Tạo danh mục mới thất bại: {res?.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
