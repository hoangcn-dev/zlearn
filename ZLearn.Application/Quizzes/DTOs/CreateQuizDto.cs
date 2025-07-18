
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class CreateQuizDto
    {
        public string Name { get; set; }
        public string CategoryId { get; set; }
        public bool IsPublic { get; set; }
        public List<string> Tags { get; set; }
        public List<CreateQuestionDto> Questions { get; set; }
    }

    
}
