using AutoMapper;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Model;
using ZLearn.Application.Common.Utils;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Events.Cate;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommandHandler : IRequestHandler<CreateCateCommand, CreateResponseDto>
    {
        private readonly ICateRepo _repo;
        private readonly IMapper _mapper;

        public CreateCateCommandHandler(ICateRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CreateResponseDto> Handle(CreateCateCommand request, CancellationToken cancellationToken)
        {
            if (await _repo.IsNameExists(request.Name))
                throw new DuplicateEntryException(nameof(Category), nameof(Category.Name));

            var cate = new Category
            {
                Id = IdGenerator.Generate("CAT"),
                Name = request.Name,
            };

            cate.AddEvent(new CateCreatedEvent(cate));
            _repo.Create(cate);
            await _repo.SaveChanges();

            return _mapper.Map<CreateResponseDto>(cate);
        }
    }
}
