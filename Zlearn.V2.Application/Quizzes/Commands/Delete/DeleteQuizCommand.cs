using System.Collections.Generic;
using MediatR;
using Zlearn.V2.Application.Common.DTOs;

namespace Zlearn.V2.Application.Quizzes.Commands.Delete
{
    public class DeleteQuizCommand : IRequest<DeleteResponseDto>
    {
        public List<string> Ids { get; set; } = new();
        public string OwnerId { get; set; } = string.Empty;
    }
}

