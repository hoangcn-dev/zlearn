using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ZLearn.API.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.CatalogContext.Answers;
using Zlearn.V2.Domain.CatalogContext.Questions.Events;
using Zlearn.V2.Domain.CatalogContext.Answers.Events;

namespace Zlearn.V2.Application.Quizzes.Commands.Update
{
    public class UpdateQuizCommandHandler : BaseCommandHandler<Quiz>, IRequestHandler<UpdateQuizCommand, UpdateResponseDto>
    {
        private readonly IQuizWriteRepo _quizWriteRepo;
        private readonly IFileRepo _fileRepo;
        private readonly IWriteRepo<Category> _cateRepo;
        private readonly IWriteRepo<Question> _questionRepo;
        private readonly IWriteRepo<Answer> _answerRepo;

        public UpdateQuizCommandHandler(
            IQuizWriteRepo quizWriteRepo,
            IFileRepo fileRepo,
            IWriteRepo<Category> cateRepo,
            IWriteRepo<Question> questionRepo,
            IWriteRepo<Answer> answerRepo,
            IMapper mapper,
            IMediator mediator) : base(quizWriteRepo, mapper, mediator)
        {
            _quizWriteRepo = quizWriteRepo;
            _fileRepo = fileRepo;
            _cateRepo = cateRepo;
            _questionRepo = questionRepo;
            _answerRepo = answerRepo;
        }

        public async Task<UpdateResponseDto> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
        {
            // Check if user has permission (ownership check matching V1)
            if (!await _quizWriteRepo.AnyAsync(q => q.CreatedBy == request.OwnerId))
                throw new UnauthorizedAccessException("You do not have permission to access this quiz.");

            // Check if quiz exists
            var quiz = await _quizWriteRepo.GetFullQuizContent(request.Data.Id ?? string.Empty)
                ?? throw new NotFoundException(nameof(Quiz), request.Data.Id ?? string.Empty);

            var data = request.Data;

            // Check category existence
            if (!await _cateRepo.AnyAsync(c => c.Id == data.CategoryId))
                throw new ArgumentException($"Category with ID {data.CategoryId} does not exist.");

            // Check uniqueness constraints
            if (quiz.Name != data.Name && await _quizWriteRepo.AnyAsync(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Quiz with name '{data.Name}' already exists in category with ID {data.CategoryId}.");

            if (quiz.Slug != data.Slug && await _quizWriteRepo.AnyAsync(q => q.Slug == data.Slug))
                throw new ArgumentException($"Quiz with slug '{data.Slug}' already exists");

            // Check question keys and media links
            var fileUrls = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.IsCorrect))
                    throw new ArgumentException($"Question {question.Order} does not have a valid correct answer.");

                if (question.MediaFileUrls != null && question.MediaFileUrls.Count > 0)
                    fileUrls.AddRange(question.MediaFileUrls);

                question.Answers.ForEach(a =>
                {
                    if (a.MediaFileUrls != null && a.MediaFileUrls.Count > 0)
                        fileUrls.AddRange(a.MediaFileUrls);
                });
            }

            var distinctFileUrls = fileUrls.ToHashSet();
            if (distinctFileUrls.Count != fileUrls.Count)
                throw new ResourceConflictException("Each file must be used one time");

            if (distinctFileUrls.Count > 0)
            {
                await _fileRepo.CheckExistingByFileUrls(distinctFileUrls);
            }

            // Remove files that are no longer used
            var fileUrlsToRemove = new List<string>();
            foreach (var q in quiz.Questions)
            {
                if (!string.IsNullOrEmpty(q.MediaFileUrls))
                    fileUrlsToRemove.AddRange(q.MediaFileUrls.Split(","));
                foreach (var a in q.Answers)
                {
                    if (!string.IsNullOrEmpty(a.MediaFileUrls))
                        fileUrlsToRemove.AddRange(a.MediaFileUrls.Split(","));
                }
            }
            var filesToDelete = fileUrlsToRemove.Where(url => !distinctFileUrls.Contains(url) && !string.IsNullOrEmpty(url)).ToList();
            if (filesToDelete.Count > 0)
            {
                await _fileRepo.DeleteFileByUrls(filesToDelete);
            }

            if (fileUrls.Count > 0)
            {
                await _fileRepo.SetUsing(fileUrls);
            }

            // Update quiz state and raise domain event
            quiz.Update(
                data.Name,
                string.IsNullOrEmpty(data.Slug) ? StringHelper.GenerateSlug(data.Name) : data.Slug,
                data.CategoryId,
                quiz.IsPublic
            );

            // Update tags
            await _quizWriteRepo.SetQuestionsTagAsync(quiz, data.Tags);

            // Update questions and answers using business methods to raise domain events
            var questionsMap = quiz.Questions.ToDictionary(q => q.Id, q => q, StringComparer.OrdinalIgnoreCase);
            var answersMap = quiz.Questions.SelectMany(q => q.Answers).ToDictionary(a => a.Id, a => a, StringComparer.OrdinalIgnoreCase);

