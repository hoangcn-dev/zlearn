using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetQuestionContent
{
    public class GetQuestionContentQuery : IRequest<QuestionContentDto>
    {
        public string? Id { get; set; }
        public string? Slug { get; set; }
        public int? Order { get; set; }
        public string? QuizId { get; set; }
    }

    public class GetQuestionContentQueryHandler : IRequestHandler<GetQuestionContentQuery, QuestionContentDto>
    {
        private readonly IReadRepo<QuizDocument> _quizReadRepo;

        public GetQuestionContentQueryHandler(IReadRepo<QuizDocument> quizReadRepo)
        {
            _quizReadRepo = quizReadRepo;
        }

        public async Task<QuestionContentDto> Handle(GetQuestionContentQuery request, CancellationToken cancellationToken)
        {
            QuizDocument? quiz = null;
            QuestionDocumentItem? question = null;

            if (!string.IsNullOrEmpty(request.Slug))
            {
                var quizzes = await _quizReadRepo.GetAllAsync(q => q.Questions.Any(qt => qt.Slug == request.Slug));
                quiz = quizzes.FirstOrDefault();
                question = quiz?.Questions.FirstOrDefault(qt => qt.Slug == request.Slug);
            }
            else if (!string.IsNullOrEmpty(request.Id))
            {
                var targetId = request.Id.ToUpper();
                var quizzes = await _quizReadRepo.GetAllAsync(q => q.Questions.Any(qt => qt.Id == targetId));
                quiz = quizzes.FirstOrDefault();
                question = quiz?.Questions.FirstOrDefault(qt => qt.Id == targetId);
            }
            else if (request.Order.HasValue && !string.IsNullOrEmpty(request.QuizId))
            {
                quiz = await _quizReadRepo.GetByIdAsync(request.QuizId);
                question = quiz?.Questions.FirstOrDefault(qt => qt.Order == request.Order.Value);
            }

            if (quiz == null || question == null)
            {
                throw new NotFoundException("Question not found");
            }

            var data = new QuestionContentDto
            {
                Id = question.Id,
                StringContent = question.StringContent,
                QuizId = quiz.Id,
                Slug = question.Slug,
                QuizName = quiz.Name,
                QuizSlug = quiz.Slug,
                AttemptCount = question.AttemptCount,
                Order = question.Order,
                Answers = question.Answers.Select((a, idx) => new AnswerContentDto
                {
                    Key = a.Key,
                    StringContent = a.StringContent,
                    ImageUrls = a.MediaFileUrls.Split(',').Select(url => url.Trim()).Where(url => !string.IsNullOrEmpty(url)).ToList(),
                    Label = ((char)('A' + idx)).ToString()
                }).ToList(),
                IsMultipleChoice = question.Answers.Count(a => a.IsCorrect) > 1
            };

            if (!string.IsNullOrEmpty(question.MediaFileUrls))
            {
                foreach (var rawUrl in question.MediaFileUrls.Split(','))
                {
                    var url = rawUrl.Trim();
                    if (string.IsNullOrEmpty(url)) continue;
                    
                    var lowerUrl = url.ToLowerInvariant();
                    if (lowerUrl.EndsWith(".jpg") || lowerUrl.EndsWith(".jpeg") || lowerUrl.EndsWith(".png") || lowerUrl.EndsWith(".gif") || lowerUrl.EndsWith(".webp") || lowerUrl.EndsWith(".svg"))
                    {
                        data.ImageUrls.Add(url);
                    }
                    else if (lowerUrl.EndsWith(".mp3") || lowerUrl.EndsWith(".wav") || lowerUrl.EndsWith(".ogg"))
                    {
                        data.AudioUrls.Add(url);
                    }
                    else if (lowerUrl.EndsWith(".mp4") || lowerUrl.EndsWith(".webm") || lowerUrl.EndsWith(".avi"))
                    {
                        data.VideoUrls.Add(url);
                    }
                }
            }

            return data;
        }
    }
}
