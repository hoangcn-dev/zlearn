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
            else
            {
                filterBuilder.AndCondition(q => q.Order == request.Order && q.QuizId == request.QuizId);
            }
            var question = await _questionRepo.GetQuestionWithAnswers(filterBuilder.GetPredicateOrDefault())
                ?? throw new NotFoundException(nameof(Question), request.Id);
            // Get file urls from file ids in question's content
            var fileIds = new List<string>();
            fileIds.AddRange(question.MediaFileIds.Split(","));
            question.Answers.ForEach(a => fileIds.AddRange(a.MediaFileIds.Split(",")));
            fileIds.RemoveAll(id => string.IsNullOrEmpty(id));
            var files = await _fileRepo.GetFileUrlsAsync(fileIds);
            var data = new QuestionContentDto()
            {
                Id = question.Id,
                StringContent = question.StringContent,
                QuizId = question.QuizId,
                QuizName = question.Quiz.Name,
                AttemptCount = question.AttemptCount,
                ImageUrls = question.MediaFileIds.Split(",")
                    .Where(id => files.ContainsKey(id))
                    .Select(id => files[id])
                    .Where(file => file.Type == MediaType.Image)
                    .Select(file => file.SourceUrl)
                    .ToList(),
                AudioUrls = question.MediaFileIds.Split(",")
                    .Where(id => files.ContainsKey(id))
                    .Select(id => files[id])
                    .Where(file => file.Type == MediaType.Audio)
                    .Select(file => file.SourceUrl)
                    .ToList(),
                VideoUrls = question.MediaFileIds.Split(",")
                    .Where(id => files.ContainsKey(id))
                    .Select(id => files[id])
                    .Where(file => file.Type == MediaType.Video)
                    .Select(file => file.SourceUrl)
                    .ToList(),
                Order = question.Order,
                Answers = question.Answers.Select(a => new AnswerContentDto
                {
                    Key = a.Key,
                    StringContent = a.StringContent,
                    ImageUrls = a.MediaFileIds.Split(",")
                        .Where(id => files.ContainsKey(id))
                        .Select(id => files[id])
                        .Where(file => file.Type == MediaType.Image)
                        .Select(file => file.SourceUrl)
                        .ToList()
                }).ToList()
            };
            // Create label
            for (int i = 0; i < data.Answers.Count; i++)
            {
                data.Answers[i].Label = StringHelper.IndexToChar(i);
            }
            return data;
        }
    }
}
