using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Application.Quizzes.Queries.GetQuestionContent;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Queries.GetQuizDetail
{
    public class GetQuizDetailQueryHandler : BaseQueryHandler, IRequestHandler<GetQuizDetailQuery, QuizDetailDto>
    {
        private readonly IQuizRepo _quizRepo;

        public GetQuizDetailQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuizRepo quizRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
        }

        public async Task<QuizDetailDto> Handle(GetQuizDetailQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<Quiz>();
            if (!string.IsNullOrEmpty(request.Slug))
            {
                filterBuilder.AndCondition(q => q.Slug == request.Slug);
            }
            else if (!string.IsNullOrEmpty(request.Id))
            {
                filterBuilder.AndCondition(q => q.Id == request.Id);
            }
            else
            {
                throw new ArgumentException("Quiz ID or Slug must be provided.");
            }

            var quiz = await _quizRepo.Get(
                filter: filterBuilder.GetPredicateOrDefault(),
                projector: q => new QuizDetailDto
                {
                    Id = q.Id,
                    Name = q.Name,
                    Slug = q.Slug,
                    AttemptCount = 0,
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                    QuestionCount = q.Questions.Count,
                    Questions = q.Questions.Select(q => new QuestionListItemDto
                    {
                        Id = q.Id,
                        Slug = q.Slug,
                        Order = q.Order,
                        Content = q.StringContent ?? "Nội dung media",
                    }).OrderBy(q => q.Order).ToList()
                })
                ?? throw new NotFoundException(nameof(Quiz), request.Id);
            return quiz;
        }
    }
}
