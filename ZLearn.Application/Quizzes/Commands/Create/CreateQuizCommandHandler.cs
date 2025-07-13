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
                throw new ArgumentException($"Category with ID {data.CategoryId} does not exist.");
            if (await _quizRepo.Any(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Quiz with name '{data.Name}' already exists in category with ID {data.CategoryId}.");
            // Check question keys and media links
            var fileIds = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.Key == question.CorrectKey))
                    throw new ArgumentException($"Question {question.Order} does not have a valid correct answer key.");
                // Add question media file IDs
                if (question.ImageIds.Count > 0)
                    fileIds.AddRange(question.ImageIds);
                if (question.AudioIds.Count > 0)
                    fileIds.AddRange(question.AudioIds);
                question.Answers.ForEach(a =>
                {
                    if (a.ImageIds.Count > 0)
                        fileIds.AddRange(a.ImageIds);
                });
            }

            var distinctFileIds = fileIds.ToHashSet();
            if (distinctFileIds.Count != fileIds.Count)
                throw new ResourceConflictException("Each file must be used one time");
            await _fileRepo.CheckExistingByFileIds(distinctFileIds);

            // Add new question
            var quiz = new Quiz
            {
                Id = IdGenerator.Generate("QUI"),
                Name = data.Name,
                CategoryId = data.CategoryId,
            };
            await _quizRepo.SetQuestionsTagAsync(quiz, data.Tags);
            quiz.Questions = data.Questions.Select(q => new Question
            {
                Id = IdGenerator.Generate("QUE"),
                Order = q.Order,
                StringContent = q.StringContent,
                ImageIds = string.Join(",", q.ImageIds),
                AudioIds = string.Join(",", q.AudioIds),
                CorrectKey = q.CorrectKey,
                Answers = q.Answers.Select(a => new Answer
                {
                    Id = IdGenerator.Generate("ANS"),
                    Key = a.Key,
                    StringContent = a.StringContent,
                    ImageIds = string.Join(",", a.ImageIds),
                }).ToList()
            }).ToList();

            _quizRepo.Create(quiz);
            await _quizRepo.SaveChanges();
            return _mapper.Map<CreateResponseDto>(quiz);
        }
    }
}
