using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;
using Zlearn.V2.Domain.CatalogContext.Questions.Events;

namespace Zlearn.V2.Application.Quizzes.Queries.GetQuestionAnswerKey
{
    public class GetQuestionAnswerKeyQuery : IRequest<CorrectAnswerKeyDto>
    {
        public string QuestionId { get; set; } = null!;
    }

    public class GetQuestionAnswerKeyQueryHandler : IRequestHandler<GetQuestionAnswerKeyQuery, CorrectAnswerKeyDto>
    {
        private readonly IReadRepo<QuizDocument> _quizReadRepo;
        private readonly IWriteRepo<Zlearn.V2.Domain.CatalogContext.Questions.Question> _questionWriteRepo;

        public GetQuestionAnswerKeyQueryHandler(
            IReadRepo<QuizDocument> quizReadRepo, 
            IWriteRepo<Zlearn.V2.Domain.CatalogContext.Questions.Question> questionWriteRepo)
        {
            _quizReadRepo = quizReadRepo;
            _questionWriteRepo = questionWriteRepo;
        }

        public async Task<CorrectAnswerKeyDto> Handle(GetQuestionAnswerKeyQuery request, CancellationToken cancellationToken)
        {
            var targetId = request.QuestionId.ToUpper();
            
            // Increment attempt count on the Question via WriteRepo
            var questionEntity = await _questionWriteRepo.GetByIdAsync(targetId);
            if (questionEntity != null)
            {
                questionEntity.IncAttemptCount();
                await _questionWriteRepo.SaveChangesAsync(cancellationToken);
            }

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
