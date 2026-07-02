using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetWaitExamInfo
{
    public class GetWaitExamInfoQuery : IRequest<WaitExamInfoDto>
    {
        public string ParticipantId { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }

    public class GetWaitExamInfoQueryHandler : IRequestHandler<GetWaitExamInfoQuery, WaitExamInfoDto>
    {
        private readonly IExamRepo _examRepo;
        private readonly IConfiguration _configuration;

        public GetWaitExamInfoQueryHandler(
            IExamRepo examRepo,
            IConfiguration configuration)
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


