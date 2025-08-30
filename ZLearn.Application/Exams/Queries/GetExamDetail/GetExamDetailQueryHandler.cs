using Microsoft.Extensions.Configuration;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Queries.GetExamDetail
{
    public class GetExamDetailQueryHandler : BaseQueryHandler, IRequestHandler<GetExamDetailQuery, ExamDetailDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;

        public GetExamDetailQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IConfiguration configuration) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _configuration = configuration;
        }

        public async Task<ExamDetailDto> Handle(GetExamDetailQuery request, CancellationToken cancellationToken)
        {
            var exam = await _examRepo.Get(id: request.Id, projector: e => new ExamDetailDto
            {
                Id = e.Id,
                Name = e.Name,
                Alias = e.Alias,
                JoinUrl = $"{_configuration["Common:BaseUrl"]}/bai-kiem-tra/join?alias={e.Alias}",
                EndTime = e.EndTime,
                JoinPass = e.JoinPass,
                LockAccess = e.LockAccess,
                MaxParticipants = e.MaxParticipants,
                QuizId = e.QuizId,
                ShowAnswerAndKey = e.ShowAnswerAndKey,
                StartTime = e.StartTime,
            }) ?? throw new NotFoundException(nameof(Exam), request.Id);
            return exam;
        }
    }
}
