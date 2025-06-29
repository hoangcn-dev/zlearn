using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Model;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommandHandler : IRequestHandler<UpdateCateCommand, UpdateResponseDto>
    {
        private readonly ICateRepo _cateRepo;
        private readonly IMapper _mapper;

        public UpdateCateCommandHandler(ICateRepo cateRepo, IMapper mapper)
        {
            _cateRepo = cateRepo;
            _mapper = mapper;
        }

        public async Task<UpdateResponseDto> Handle(UpdateCateCommand request, CancellationToken cancellationToken)
        {
            if (await _cateRepo.IsNameExists(request.Name)) 
                throw new DuplicateEntryException(nameof(Category), request.Name);

            var cate = await _cateRepo.Get(request.CateId)
                ?? throw new NotFoundException(nameof(Category), request.CateId);

            cate.Name = request.Name;
            cate.AddEvent(new CateUpdatedEvent(cate));
            _cateRepo.Update(cate);
            await _cateRepo.SaveChanges();

            return _mapper.Map<UpdateResponseDto>(cate);
        }
    }
}
