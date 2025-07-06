using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public class AddQuizCateViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;


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

        public IAsyncRelayCommand AddCateCommand { get; }

        public AddQuizCateViewModel(
            VariableStore store,
            TaskStatusStore taskStatusStore,
            IQuizApiService quizApiService,
            IManageWindowService windowManager) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _windowManager = windowManager;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            AddCateCommand = new AsyncRelayCommand(AddCateAsync, CanAddCate);
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

            var res = await ExecuteAsync(() => _quizApiService.CreateNewCategoryAsync(CategoryName));
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
