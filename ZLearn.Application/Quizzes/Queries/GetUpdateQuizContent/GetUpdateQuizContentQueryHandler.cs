using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetUpdateQuizContent
{
    public class GetUpdateQuizContentQueryHandler : BaseQueryHandler, IRequestHandler<GetUpdateQuizContentQuery, UpdateQuizDto>
    {
        private readonly IQuizRepo _quizRepo;

        public GetUpdateQuizContentQueryHandler(
            IMapper mapper, IMediator mediator, 
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<UpdateQuizDto> Handle(GetUpdateQuizContentQuery request, CancellationToken cancellationToken)
        {
            var quizData = await _quizRepo.GetFullQuizContent(request.Id)
                ?? throw new NotFoundException(nameof(Quiz), request.Id);
            return _mapper.Map<UpdateQuizDto>(quizData);
        }
    }
}
