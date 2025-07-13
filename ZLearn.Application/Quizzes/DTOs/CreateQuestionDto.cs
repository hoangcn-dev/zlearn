using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
        public string? StringContent { get; set; }
        public List<string> ImageIds { get; set; }
        public List<string> AudioIds { get; set; }
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<CreateAnswerDto> Answers { get; set; }
    }
}
