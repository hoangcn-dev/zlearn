using Microsoft.Extensions.Configuration;
using ZLearn.Application.Common.Identity;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQueryHandler : BaseQueryHandler, IRequestHandler<GetExamDetailQuery, ExamDetailDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;

        public GetExamDetailQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IConfiguration configuration,
            IIdentityService identityService) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _configuration = configuration;
            _identityService = identityService;
        }

        public async Task<ExamDetailDto> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
        {
            var exam = await _examRepo.Get(
                filter: e => e.Id.ToLower() == request.Id && e.CreatedBy == request.UserId, 
                projector: e => new ExamDetailDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Alias = e.Alias,
                    JoinUrl = $"{_configuration["Common:BaseUrl"]}/bai-kiem-tra/join?alias={e.Alias}",
                    EndTime = e.EndTime,
                    JoinPass = e.JoinPass,
                    LockAccess = e.LockAccess,
                    AllowLateSubmit = e.AllowLateSubmit,
                    MixAnswers = e.MixAnswers,
                    MixQuestions = e.MixQuestions,
                    RequireJoinWithCode = e.RequireJoinWithCode,
                    RequireJoinWithName = e.RequireJoinWithName,
                    MaxParticipants = e.MaxParticipants,
                    QuizId = e.QuizId,
                    ShowAnswerAndKey = e.ShowAnswerAndKey,
                    StartTime = e.StartTime,
                    Status = e.Status,
                    QuestionsCount = e.Quiz.Questions.Count,
                    JoinedParticipants = e.Participants
                        .Select(p => new ParticipantStatusDto
                        {
                            UserId = p.UserId,
                            Status = p.Status,
                            ParticipantCode = p.ParticipantCode,
                            ParticipantName = p.ParticipantName,
                            ParticipantId = p.Id,
                            CompletedCount = p.Completed
                        }).ToList()
                }) ?? throw new NotFoundException(nameof(Exam), request.Id);

            // Get image urls
            var imageUrls = await _identityService.GetImageUrls(exam.JoinedParticipants.Select(p => p.UserId).ToList());
            foreach (var participant in exam.JoinedParticipants)
            {
                participant.ImageUrl = imageUrls.ContainsKey(participant.UserId) ? imageUrls[participant.UserId] : string.Empty;
            }

            return exam;
        }
    }
}
