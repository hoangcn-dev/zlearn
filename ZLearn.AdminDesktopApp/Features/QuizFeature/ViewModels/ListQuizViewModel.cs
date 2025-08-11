using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Models;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Services;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Views;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.ViewModels
{
    public class ListQuizViewModel : ViewModelBase
    {
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _manageWindowService;

        private CateListItemDto? _selectedCategory = null;
        private string? _key;
        private bool _isSearching = false;

        public ObservableCollection<DataGridItem<QuizListItemDto>> Quizzes { get; } = new();
        public ObservableCollection<CateListItemDto> Categories { get; } = new();
        public ObservableCollection<string> SelectedIds { get; } = new();
        public CateListItemDto? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    SetProperty(ref _selectedCategory, value);
                    LoadQuizzesData();
                }
            }
        }
        public string? Key
        {
            get => _key;
            set {
                SetProperty(ref _key, value); 
                SearchCommand.NotifyCanExecuteChanged();
            }
        }


        public Visibility ShowUpdateButton => SelectedIds.Count == 1 ? Visibility.Visible : Visibility.Hidden;
        public Visibility ShowDeleteButton => SelectedIds.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        public bool IsLoading => _taskStatusStore.Loading;
        public string SearchButtonText => _isSearching ? "Hủy tìm kiếm" : "Tìm kiếm";
        public bool CanEditSearchText => !_isSearching;


        public IAsyncRelayCommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowAddQuizCommand => new RelayCommand(() =>
        {
            _manageWindowService.ShowSubWindow<MainWindow, AddQuizWindow>(async () => await Search());
        });
        public ICommand DeleteCommand { get; }


        public ListQuizViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            IQuizApiService quizApiService,
            IManageWindowService manageWindowService) : base(taskStatusStore, store)
        {
            _quizApiService = quizApiService;
            _manageWindowService = manageWindowService;

            SearchCommand = new AsyncRelayCommand(Search, CanSearch);
            ResetCommand = new RelayCommand(ResetSearch);
            DeleteCommand = new RelayCommand(DeleteQuiz);

            LoadCategoriesData();
            LoadQuizzesData();
        }

        private void ResetSearch()
        {
            Key = null;
            _isSearching = false;
            SelectedCategory = null;
            SelectedIds.Clear();
            OnPropertyChanged(nameof(ShowDeleteButton));
            OnPropertyChanged(nameof(ShowUpdateButton));
            OnPropertyChanged(nameof(SearchButtonText));
            OnPropertyChanged(nameof(CanEditSearchText));
        }


        private void DeleteQuiz()
        {
            var quizNames = string.Join(", ", Quizzes
                .Where(q => SelectedIds.Contains(q.Data.Id))
                .Select(q => q.Data.Name));
            DialogHelper.ShowConfirm($"Xác nhận xóa các đề: {quizNames}?", async () =>
            {
                var res = await ExecuteAsync(() => _quizApiService.DeleteQuizAsync(new()
                {
                    Ids = SelectedIds.ToList()
                }));
                if (res is not null && res.Succeeded)
                {
                    DialogHelper.ShowSuccessMess("Xóa đề thành công.");
                    await LoadQuizzesData();
                }
                else DialogHelper.ShowErrorMess("Xóa đề thất bại.");
            });
        }

        private async Task Search()
        {
            if (!_store.ContainsKey("AccessToken")) return;
            if (_isSearching)
            {
                Key = null;
                await LoadQuizzesData();
                _isSearching = false;
            }
            else
            {
                await LoadQuizzesData();
                _isSearching = true;
            }
            OnPropertyChanged(nameof(SearchButtonText));
            OnPropertyChanged(nameof(CanEditSearchText));
        }
        private bool CanSearch() => !string.IsNullOrEmpty(Key);

        public async Task LoadQuizzesData()
        {
            // Load quizzes
            var quizzesResult = await ExecuteAsync(() => _quizApiService.GetListQuizzesAsync(new()
            {
                Name = Key,
                CategorySlug = _selectedCategory?.Slug,
                PageIndex = 1,
                PageSize = 100
            }));
            if (quizzesResult is not null && quizzesResult.Succeeded)
            {
                Quizzes.Clear();
                SelectedIds.Clear();
                OnPropertyChanged(nameof(ShowDeleteButton));
                OnPropertyChanged(nameof(ShowUpdateButton));

                var newItems = DataGridItem<QuizListItemDto>.MapFromList(quizzesResult.Data.Items);
                foreach (var item in newItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    Quizzes.Add(item);
                }
            }
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataGridItem<QuizListItemDto>.IsSelected))
            {
                SelectedIds.Clear();
                foreach (var item in Quizzes.Where(i => i.IsSelected))
                {
                    SelectedIds.Add(item.Data.Id);
                }
                OnPropertyChanged(nameof(ShowDeleteButton));
                OnPropertyChanged(nameof(ShowUpdateButton));
            }
        }

        public async void LoadCategoriesData()
        {
            // Load cate
            var categoriesResult = await ExecuteAsync(() => _quizApiService.GetAllCategoriesAsync());
            if (categoriesResult is not null && categoriesResult.Succeeded)
            {
                Categories.Clear();
                foreach (var cate in categoriesResult.Data)
                {
                    Categories.Add(cate);
                }
            }
        }
    }
}
