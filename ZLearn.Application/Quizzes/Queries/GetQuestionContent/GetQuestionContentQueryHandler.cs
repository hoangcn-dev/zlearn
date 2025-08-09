using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionContent
{
    public class GetQuestionContentQueryHandler : BaseQueryHandler, IRequestHandler<GetQuestionContentQuery, QuestionContentDto>
    {
        private readonly IQuestionRepo _questionRepo;
        private readonly IFileRepo _fileRepo;

        public GetQuestionContentQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IQuestionRepo questionRepo,
            IFileRepo fileRepo) : base(mapper, mediator)
        {
            _questionRepo = questionRepo;
            _fileRepo = fileRepo;
        }

        public async Task<QuestionContentDto> Handle(GetQuestionContentQuery request, CancellationToken cancellationToken)
        {
            var filterBuilder = new FilterBuilder<Question>();
            if (!string.IsNullOrEmpty(request.Id))
            {
                filterBuilder.AndCondition(q => q.Id == request.Id.ToUpper());
            }
            else if (!string.IsNullOrEmpty(request.Slug))
            {
                filterBuilder.AndCondition(q => q.Slug == request.Slug);
            }
            else
            {
                filterBuilder.AndCondition(q => q.Order == request.Order && q.QuizId == request.QuizId);
            }

            var question = await _questionRepo.GetQuestionWithAnswers(filterBuilder.GetPredicateOrDefault())
                ?? throw new NotFoundException(nameof(Question), request.Id);
            var data = new QuestionContentDto()
            {
                Id = question.Id,
                StringContent = question.StringContent,
                QuizId = question.QuizId,
                Slug = question.Slug,
                QuizName = question.Quiz.Name,
                AttemptCount = question.AttemptCount,
                Order = question.Order,
                Answers = question.Answers.Select(a => new AnswerContentDto
                {
                    Key = a.Key,
                    StringContent = a.StringContent,
                    ImageUrls = a.MediaFileUrls.Split(",").Where(url => !string.IsNullOrEmpty(url)).ToList()
                }).ToList()
            };
            foreach (var url in question.MediaFileUrls.Split(","))
            {
                if (string.IsNullOrEmpty(url)) continue;
                var mediaType = FileHelper.GetMediaType(url);
                if (mediaType == MediaType.Image)
                {
                    data.ImageUrls.Add(url);
                }
                else if (mediaType == MediaType.Audio)
                {
                    data.AudioUrls.Add(url);
                }
                else if (mediaType == MediaType.Video)
                {
                    data.VideoUrls.Add(url);
                }
            }

            // Create label
            for (int i = 0; i < data.Answers.Count; i++)
            {
                data.Answers[i].Label = StringHelper.IndexToChar(i);
            }
            return data;
        }
    }
}
