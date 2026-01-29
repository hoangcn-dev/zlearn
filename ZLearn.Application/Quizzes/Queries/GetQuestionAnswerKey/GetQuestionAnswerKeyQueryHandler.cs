
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionAnswerKey
{
    public class GetQuestionAnswerKeyQueryHandler : BaseQueryHandler, IRequestHandler<GetQuestionAnswerKeyQuery, CorrectAnswerKeyDto>
    {
        private readonly IQuestionRepo _questionRepo;

        public GetQuestionAnswerKeyQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuestionRepo questionRepo) : base(mapper, mediator)
        {
            _questionRepo = questionRepo;
        }

        public async Task<CorrectAnswerKeyDto> Handle(GetQuestionAnswerKeyQuery request, CancellationToken cancellationToken)
        {
            var key = await _questionRepo.GetCorrectAnswerKeyAsync(request.QuestionId)
                ?? throw new NotFoundException(nameof(Question), request.QuestionId);
            return key;
        }
    }
}
