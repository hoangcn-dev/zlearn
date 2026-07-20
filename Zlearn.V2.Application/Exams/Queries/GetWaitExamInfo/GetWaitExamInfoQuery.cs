using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams.DTOs;

namespace Zlearn.V2.Application.Exams.Queries.GetWaitExamInfo
{
    public class GetWaitExamInfoQuery : IRequest<WaitExamInfoDto>
    {
        public string ParticipantId { get; set; } = string.Empty;
        public string Alias { get; set; } = string.Empty;
    }

    public class GetWaitExamInfoQueryHandler : IRequestHandler<GetWaitExamInfoQuery, WaitExamInfoDto>
    {
        private readonly IReadRepo<ExamDocument> _readRepo;
        private readonly IConfiguration _configuration;

        public GetWaitExamInfoQueryHandler(
            IReadRepo<ExamDocument> readRepo,
            IConfiguration configuration)
        {
            _readRepo = readRepo;
            _configuration = configuration;
        }

        public async Task<WaitExamInfoDto> Handle(GetWaitExamInfoQuery request, CancellationToken cancellationToken)
        {
            var docs = await _readRepo.GetAllAsync(e => e.Alias == request.Alias);
            var doc = docs.FirstOrDefault() ?? throw new NotFoundException(nameof(ExamDocument));

            return new WaitExamInfoDto
            {
                Id = doc.Id,
                Name = doc.Name,
                StartTime = doc.StartTime,
                EndTime = doc.EndTime,
                Alias = doc.Alias,
                JoinUrl = $"{_configuration["Common:BaseUrl"]}/bai-kiem-tra/join?alias={doc.Alias}",
                Note = doc.Note ?? string.Empty,
                RequireJoinWithCode = doc.RequireJoinWithCode,
                RequireJoinWithName = doc.RequireJoinWithName,
                RequirePassword = !string.IsNullOrEmpty(doc.JoinPass)
            };
        }
    }
}


