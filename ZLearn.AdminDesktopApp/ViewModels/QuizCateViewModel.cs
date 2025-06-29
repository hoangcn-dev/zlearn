using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Models;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views.QuizView;
using ZLearn.Application.Categories.DTOs;

namespace ZLearn.AdminDesktopApp.ViewModels
{
    public class QuizCateViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IQuizApiService _quizApiService;
        private readonly IManageWindowService _manageWindowService;

        public ObservableCollection<DataGridItem<CateListItemDto>> Categories { get; }
        public ObservableCollection<string> SelectedCateIds { get; }


        public Visibility ShowUpdateButton => SelectedCateIds.Count == 1 ? Visibility.Visible : Visibility.Hidden;
        public Visibility ShowDeleteButton => SelectedCateIds.Count > 0 ? Visibility.Visible : Visibility.Hidden;


        public ICommand AddNewCateCommand { get; }
        public ICommand UpdateCateCommand { get; }
        public ICommand DeleteCateCommand { get; }


        public QuizCateViewModel(
            VariableStore store,
            IServiceProvider serviceProvider,
            IQuizApiService quizApiService,
            TaskStatusStore taskStatusStore,
            IManageWindowService manageWindowService) : base(taskStatusStore, store)
        {
            _serviceProvider = serviceProvider;
            _manageWindowService = manageWindowService;
            _serviceProvider = serviceProvider;
            _quizApiService = quizApiService;

            AddNewCateCommand = new RelayCommand(ShowAddCateWindow, CanOpenAddCateWindow);
            UpdateCateCommand = new RelayCommand(ShowUpdateCateWindow);
            DeleteCateCommand = new RelayCommand(DeleteCategories);
            Categories = new ObservableCollection<DataGridItem<CateListItemDto>>();
            SelectedCateIds = new ObservableCollection<string>();
            LoadData();
        }

        private async void LoadData()
        {
            var res = await ExecuteAsync(() => _quizApiService.GetAllCategoriesAsync());
            if (res is not null && res.Succeeded)
            {
                Categories.Clear();
                SelectedCateIds.Clear();
                OnPropertyChanged(nameof(ShowDeleteButton));
                OnPropertyChanged(nameof(ShowUpdateButton));

                var newItems = DataGridItem<CateListItemDto>.MapFromList(res.Data!);
                foreach (var item in newItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                    Categories.Add(item);
                }
            }
            else
            {
                CommonHelper.ShowError(res?.Message ?? "Lỗi không xác định");
            }
        }

        private void ShowAddCateWindow()
        {
            _manageWindowService.ShowSubWindow<AddQuizCategoryWindow>(LoadData);
        }


        private void ShowUpdateCateWindow()
        {
            _store.Add("UpdateCateId", SelectedCateIds.First());
            _manageWindowService.ShowSubWindow<UpdateQuizCategoryWindow>(LoadData);
        }

        private void DeleteCategories()
        {
            DialogHelper.ShowConfirm(
                $"Xác nhận xóa các danh mục sau: {string.Join(", ", SelectedCateIds)}",
                async () =>
                {
                    var res = await ExecuteAsync(() => _quizApiService.DeleteCategory(SelectedCateIds));
                    if (res is not null && res.Succeeded)
                    {
                        DialogHelper.ShowSuccessMess("Xóa danh mục thành công.");
                        LoadData();
                    }
                    else
                    {
                        DialogHelper.ShowErrorMess("Xóa danh mục thất bại.");
                    }
                });
        }

        private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataGridItem<CateListItemDto>.IsSelected))
            {
                SelectedCateIds.Clear();
                foreach (var item in Categories.Where(i => i.IsSelected))
                {
                    SelectedCateIds.Add(item.Data.Id);
                }
                OnPropertyChanged(nameof(ShowDeleteButton));
                OnPropertyChanged(nameof(ShowUpdateButton));
            }
        }

        private bool CanOpenAddCateWindow() => !System.Windows.Application.Current.Windows
                .OfType<AddQuizCategoryWindow>().Any();
    }
}
