using ZLearn.Application.Common.Queries;
using ZLearn.Application.Files;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetUpdateQuizContent
{
    public class GetUpdateQuizContentQueryHandler : BaseQueryHandler, IRequestHandler<GetUpdateQuizContentQuery, UpdateQuizDto>
    {
        private readonly IQuizRepo _quizRepo;
        private readonly IFileRepo _fileRepo;

        public GetUpdateQuizContentQueryHandler(
            IMapper mapper, IMediator mediator,
            IQuizRepo quizRepo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
            _fileRepo = fileRepo;
        }

        public async Task<UpdateQuizDto> Handle(GetUpdateQuizContentQuery request, CancellationToken cancellationToken)
        {
            if (!await _quizRepo.Any(q => q.CreatedBy == request.OwnerId && q.Id == request.Id))
                throw new UnauthorizedAccessException("You do not have permission to access this quiz.");
            var quizData = await _quizRepo.GetFullQuizContent(request.Id)
                ?? throw new NotFoundException(nameof(Quiz), request.Id);
            quizData.Questions.Sort((a, b) => a.Order - b.Order);
            return _mapper.Map<UpdateQuizDto>(quizData);
        }
    }
}