            // Sort incoming questions by their client-side order first to preserve the layout
            var sortedQuestions = data.Questions.OrderBy(q => q.Order).ToList();

            // Identify deleted items (orphans)
            var dataQuestionIds = sortedQuestions.Select(dq => dq.Id).Where(id => !string.IsNullOrEmpty(id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var questionsToDelete = questionsMap.Values.Where(q => !dataQuestionIds.Contains(q.Id)).ToList();

            var dataAnswerIds = sortedQuestions.SelectMany(dq => dq.Answers).Select(da => da.Id).Where(id => !string.IsNullOrEmpty(id)).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var answersToDelete = answersMap.Values.Where(a => !dataAnswerIds.Contains(a.Id)).ToList();

            // Prepare new questions to be added
            var newQuestions = new List<Question>();
            for (int i = 0; i < sortedQuestions.Count; i++)
            {
                var q = sortedQuestions[i];
                if (string.IsNullOrEmpty(q.Id))
                {
                    newQuestions.Add(new Question
                    {
                        Id = IdGenerator.Generate("QUE"),
                        Order = i + 1,
                        StringContent = q.StringContent,
                        Explanation = q.Explanation,
                        Slug = string.IsNullOrEmpty(q.Slug) ? StringHelper.GenerateUniqueSlug(q.StringContent ?? $"Câu hỏi {i + 1}") : q.Slug,
                        MediaFileUrls = q.MediaFileUrls != null ? string.Join(",", q.MediaFileUrls) : string.Empty,
                        Answers = q.Answers.Select(a => new Answer
                        {
                            Id = IdGenerator.Generate("ANS"),
                            Key = a.Key,
                            StringContent = a.StringContent,
                            MediaFileUrls = a.MediaFileUrls != null ? string.Join(",", a.MediaFileUrls) : string.Empty,
                            IsCorrect = a.IsCorrect
                        }).ToList()
                    });
                }
            }

            // Call business method to raise QuestionCreatedEvent, QuestionDeletedEvent, AnswerDeletedEvent
            quiz.UpdateQuestionsAndAnswers(newQuestions, questionsToDelete, answersToDelete);

            // Update existing items and add new answers to existing questions
            var remainingQuestions = new List<Question>();
            for (int i = 0; i < sortedQuestions.Count; i++)
            {
                var q = sortedQuestions[i];
                if (!string.IsNullOrEmpty(q.Id) && questionsMap.TryGetValue(q.Id, out var existingQuestion))
                {
                    existingQuestion.Order = i + 1;
                    existingQuestion.StringContent = q.StringContent;
                    existingQuestion.Slug = string.IsNullOrEmpty(q.Slug) ? StringHelper.GenerateUniqueSlug(q.StringContent ?? $"Câu hỏi {i + 1}") : q.Slug;
                    existingQuestion.MediaFileUrls = q.MediaFileUrls != null ? string.Join(",", q.MediaFileUrls) : string.Empty;
                    existingQuestion.Explanation = q.Explanation;
                    existingQuestion.Answers.Clear();

                    foreach (var a in q.Answers)
                    {
                        if (string.IsNullOrEmpty(a.Id))
                        {
                            var newAnswer = new Answer
                            {
                                Id = IdGenerator.Generate("ANS"),
                                Key = a.Key,
                                StringContent = a.StringContent,
                                MediaFileUrls = a.MediaFileUrls != null ? string.Join(",", a.MediaFileUrls) : string.Empty,
                                IsCorrect = a.IsCorrect
                            };
                            existingQuestion.Answers.Add(newAnswer);
                            quiz.RaiseEvent(new AnswerCreatedEvent(newAnswer.Id, newAnswer.Key, newAnswer.StringContent, newAnswer.IsCorrect, existingQuestion.Id));
                        }
                        else if (answersMap.TryGetValue(a.Id, out var existingAnswer))
                        {
                            existingAnswer.Key = a.Key;
                            existingAnswer.StringContent = a.StringContent;
                            existingAnswer.MediaFileUrls = a.MediaFileUrls != null ? string.Join(",", a.MediaFileUrls) : string.Empty;
                            existingAnswer.IsCorrect = a.IsCorrect;
                            existingQuestion.Answers.Add(existingAnswer);
                        }
                    }
                    remainingQuestions.Add(existingQuestion);
                }
            }

            // Execute deletions in repository
            foreach (var a in answersToDelete)
            {
                _answerRepo.Delete(a);
            }
            foreach (var q in questionsToDelete)
            {
                _questionRepo.Delete(q);
            }

            // Apply final collections back to quiz
            quiz.Questions.Clear();
            foreach (var q in remainingQuestions)
            {
                quiz.Questions.Add(q);
            }
            foreach (var q in newQuestions)
            {
                quiz.Questions.Add(q);
            }

            _quizWriteRepo.Update(quiz);
            await _quizWriteRepo.SaveChangesAsync(cancellationToken);

            return _mapper.Map<UpdateResponseDto>(quiz);
        }
    }
}
