using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using ZLearn.AdminDesktopApp.Features.SystemFeature.Services;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.ViewModels;
using ZLearn.Application.Tracking.DTOs;

namespace ZLearn.AdminDesktopApp.Features.SystemFeature.ViewModels
{
    public partial class SystemStatViewModel : ViewModelBase
    {
        private HubConnection _hubConnection;
        private readonly ISystemApiService _systemApiService;
        private readonly IConfiguration _configuration;

        [ObservableProperty]
        private AccessCountStatDto history;
        [ObservableProperty]
        private long currentAccessCount = 0;

        public SystemStatViewModel(
            TaskStatusStore taskStatusStore,
            VariableStore store,
            ISystemApiService systemApiService,
            IConfiguration configuration) : base(taskStatusStore, store)
        {
            _systemApiService = systemApiService;
            _configuration = configuration;
            LoadData();
            InitSignalR();
        }

        private async void LoadData()
        {
            var res = await ExecuteAsync(() => _systemApiService.GetAccessTrackingData());
            if (res != null && res.Succeeded)
            {
                History = res.Data;
            }
            else
            {
                DialogHelper.ShowErrorMess("Lấy dữ liệu thất bại");
            }
        }

        private async void InitSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_configuration.GetSection("APIConfigs")["BaseSocketUrl"]}/access-tracking")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<object>("UpdateAccessCount", (data) =>
            {
                CurrentAccessCount = ((JsonElement)data).GetInt64();
            });

            await _hubConnection.StartAsync();
        }
    }
}
