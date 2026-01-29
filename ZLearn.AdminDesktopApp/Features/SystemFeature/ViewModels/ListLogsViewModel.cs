using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using ZLearn.AdminDesktopApp.Features.QuizFeature.Models;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Services;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Logs.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.ViewModels
{
    public partial class ListLogsViewModel : ViewModelBase
    {
        private readonly ILogApiService _logService;
        private readonly List<LogListItemDto> _logs = new();

        public ListLogsViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            ILogApiService logService) : base(taskStatusStore, store)
        {
            _logService = logService;
            SearchCommand = new RelayCommand(SearchLogsByUserId, CanSearchLogsByUserId);
            ResetCommand = new RelayCommand(ResetList);
            ShowExceptionCommand = new RelayCommand(ShowException);
            LoadData();
        }

        private async Task LoadData()
        {
            var logs = await ExecuteAsync(() => _logService.GetLogsOfDay(SelectedDate));
            if (logs is not null && logs.Succeeded)
            {
                _logs.Clear();
                _logs.AddRange(logs.Data);
                ResetList();
            }
            else
            {
                DialogHelper.ShowErrorMess("Lấy dữ liệu log thất bại.");
            }
        }

        private void ShowException()
        {
            var selectedLog = Logs.FirstOrDefault(l => l.Index == SelectedIds.FirstOrDefault());
            var exceptions = selectedLog?.Data.Exception?.Split("\n") ?? Array.Empty<string>();
            DialogHelper.ShowErrors(exceptions.Select(e => e.Trim()).ToList());
        }

        private void ResetList()
        {
            Key = string.Empty;
            Logs.Clear();
            var items = DataGridItem<LogListItemDto>.MapFromList(_logs);
            foreach (var item in items)
            {
                Logs.Add(item);
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DataGridItem<LogListItemDto>.IsSelected))
                    {
                        SelectedIds.Clear();
                        foreach (var item in Logs.Where(i => i.IsSelected))
                        {
                            SelectedIds.Add(item.Index);
                        }
                        OnPropertyChanged(nameof(ShowExceptionButton));
                    }
                };
            }
        }

        private bool CanSearchLogsByUserId() => !IsSearching;

        private void SearchLogsByUserId()
        {
            Logs.Clear();
            var items = DataGridItem<LogListItemDto>.MapFromList(_logs.Where(l => l.UserId == Key));
            foreach (var item in items)
            {
                Logs.Add(item);
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DataGridItem<LogListItemDto>.IsSelected))
                    {
                        SelectedIds.Clear();
                        foreach (var item in Logs.Where(i => i.IsSelected))
                        {
                            SelectedIds.Add(item.Index);
                        }
                        OnPropertyChanged(nameof(ShowExceptionButton));
                    }
                };
            }
        }

        [ObservableProperty]
        private DateTime? selectedDate;
        partial void OnSelectedDateChanged(DateTime? value) => LoadData();

        [ObservableProperty]
        private bool isSearching;
        [ObservableProperty]
        private string key;
        [ObservableProperty]
        private ObservableCollection<int> selectedIds = new();
        [ObservableProperty]
        private ObservableCollection<DataGridItem<LogListItemDto>> logs = new();


        public Visibility ShowExceptionButton => SelectedIds.Count == 1 ? Visibility.Visible : Visibility.Hidden;
        public string SearchButtonText => IsSearching ? "Hủy tìm kiếm" : "Lọc theo User ID";
        public bool CanEditSearchText => !IsSearching;
        public bool IsLoading => _taskStatusStore.Loading;


        public ICommand SearchCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowExceptionCommand { get; }

    }
}
