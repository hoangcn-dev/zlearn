using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Application.Quizzes.DTOs;

namespace ZLearn.Application.Quizzes.Queries.GetQuestionAnswerKey
{
    public class GetQuestionAnswerKeyQuery : IRequest<CorrectAnswerKeyDto>
    {
        public string QuestionId { get; set; }
    }
}
