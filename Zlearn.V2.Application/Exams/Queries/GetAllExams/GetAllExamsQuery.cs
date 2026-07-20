using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetAllExams
{
    public class GetAllExamsQuery : IRequest<List<ExamListItemDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetAllExamsQueryHandler : IRequestHandler<GetAllExamsQuery, List<ExamListItemDto>>
    {
        private readonly IReadRepo<ExamDocument> _readRepo;

        public GetAllExamsQueryHandler(IReadRepo<ExamDocument> readRepo)
        {
            _readRepo = readRepo;
        }

        public async Task<List<ExamListItemDto>> Handle(GetAllExamsQuery request, CancellationToken cancellationToken)
        {
            var docs = await _readRepo.GetAllAsync(e => e.CreatedBy == request.UserId);
            return docs.OrderByDescending(e => e.CreatedAt)
                .Select(e => new ExamListItemDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Status = Enum.TryParse<ExamStatus>(e.Status, true, out var status) ? status : ExamStatus.WaitStart
                })
                .ToList();
        }
    }
}


