using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Application.Exams.Queries.GetAllExams
{
    public class GetAllExamsQuery : IRequest<List<ExamListItemDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetAllExamsQueryHandler : IRequestHandler<GetAllExamsQuery, List<ExamListItemDto>>
    {
        private readonly IExamRepo _examRepo;

        public GetAllExamsQueryHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<List<ExamListItemDto>> Handle(GetAllExamsQuery request, CancellationToken cancellationToken)
        {
            var exams = await _examRepo.GetAll(
                filter: e => e.CreatedBy == request.UserId,
                projector: e => new ExamListItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Status = (ExamStatus)e.Status
                },
                orderBy: e => e.CreatedAt,
                isAsc: false);
            return exams;
        }
    }
}


