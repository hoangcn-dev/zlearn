using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.CatalogContext.Answers;

namespace Zlearn.V2.Application.Quizzes.Commands.Create
{
    public class CreateQuizCommandHandler : BaseCommandHandler<Quiz>, IRequestHandler<CreateQuizCommand, CreateResponseDto>
    {
        private readonly IQuizWriteRepo _quizWriteRepo;
        private readonly IFileRepo _fileRepo;
        private readonly IWriteRepo<Category> _cateRepo;

        public CreateQuizCommandHandler(
            IQuizWriteRepo quizWriteRepo,
            IFileRepo fileRepo,
            IWriteRepo<Category> cateRepo,
            IMapper mapper,
            IMediator mediator) : base(quizWriteRepo, mapper, mediator)
        {
            _quizWriteRepo = quizWriteRepo;
            _fileRepo = fileRepo;
            _cateRepo = cateRepo;
        }

        public async Task<CreateResponseDto> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;

            // Check category existence
            if (!await _cateRepo.AnyAsync(c => c.Id == data.CategoryId))
                throw new ArgumentException("Danh mục không tồn tại.");

            // Check quiz name uniqueness in the category
            if (await _quizWriteRepo.AnyAsync(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Đề với tên '{data.Name}' đã tồn tại trong danh mục bạn chọn.");

            var slug = string.IsNullOrEmpty(data.Slug) ? StringHelper.GenerateSlug(data.Name) : data.Slug;
            if (await _quizWriteRepo.AnyAsync(q => q.Slug == slug))
                throw new ArgumentException("Nhãn đã tồn tại");

            // Check question keys and media links
            var fileUrls = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.IsCorrect))
                    throw new ArgumentException($"Câu hỏi {question.Order} chưa chọn đáp án hợp lệ.");
                
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
                throw new ResourceConflictException("Tệp trùng lặp");

            if (distinctFileUrls.Count > 0)
            {
                await _fileRepo.CheckExistingByFileUrls(distinctFileUrls);
                await _fileRepo.SetUsing(fileUrls);
            }

            // Create Quiz Domain entity
            var quiz = new Quiz(
                IdGenerator.Generate("QUI"),
                data.Name,
                slug,
                data.CategoryId,
                data.IsPublic
            );

            // Populate Questions and Answers via business methods to raise domain events
            var sortedQuestions = data.Questions.OrderBy(q => q.Order).ToList();
            for (int i = 0; i < sortedQuestions.Count; i++)
            {
                var q = sortedQuestions[i];
                var question = new Question
                {
                    Id = IdGenerator.Generate("QUE"),
                    Order = i + 1,
                    Slug = StringHelper.GenerateUniqueSlug(q.StringContent ?? $"Câu hỏi {i + 1}"),
                    StringContent = q.StringContent,
                    MediaFileUrls = q.MediaFileUrls != null ? string.Join(",", q.MediaFileUrls) : string.Empty,
                    Explanation = q.Explanation,
                    Answers = q.Answers.Select(a => new Answer
                    {
                        Id = IdGenerator.Generate("ANS"),
                        Key = a.Key,
                        StringContent = a.StringContent,
                        MediaFileUrls = a.MediaFileUrls != null ? string.Join(",", a.MediaFileUrls) : string.Empty,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                };
                quiz.AddQuestion(question);
            }

            // Set tags
            await _quizWriteRepo.SetQuestionsTagAsync(quiz, data.Tags);

            _quizWriteRepo.Create(quiz);
            await _quizWriteRepo.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CreateResponseDto>(quiz);
        }
    }
}

