using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetQuestionAnswerKey
{
    public class GetQuestionAnswerKeyQuery : IRequest<CorrectAnswerKeyDto>
    {
        public string QuestionId { get; set; } = null!;
    }

    public class GetQuestionAnswerKeyQueryHandler : IRequestHandler<GetQuestionAnswerKeyQuery, CorrectAnswerKeyDto>
    {
        private readonly IReadRepo<QuizDocument> _quizReadRepo;

        public GetQuestionAnswerKeyQueryHandler(IReadRepo<QuizDocument> quizReadRepo)
        {
            _quizReadRepo = quizReadRepo;
        }

        public async Task<CorrectAnswerKeyDto> Handle(GetQuestionAnswerKeyQuery request, CancellationToken cancellationToken)
        {
            var targetId = request.QuestionId.ToUpper();
            var quizzes = await _quizReadRepo.GetAllAsync(q => q.Questions.Any(qt => qt.Id == targetId));
            var quiz = quizzes.FirstOrDefault();
            var question = quiz?.Questions.FirstOrDefault(qt => qt.Id == targetId);

            if (quiz == null || question == null)
            {
                throw new NotFoundException("Question not found");
            }

            var correctKeys = question.Answers
                .Where(a => a.IsCorrect)
                .Select(a => a.Key)
                .ToList();

            return new CorrectAnswerKeyDto
            {
                QuestionId = question.Id,
                CorrectKeys = correctKeys,
                Explanation = question.Explanation
            };
        }
    }
}
