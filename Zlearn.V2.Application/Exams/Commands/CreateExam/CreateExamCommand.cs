using MediatR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Exams.DTOs;

namespace Zlearn.V2.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommand : IRequest<CreateResponseDto>
    {
        public CreateExamDto Data { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
    }
}


