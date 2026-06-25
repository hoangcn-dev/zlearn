using ZLearn.API.Exceptions;
using ZLearn.Application.Categories;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Commands.Create
{
    public class CreateQuizCommandHandler : BaseCommandHandler, IRequestHandler<CreateQuizCommand, CreateResponseDto>
    {
        private readonly IQuizRepo _quizRepo;
        private readonly IFileRepo _fileRepo;
        private readonly ICateRepo _cateRepo;

        public CreateQuizCommandHandler(
            IMapper mapper, IMediator mediator,
            IQuizRepo quizRepo,
            IFileRepo fileRepo, 
            ICateRepo cateRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
            _fileRepo = fileRepo;
            _cateRepo = cateRepo;
        }

        public async Task<CreateResponseDto> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
        {
            var data = request.Data;
            
            // Check category existence and quiz uniqueness
            if (!await _cateRepo.Any(c => c.Id == data.CategoryId))
                throw new ArgumentException($"Danh mục không tồn tại.");
            if (await _quizRepo.Any(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Đề với tên '{data.Name}' đã tồn tại trong danh mục bạn chọn.");
            if (await _quizRepo.Any(q => q.Slug == data.Slug))
                throw new ArgumentException($"Nhãn đã tồn tại");
            // Check question keys and media links
            var fileUrls = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.IsCorrect))
                    throw new ArgumentException($"Câu hỏi {question.Order} chưa chọn đáp án hợp lệ.");
                if (question.MediaFileUrls.Count > 0) fileUrls.AddRange(question.MediaFileUrls);
                question.Answers.ForEach(a =>
                {
                    if (a.MediaFileUrls.Count > 0)
                        fileUrls.AddRange(a.MediaFileUrls);
                });
            }

            var distinctFileUrls = fileUrls.ToHashSet();
            if (distinctFileUrls.Count != fileUrls.Count)
                throw new ResourceConflictException("Tệp trùng lặp");
            await _fileRepo.CheckExistingByFileUrls(distinctFileUrls);
            await _fileRepo.SetUsing(fileUrls);

            // Add new question
            var quiz = new Quiz
            {
                Id = IdGenerator.Generate("QUI"),
                Name = data.Name,
                Slug = string.IsNullOrEmpty(data.Slug) ? StringHelper.GenerateSlug(data.Name) : data.Slug,
                IsPublic = data.IsPublic,
                CategoryId = data.CategoryId,
            };
            await _quizRepo.SetQuestionsTagAsync(quiz, data.Tags);
            quiz.Questions = data.Questions.Select(q => new Question
            {
                Id = IdGenerator.Generate("QUE"),
                Order = q.Order,
                Slug = StringHelper.GenerateUniqueSlug(q.StringContent ?? $"Câu hỏi {q.Order}"),
                StringContent = q.StringContent,
                MediaFileUrls = string.Join(",", q.MediaFileUrls),
                Explanation = q.Explanation,
                Answers = q.Answers.Select(a => new Answer
                {
                    Id = IdGenerator.Generate("ANS"),
                    Key = a.Key,
                    StringContent = a.StringContent,
                    MediaFileUrls = string.Join(",", a.MediaFileUrls),
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList();

            _quizRepo.Create(quiz);
            await _quizRepo.SaveChanges();
            return _mapper.Map<CreateResponseDto>(quiz);
        }
    }
}
