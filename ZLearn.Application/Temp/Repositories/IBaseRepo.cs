using ZLearn.Application.Temp.DTOs;

namespace ZLearn.Application.Temp.Repositories
{
    public interface IBaseRepo<TModel> where TModel : class
    {
        Task<CreateResultDTO> Create(TModel model);
        Task<List<TModel>> GetAll();
        Task<TModel?> Get(string id);
        Task<UpdateResultDTO> Update(TModel model);
        Task<DeleteResultDto> Delete(TModel model);
        Task SaveDbChanges();
    }
}
