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
using ZLearn.Domain.Entities;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public class AddQuizCateViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;
        private readonly IFileApiService _fileApiService;
        private string _uploadFilePath = string.Empty;
        private string _uploadFileId = string.Empty;


        private string _categoryName;
        public string CategoryName 
        { 
            get => _categoryName;
            set
            {
                SetProperty(ref _categoryName, value);
                AddCateCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsLoading => _taskStatusStore.Loading;
        public string UploadFileName => $"Đã tải {Path.GetFileName(_uploadFilePath)}";
        public Visibility ShowFileUploadedMessage => !string.IsNullOrEmpty(_uploadFilePath) ? Visibility.Visible : Visibility.Collapsed;


        public ICommand UploadThumbnailFileCommand { get; }
        public IAsyncRelayCommand AddCateCommand { get; }


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

            AddCateCommand = new AsyncRelayCommand(AddCateAsync, CanAddCate);
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
                _uploadFilePath = dialog.FileName;
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

        private bool CanAddCate() => true;

        private async Task AddCateAsync()
        {
            if (string.IsNullOrEmpty(CategoryName))
            {
                MessageBox.Show("Vui lòng không bỏ trống tên");
                return;
            }

            if (string.IsNullOrEmpty(_uploadFilePath))
            {
                MessageBox.Show("Vui lòng tải lên ảnh đại diện cho danh mục");
                return;
            }

            //Save file to server
            if (string.IsNullOrEmpty(_uploadFileId))
            {
                var saveFileRes = await ExecuteAsync(() => _fileApiService.SaveFilesAsync(new List<string> { _uploadFilePath }));
                if (saveFileRes is null || !saveFileRes.Succeeded)
                {
                    MessageBox.Show($"Tải lên ảnh đại diện thất bại: {saveFileRes?.Message}", "Thất bại", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _uploadFileId = saveFileRes.Data!.Files.First().Id;
            }

            var res = await ExecuteAsync(() => _quizApiService.CreateNewCategoryAsync(CategoryName, _uploadFileId));
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
