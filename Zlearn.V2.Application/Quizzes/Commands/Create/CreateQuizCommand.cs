using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Commands.Create
{
    public class CreateQuizCommand : IRequest<CreateResponseDto>
    {
        public CreateQuizDto Data { get; set; } = new();
    }
}
