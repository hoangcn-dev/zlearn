using ZLearn.Application.Categories;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.Commands.Update
{
    public class UpdateQuizCommandHandler : BaseCommandHandler, IRequestHandler<UpdateQuizCommand, UpdateResponseDto>
    {
        private readonly IQuizRepo _quizRepo;
        private readonly IFileRepo _fileRepo;
        private readonly ICateRepo _cateRepo;

        public UpdateQuizCommandHandler(
            IMapper mapper, IMediator mediator,
            IQuizRepo quizRepo,
            IFileRepo fileRepo,
            ICateRepo cateRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
            _fileRepo = fileRepo;
            _cateRepo = cateRepo;
        }

        public async Task<UpdateResponseDto> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
        {
            if (!await _quizRepo.Any(q => q.CreatedBy == request.OwnerId))
                throw new UnauthorizedAccessException("You do not have permission to access this quiz.");

            // Check if quiz exists
            var quiz = await _quizRepo.GetFullQuizContent(request.Data.Id)
                ?? throw new NotFoundException(nameof(Quiz), request.Data.Id);

            // Check category existence and quiz uniqueness
            var data = request.Data;
            if (!await _cateRepo.Any(c => c.Id == data.CategoryId))
                throw new ArgumentException($"Category with ID {data.CategoryId} does not exist.");
            if (quiz.Name != data.Name && await _quizRepo.Any(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Quiz with name '{data.Name}' already exists in category with ID {data.CategoryId}.");
            if (quiz.Slug != data.Slug && await _quizRepo.Any(q => q.Slug == data.Slug))
                throw new ArgumentException($"Quiz with slug '{data.Slug}' already exists");

            // Check question keys and media links
            var fileUrls = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.Key == question.CorrectKey))
                    throw new ArgumentException($"Question {question.Order} does not have a valid correct answer key.");
                // Add question media file IDs
                if (question.MediaFileUrls.Count > 0)
                    fileUrls.AddRange(question.MediaFileUrls);
                question.Answers.ForEach(a =>
                {
                    if (a.MediaFileUrls.Count > 0)
                        fileUrls.AddRange(a.MediaFileUrls);
                });
            }
            var distinctFileUrls = fileUrls.ToHashSet();
            if (distinctFileUrls.Count != fileUrls.Count)
                throw new ResourceConflictException("Each file must be used one time");
            await _fileRepo.CheckExistingByFileUrls(distinctFileUrls);

            // Remove file not exist in data
            var fileUrlsToRemove = new List<string>();
            foreach (var q in quiz.Questions)
            {
                if (q.MediaFileUrls is not null)
                    fileUrlsToRemove.AddRange(q.MediaFileUrls.Split(","));
                foreach (var a in q.Answers)
                {
                    if (a.MediaFileUrls is not null)
                        fileUrlsToRemove.AddRange(a.MediaFileUrls.Split(","));
                }
            }
            await _fileRepo.DeleteFileByUrls(fileUrlsToRemove.Where(url => !distinctFileUrls.Contains(url)).ToList());

            // Update quiz properties
            quiz.Name = data.Name;
            quiz.CategoryId = data.CategoryId;
            quiz.Slug = data.Slug ?? StringHelper.GenerateSlug(data.Name);
            await _quizRepo.SetQuestionsTagAsync(quiz, data.Tags);

            // Update questions
            var questions = quiz.Questions.ToDictionary(q => q.Id, q => q);
            var answers = quiz.Questions.SelectMany(q => q.Answers).ToDictionary(a => a.Id, a => a);
            quiz.Questions.Clear();
            foreach (var q in data.Questions)
            {
                if (q.Id is null)
                {
                    // Add new question
                    quiz.Questions.Add(new Question
                    {
                        Id = IdGenerator.Generate("QUE"),
                        Order = q.Order,
                        StringContent = q.StringContent,
                        Slug = q.Slug,
                        MediaFileUrls = string.Join(",", q.MediaFileUrls),
                        Answers = q.Answers.Select(a => new Answer
                        {
                            Id = IdGenerator.Generate("ANS"),
                            Key = a.Key,
                            StringContent = a.StringContent,
                            MediaFileUrls = string.Join(",", a.MediaFileUrls),
                        }).ToList()
                    });
                }
                else if (questions.TryGetValue(q.Id, out var existingQuestion))
                {
                    // Update existing question
                    existingQuestion.Order = q.Order;
                    existingQuestion.StringContent = q.StringContent;
                    existingQuestion.Slug = q.Slug;
                    existingQuestion.MediaFileUrls = string.Join(",", q.MediaFileUrls);
                    existingQuestion.CorrectKey = q.CorrectKey;
                    existingQuestion.Answers.Clear();
                    foreach (var a in q.Answers)
                    {
                        if (a.Id is null)
                        {
                            // Add new answer
                            existingQuestion.Answers.Add(new Answer
                            {
                                Id = IdGenerator.Generate("ANS"),
                                Key = a.Key,
                                StringContent = a.StringContent,
                                MediaFileUrls = string.Join(",", a.MediaFileUrls)
                            });
                        }
                        else if (answers.TryGetValue(a.Id, out var existingAnswer))
                        {
                            // Update existing answer
                            existingAnswer.Key = a.Key;
                            existingAnswer.StringContent = a.StringContent;
                            existingAnswer.MediaFileUrls = string.Join(",", a.MediaFileUrls);
                            existingQuestion.Answers.Add(existingAnswer);
                        }
                    }
                    quiz.Questions.Add(existingQuestion);
                }
            }

            _quizRepo.Update(quiz);
            await _quizRepo.SaveChanges();
            return _mapper.Map<UpdateResponseDto>(quiz);
        }
    }
}
