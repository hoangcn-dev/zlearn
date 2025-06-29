using ZLearn.API.Exceptions;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.Queries.GetCateDetail
{
    public class GetCateDetailQueryHandler : IRequestHandler<GetCateDetailQuery, CateDetailDto>
    {
        private readonly ICateRepo _cateRepo;

        public GetCateDetailQueryHandler(ICateRepo cateRepo)
        {
            _cateRepo = cateRepo;
        }

        public async Task<CateDetailDto> Handle(GetCateDetailQuery request, CancellationToken cancellationToken)
        {
            return await _cateRepo.Get(
                id: request.CateId,
                projector: c => new CateDetailDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy,
                    LastModifiedAt = c.LastModifiedAt,
                    ModifiedBy = c.ModifiedBy,
                    QuizCount = c.Quizzes.Count,
                }) ?? throw new NotFoundException(nameof(Category), request.CateId);
        }
    }
}
