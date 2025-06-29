using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views.QuizView;
using ZLearn.Application.Categories.DTOs;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class UpdateQuizCategoryViewModel : ViewModelBase
    {
        
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _windowManager;

        public bool IsLoading => _taskStatusStore.Loading;


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


        public IAsyncRelayCommand UpdateCommand { get; }


        public UpdateQuizCategoryViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IQuizApiService quizApiService,
            IManageWindowService windowManager) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _windowManager = windowManager;
            _taskStatusStore.PropertyChanged += TaskStatusStore_PropertyChanged;

            UpdateCommand = new AsyncRelayCommand(UpdateCate, CanUpdateCate);
            LoadData();
        }

        private bool CanUpdateCate() =>
            !string.IsNullOrEmpty(UpdatingName) &&
            UpdatingName != Data?.Name;

        private async Task UpdateCate()
        {
            var result = MessageBox.Show("Xác nhận cập nhật?", "Confirm", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var res = await ExecuteAsync(() => _quizApiService.UpdateCategory(Data!.Id, UpdatingName));
                if (res is not null && res.Succeeded)
                {
                    MessageBox.Show("Cập nhật thành công");
                    _windowManager.CloseWindow<UpdateQuizCategoryWindow>();
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


        private async void LoadData()
        {
            var id = _store.Get<string>("UpdateCateId");
            if (string.IsNullOrEmpty(id))
            {
                _windowManager.CloseWindow<UpdateQuizCategoryWindow>();
                MessageBox.Show("Tham số lỗi");
                return;
            }

            var res = await ExecuteAsync(() => _quizApiService.GetCategoryDetailAsync(id));
            if (res is not null && res.Succeeded)
            {
                Data = res.Data!;
                UpdatingName = Data.Name;
            }
            else
            {
                _windowManager.CloseWindow<UpdateQuizCategoryWindow>();
                MessageBox.Show("Tải xuống dữ liệu thất bại");
            }
        }
    }
}
