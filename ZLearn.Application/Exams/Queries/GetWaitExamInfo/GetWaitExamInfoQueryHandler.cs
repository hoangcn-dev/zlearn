using Microsoft.Extensions.Configuration;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Queries.GetWaitExamInfo
{
    public class GetWaitExamInfoQueryHandler : BaseQueryHandler, IRequestHandler<GetWaitExamInfoQuery, WaitExamInfoDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;

        public GetWaitExamInfoQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo,
            IConfiguration configuration) : base(mapper, mediator)
        {
            _examRepo = examRepo;
            _configuration = configuration;
        }

        public async Task<WaitExamInfoDto> Handle(GetWaitExamInfoQuery request, CancellationToken cancellationToken)
        {
            var data = await _examRepo.Get(
                filter: e => e.Alias == request.Alias,
                projector: e => new WaitExamInfoDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Alias = e.Alias,
                    JoinUrl = $"{_configuration["Common:BaseUrl"]}/bai-kiem-tra/join?alias={e.Alias}",
                    Note = e.Note ?? string.Empty,
                    RequireJoinWithCode = e.RequireJoinWithCode,
                    RequireJoinWithName = e.RequireJoinWithName,
                    RequirePassword = !string.IsNullOrEmpty(e.JoinPass)
                });
            return data ?? throw new NotFoundException(nameof(Exam));
        }
    }
}
