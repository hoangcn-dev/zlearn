using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Queries.GetExamScore
{
    public class GetExamScoreQuery : IRequest<ExamScoreDto>
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class GetExamScoreQueryHandler : IRequestHandler<GetExamScoreQuery, ExamScoreDto>
    {
        private readonly IExamRepo _examRepo;

        public GetExamScoreQueryHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<ExamScoreDto> Handle(GetExamScoreQuery request, CancellationToken cancellationToken)
        {
            var data = await _examRepo.Get(
                filter: e => e.Id.ToLower() == request.Id.ToLower() && e.CreatedBy == request.UserId && e.Status == ExamStatus.Ended,
                projector: e => new
                {
                    ExamId = e.Id,
                    ExamName = e.Name,
                    MaxParticipants = e.MaxParticipants,
                    QuestionCount = e.Quiz.Questions.Count,
                    ParticipantScores = e.Participants
                        .Where(p => p.Status == ParticipantStatus.Completed || (e.AllowLateSubmit && p.Status == ParticipantStatus.TimeOut))
                        .Select(p => new
                        {
                            ParticipantId = p.Id,
                            ParticipantCode = p.ParticipantCode,
                            ParticipantName = p.ParticipantName,
                            Completed = p.Completed,
                            StartTime = p.FirstCheckIn!,
                            EndTime = p.LastCheckOut!,
                            Score = p.Score,
                            UserId = p.UserId,
                            Status = p.Status,
                            p.Correct
                        })
                }) ?? throw new NotFoundException("Bài kiểm tra chưa kết thúc hoặc không tồn tại.");

            return new ExamScoreDto
            {
                ExamId = data.ExamId,
                ExamName = data.ExamName,
                MaxParticipants = data.MaxParticipants,
                QuestionCount = data.QuestionCount,
                ParticipantScores = data.ParticipantScores.Select(p => new ExamParticipantScoreDto
                {
                    ParticipantId = p.ParticipantId,
                    ParticipantCode = p.ParticipantCode,
                    ParticipantName = p.ParticipantName,
                    FirstCheckIn = p.StartTime.Value,
                    LastCheckOut = p.EndTime.Value,
                    Completed = p.Completed,
                    Score = p.Score,
                    Status = (ParticipantStatus)p.Status,
                    UserId = p.UserId,
                    Duration = (long)(p.EndTime - p.StartTime).Value.TotalSeconds,
                    Correct = p.Correct
                }).ToList(),
            };
        }
    }
}


