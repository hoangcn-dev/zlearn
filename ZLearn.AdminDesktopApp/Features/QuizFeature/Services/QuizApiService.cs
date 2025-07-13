using System.Net.Http;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.Commands.UpdateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.Services
{
    public interface IQuizApiService
    {
        Task<Result<List<CateListItemDto>>> GetAllCategoriesAsync();
        Task<Result<CreateResponseDto>> CreateNewCategoryAsync(string name);
        Task<Result<DeleteResponseDto>> DeleteCategoryAsync(IEnumerable<string> cateIds);
        Task<Result<UpdateResponseDto>> UpdateCategoryAsync(string cateId, string name);
        Task<Result<CateDetailDto>> GetCategoryDetailAsync(string cateId);
    }

    public class QuizApiService : BaseApiService, IQuizApiService
    {
        public QuizApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<List<CateListItemDto>>> GetAllCategoriesAsync()
        {
            var res = await GetAsync<List<CateListItemDto>>("quizzies/categories");
            return res;
        }

        public async Task<Result<CateDetailDto>> GetCategoryDetailAsync(string cateId)
        {
            var res = await GetAsync<CateDetailDto>($"quizzies/categories/{cateId}");
            return res;
        }

        public async Task<Result<CreateResponseDto>> CreateNewCategoryAsync(string name)
        {
            var data = new CreateCateCommand { Name = name };
            var res = await PostAsync<CreateResponseDto>("quizzies/categories", data);
            return res;
        }

        public async Task<Result<DeleteResponseDto>> DeleteCategoryAsync(IEnumerable<string> cateIds)
        {
            var data = new DeleteRequestDto { Ids = cateIds.ToList() };
            var res = await PostAsync<DeleteResponseDto>($"quizzies/categories/delete", data);
            return res;
        }

        public async Task<Result<UpdateResponseDto>> UpdateCategoryAsync(string cateId, string name)
        {
            var data = new UpdateCateCommand { Name = name };
            var res = await PutAsync<UpdateResponseDto>($"quizzies/categories/{cateId}", data);
            return res;
        }
    }
}
