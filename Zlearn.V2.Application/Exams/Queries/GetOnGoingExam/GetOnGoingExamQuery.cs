using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetOnGoingExam
{
    public class GetOnGoingExamQuery : IRequest<List<OnGoingExamListItemDto>>
    {
        public ClaimsPrincipal UserClaims { get; set; } = null!;
    }

    public class GetOnGoingExamQueryHandler : IRequestHandler<GetOnGoingExamQuery, List<OnGoingExamListItemDto>>
    {
        private readonly IExamRepo _examRepo;

        public GetOnGoingExamQueryHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<List<OnGoingExamListItemDto>> Handle(GetOnGoingExamQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return await _examRepo.GetOnGoingExams(userId);
        }
    }
}


