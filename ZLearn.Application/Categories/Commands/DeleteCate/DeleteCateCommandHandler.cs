
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Model;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.Commands.DeleteCate
{
    public class DeleteCateCommandHandler : IRequestHandler<DeleteCateCommand, DeleteResponseDto>
    {
        private readonly ICateRepo _cateRepo;
        private readonly IMapper _mapper;

        public DeleteCateCommandHandler(ICateRepo cateRepo, IMapper mapper)
        {
            _cateRepo = cateRepo;
            _mapper = mapper;
        }

        public async Task<DeleteResponseDto> Handle(DeleteCateCommand request, CancellationToken cancellationToken)
        {
            var cates = await _cateRepo
                .GetAll(filter: e => request.CateIds.Contains(e.Id));
            cates.ForEach(cate => cate.AddEvent(new CateDeletedEvent(cate)));
            _cateRepo.Delete(cates);
            await _cateRepo.SaveChanges();

            return new DeleteResponseDto
            {
                DeletedIds = cates.Select(c => c.Id),
                DeletedAt = DateTime.UtcNow,
            };
        }
    }
}
