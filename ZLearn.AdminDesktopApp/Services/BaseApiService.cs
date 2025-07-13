using System.Net.Http;
using System.Net.Http.Json;
using ZLearn.AdminDesktopApp.Exceptions;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.Services
{
    public class BaseApiService
    {
        private readonly HttpClient _httpClient;

        public BaseApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<Result<TResponse>?> DeleteAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            if (response is not null)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<TResponse>>();
                return result;
            }
            throw new ConvertApiResultException();
        }

        public async Task<Result<TResponse>?> GetAsync<TResponse>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            if (response is not null)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<TResponse>>();
                return result;
            }
            throw new ConvertApiResultException();
        }

        public async Task<Result<TResponse>?> PostAsync<TResponse>(string endpoint, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            if (response is not null)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<TResponse>>();
                return result;
            }
            throw new ConvertApiResultException();
        }

        public async Task<Result<TResponse>?> PutAsync<TResponse>(string endpoint, object data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            if (response is not null)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<TResponse>>();
                return result;
            }
            throw new ConvertApiResultException();
        }
    }
}
