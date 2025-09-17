using System.Security.Claims;
using ZLearn.Application.Exams.DTOs;

namespace ZLearn.Application.Exams.Queries.GetOnGoingExam
{
    public class GetOnGoingExamQuery : IRequest<List<OnGoingExamListItemDto>>
    {
        public ClaimsPrincipal UserClaims { get; set; }
    }
}
