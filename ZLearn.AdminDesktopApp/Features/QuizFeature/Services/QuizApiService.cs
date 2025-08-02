using System.Net.Http;
using ZLearn.AdminDesktopApp.Services;
using ZLearn.Application.Categories.Commands.CreateCate;
using ZLearn.Application.Categories.Commands.UpdateCate;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Quizzes.Commands.Create;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Application.Quizzes.Queries.GetListQuiz;

namespace ZLearn.AdminDesktopApp.Features.QuizFeature.Services
{
    public interface IQuizApiService
    {
        Task<Result<CreateResponseDto>> CreateNewQuizAsync(CreateQuizDto data);
        Task<Result<PaginatedDto<QuizListItemDto>>> GetListQuizzesAsync(GetListQuizQuery query);
        Task<Result<DeleteResponseDto>> DeleteQuizAsync(DeleteRequestDto data);

        Task<Result<List<CateListItemDto>>> GetAllCategoriesAsync();
        Task<Result<CreateResponseDto>> CreateNewCategoryAsync(CreateCateCommand data);
        Task<Result<DeleteResponseDto>> DeleteCategoryAsync(IEnumerable<string> cateIds);
        Task<Result<UpdateResponseDto>> UpdateCategoryAsync(string cateId, string name, string? thumbnailId, string? description);
        Task<Result<CateDetailDto>> GetCategoryDetailAsync(string cateId);
        Task<Result<List<string>>> GetAllTagsAsync();
    }

    public class QuizApiService : BaseApiService, IQuizApiService
    {
        public QuizApiService(IHttpClientFactory httpClientFactory) : base(httpClientFactory)
        {
        }

        public async Task<Result<List<CateListItemDto>>> GetAllCategoriesAsync()
        {
            var res = await GetAsync<List<CateListItemDto>>("quizzes/categories");
            return res;
        }

        public async Task<Result<CateDetailDto>> GetCategoryDetailAsync(string cateId)
        {
            var res = await GetAsync<CateDetailDto>($"quizzes/categories/{cateId}");
            return res;
        }

        public async Task<Result<CreateResponseDto>> CreateNewCategoryAsync(CreateCateCommand data)
        {
            var res = await PostAsync<CreateResponseDto>("quizzes/categories", data);
            return res;
        }

        public async Task<Result<DeleteResponseDto>> DeleteCategoryAsync(IEnumerable<string> cateIds)
        {
            var data = new DeleteRequestDto { Ids = cateIds.ToList() };
            var res = await PostAsync<DeleteResponseDto>($"quizzes/categories/delete", data);
            return res;
        }

        public async Task<Result<UpdateResponseDto>> UpdateCategoryAsync(string cateId, string name, string? thumbnailId, string description)
        {
            var data = new UpdateCateCommand { 
                Name = name, 
                Description = description,
                ThumbnailId = thumbnailId 
            };
            var res = await PutAsync<UpdateResponseDto>($"quizzes/categories/{cateId}", data);
            return res;
        }

        public async Task<Result<PaginatedDto<QuizListItemDto>>> GetListQuizzesAsync(GetListQuizQuery query)
        {
            var res = await GetAsync<PaginatedDto<QuizListItemDto>>("quizzes", query);
            return res;
        }

        public async Task<Result<List<string>>> GetAllTagsAsync()
        {
            var res = await GetAsync<List<string>>("quizzes/tags");
            return res;
        }

        public async Task<Result<CreateResponseDto>> CreateNewQuizAsync(CreateQuizDto data)
        {
            var command = new CreateQuizCommand { Data = data };
            var res = await PostAsync<CreateResponseDto>("quizzes", command);
            return res;
        }

        public Task<Result<DeleteResponseDto>> DeleteQuizAsync(DeleteRequestDto data)
        {
            var res = PostAsync<DeleteResponseDto>("quizzes/delete", data);
            return res;
        }
    }
}
