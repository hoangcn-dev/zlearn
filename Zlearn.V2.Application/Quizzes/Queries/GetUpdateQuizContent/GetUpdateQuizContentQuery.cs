using MediatR;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetUpdateQuizContent
{
    public class GetUpdateQuizContentQuery : IRequest<UpdateQuizDto>
    {
        public string Id { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
    }
}

