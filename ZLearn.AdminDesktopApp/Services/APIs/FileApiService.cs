using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ZLearn.AdminDesktopApp.Exceptions;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Files.Commands.SaveFile;
using ZLearn.Application.Files.DTOs;

namespace ZLearn.AdminDesktopApp.Services
{
    public interface IFileApiService
    {
        Task<Result<ListSavedFileDto>> SaveFilesAsync(List<string> filePaths);
        Task<Result<DeleteResponseDto>> DeleteFilesAsync(List<string> fileIds);
    }

    public class FileApiService : BaseApiService, IFileApiService
    {
        public FileApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public Task<Result<DeleteResponseDto>> DeleteFilesAsync(List<string> fileIds)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<ListSavedFileDto>> SaveFilesAsync(List<string> filePaths)
        {
            using var formContent = new MultipartFormDataContent();
            for (int i = 0; i < filePaths.Count; i++)
            {
                var path = filePaths[i];
                var fileName = Path.GetFileName(path);
                var fileStream = File.OpenRead(path);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                formContent.Add(fileContent, $"Files[{i}].Data", fileName);
                formContent.Add(new StringContent(fileName), $"Files[{i}].Name");
            }

            var response = await _httpClient.PostAsync("files", formContent);
            if (response is not null)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<ListSavedFileDto>>();
                return result;
            }
            throw new ConvertApiResultException();
        }
    }
}
