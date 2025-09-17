using System.Security.Claims;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Queries;
namespace ZLearn.Application.Exams.Queries.GetExamScoreExcelFileData
{
    public class GetExamScoreExcelFileDataQueryHandler : BaseQueryHandler, IRequestHandler<GetExamScoreExcelFileDataQuery, FileDataDto>
    {
        private readonly IExamRepo _examRepo;

        public GetExamScoreExcelFileDataQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }

        public async Task<FileDataDto> Handle(GetExamScoreExcelFileDataQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserClaims.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var data = await _examRepo.GetExamScoreAsExcel(request.ExamId, userId);
            return data;
        }
    }
}
