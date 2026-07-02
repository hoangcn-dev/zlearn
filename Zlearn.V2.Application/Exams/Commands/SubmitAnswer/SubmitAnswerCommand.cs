using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Commands.SubmitAnswer
{
    public class SubmitAnswerCommand : IRequest<string>
    {
        public string ParticipantId { get; set; } = string.Empty;
        public SubmitExamDto Data { get; set; } = null!;
    }

    public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand, string>
    {
        private readonly IExamRepo _examRepo;
        private readonly IExamTrackingService _examTrackingService;

        public SubmitAnswerCommandHandler(
            IExamRepo examRepo,
            IExamTrackingService examTrackingService)
        {
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task<string> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
        {
            await _examRepo.SaveResult(request.ParticipantId, request.Data);
            await _examTrackingService.UpdateParticipantStatus(request.Data.ExamId, request.ParticipantId, ParticipantStatus.Completed);
            await _examTrackingService.UpdateParticipantProgress(request.Data.ExamId, request.ParticipantId, request.Data.Answers.Count);
            return request.Data.ExamId;
        }
    }
}


