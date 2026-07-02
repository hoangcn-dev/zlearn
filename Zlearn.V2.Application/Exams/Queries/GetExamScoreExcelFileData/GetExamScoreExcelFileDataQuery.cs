using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Exams;

namespace Zlearn.V2.Application.Exams.Queries.GetExamScoreExcelFileData
{
    public class GetExamScoreExcelFileDataQuery : IRequest<FileDataDto>
    {
        public string ExamId { get; set; } = string.Empty;
        public ClaimsPrincipal UserClaims { get; set; } = null!;
    }

    public class GetExamScoreExcelFileDataQueryHandler : IRequestHandler<GetExamScoreExcelFileDataQuery, FileDataDto>
    {
        private readonly IExamRepo _examRepo;

        public GetExamScoreExcelFileDataQueryHandler(IExamRepo examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<FileDataDto> Handle(GetExamScoreExcelFileDataQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            return await _examRepo.GetExamScoreAsExcel(request.ExamId, userId);
        }
    }
}


