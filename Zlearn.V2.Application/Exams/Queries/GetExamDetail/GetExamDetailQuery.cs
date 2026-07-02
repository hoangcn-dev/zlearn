using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQuery : IRequest<ExamDetailDto>
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class GetExamDetailQueryHandler : IRequestHandler<GetExamDetailQuery, ExamDetailDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;

        public GetExamDetailQueryHandler(
            IExamRepo examRepo,
            IConfiguration configuration,
            IIdentityService identityService)
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
                    Status = (ExamStatus)e.Status,
                    QuestionsCount = e.Quiz.Questions.Count,
                    JoinedParticipants = e.Participants
                        .Select(p => new ParticipantStatusDto
                        {
                            UserId = p.UserId,
                            Status = (ParticipantStatus)p.Status,
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
                participant.ImageUrl = imageUrls.TryGetValue(participant.UserId, out var url) ? url : string.Empty;
            }

            return exam;
        }
    }
}


