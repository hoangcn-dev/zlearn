using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Commands.Update
{
    public class UpdateQuizCommand : IRequest<UpdateResponseDto>
    {
        public UpdateQuizDto Data { get; set; } = new();
        public string OwnerId { get; set; } = string.Empty;
    }
}

