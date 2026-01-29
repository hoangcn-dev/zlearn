using System.Security.Claims;
using ZLearn.Application.Common.DTOs;

namespace ZLearn.Application.Exams.Queries.GetExamScoreExcelFileData
{
    public class GetExamScoreExcelFileDataQuery : IRequest<FileDataDto>
    {
        public string ExamId { get; set; }
        public ClaimsPrincipal UserClaims { get; set; }
    }
}
