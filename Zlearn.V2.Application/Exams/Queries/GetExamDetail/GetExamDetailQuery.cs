using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQuery : IRequest<ExamDetailDto>
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }

    public class GetExamDetailQueryHandler : IRequestHandler<GetExamDetailQuery, ExamDetailDto>
    {
        private readonly IReadRepo<ExamDocument> _readRepo;
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;
        private readonly IIdentityService _identityService;

        public GetExamDetailQueryHandler(
            IReadRepo<ExamDocument> readRepo,
            IExamRepo examRepo,
            IConfiguration configuration,
            IIdentityService identityService)
        {
            _readRepo = readRepo;
            _examRepo = examRepo;
            _configuration = configuration;
            _identityService = identityService;
        }

        public async Task<ExamDetailDto> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
        {
            var docList = await _readRepo.GetAllAsync(e => e.Id.ToLower() == request.Id.ToLower() && e.CreatedBy == request.UserId);
            var doc = docList.FirstOrDefault() ?? throw new NotFoundException(nameof(Exam), request.Id);

            var participants = await _examRepo.Get(
                filter: e => e.Id.ToLower() == request.Id.ToLower(),
                projector: e => e.Participants.Select(p => new ParticipantStatusDto
                {
                    UserId = p.UserId,
                    Status = p.Status,
                    ParticipantCode = p.ParticipantCode,
                    ParticipantName = p.ParticipantName,
                    ParticipantId = p.Id,
                    CompletedCount = p.Completed
                }).ToList()) ?? new();

            var result = new ExamDetailDto
            {
                Id = doc.Id,
                Name = doc.Name,
                Alias = doc.Alias,
                JoinUrl = $"{_configuration["Common:BaseUrl"]}/bai-kiem-tra/join?alias={doc.Alias}",
                EndTime = doc.EndTime,
                JoinPass = doc.JoinPass,
                LockAccess = doc.LockAccess,
                AllowLateSubmit = doc.AllowLateSubmit,
                MixAnswers = doc.MixAnswers,
                MixQuestions = doc.MixQuestions,
                RequireJoinWithCode = doc.RequireJoinWithCode,
                RequireJoinWithName = doc.RequireJoinWithName,
                MaxParticipants = doc.MaxParticipants,
                QuizId = doc.QuizId,
                ShowAnswerAndKey = doc.ShowAnswerAndKey,
                StartTime = doc.StartTime,
                Status = Enum.TryParse<ExamStatus>(doc.Status, true, out var status) ? status : ExamStatus.WaitStart,
                JoinedParticipants = participants
            };

            var imageUrls = await _identityService.GetImageUrls(result.JoinedParticipants.Select(p => p.UserId).ToList());
            foreach (var participant in result.JoinedParticipants)
            {
                participant.ImageUrl = imageUrls.TryGetValue(participant.UserId, out var url) ? url : string.Empty;
            }

            return result;
        }
    }
}


