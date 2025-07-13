using ZLearn.API.Exceptions;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Common.Queries;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.Queries.GetCateById
{
    public class GetCateByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetCateByIdQuery, CateDetailDto>
    {
        private readonly ICateRepo _cateRepo;

        public GetCateByIdQueryHandler(
            IMapper mapper, IMediator mediator, 
            ICateRepo cateRepo) : base(mapper, mediator)
        {
            _cateRepo = cateRepo;
        }

        public async Task<CateDetailDto> Handle(GetCateByIdQuery request, CancellationToken cancellationToken)
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
