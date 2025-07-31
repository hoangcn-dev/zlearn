using AutoMapper;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Files;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommandHandler: BaseCommandHandler, IRequestHandler<CreateCateCommand, CreateResponseDto>
    {
        private readonly ICateRepo _repo;
        private readonly IFileRepo _fileRepo;

        public CreateCateCommandHandler(
            IMapper mapper, IMediator mediator,
            ICateRepo repo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _repo = repo;
            _fileRepo = fileRepo;
        }

        public async Task<CreateResponseDto> Handle(CreateCateCommand request, CancellationToken cancellationToken)
        {
            if (await _repo.IsNameExists(request.Name))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Name));
            if (!await _fileRepo.Any(f => f.Id == request.ThumbnailId))
                throw new NotFoundException(nameof(File), request.ThumbnailId);

            var cate = new Category
            {
                Id = IdGenerator.Generate("CAT"),
                Name = request.Name,
                ThumbnailId = request.ThumbnailId,
                Description = request.Description
            };

            cate.AddEvent(new CateCreatedEvent(cate));
            _repo.Create(cate);
            await _repo.SaveChanges();

            return _mapper.Map<CreateResponseDto>(cate);
        }
    }
}
