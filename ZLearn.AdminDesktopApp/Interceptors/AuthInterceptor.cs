using Microsoft.VisualBasic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZLearn.AdminDesktopApp.Helpers;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.AdminDesktopApp.Stores;
using ZLearn.AdminDesktopApp.Views;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.Interceptors
{
    public class AuthInterceptor : DelegatingHandler
    {
        private readonly VariableStore _variableStore;
        private readonly IManageWindowService _manageWindowService;

        public AuthInterceptor(
            VariableStore variableStore, 
            IManageWindowService manageWindowService)
        {
            _variableStore = variableStore;
            _manageWindowService = manageWindowService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = _variableStore.Get<string>(VariableStore.Keys.AccessToken, false);
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _variableStore.EndSession();
                    DialogHelper.ShowErrorMess("Vui lòng đăng nhập.");
                    _manageWindowService.ShowWindow<LoginWindow>();
                });
                return new HttpResponseMessage();
            }

            return response;
        }
    }
}
