using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class AutoGenerateQuestionRequestDto
    {
        public string PromptContext { get; set; }
        public IFormFile? FileData { get; set; }
    }
}
