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
            // Check if quiz exists and update it
            var quiz = await _quizRepo.GetFullQuizContent(request.Data.Id)
                ?? throw new NotFoundException(nameof(Quiz), request.Data.Id);

            // Check category existence and quiz uniqueness
            var data = request.Data;
            if (!await _cateRepo.Any(c => c.Id == data.CategoryId))
                throw new ArgumentException($"Category with ID {data.CategoryId} does not exist.");
            if (quiz.Name != data.Name && await _quizRepo.Any(q => q.Name == data.Name && q.CategoryId == data.CategoryId))
                throw new ArgumentException($"Quiz with name '{data.Name}' already exists in category with ID {data.CategoryId}.");
            
            // Check question keys and media links
            var fileIds = new List<string>();
            foreach (var question in data.Questions)
            {
                if (!question.Answers.Any(a => a.Key == question.CorrectKey))
                    throw new ArgumentException($"Question {question.Order} does not have a valid correct answer key.");
                // Add question media file IDs
                if (question.MediaFileIds.Count > 0)
                    fileIds.AddRange(question.MediaFileIds);
                question.Answers.ForEach(a =>
                {
                    if (a.MediaFileIds.Count > 0)
                        fileIds.AddRange(a.MediaFileIds);
                });
            }
            var distinctFileIds = fileIds.ToHashSet();
            if (distinctFileIds.Count != fileIds.Count)
                throw new ResourceConflictException("Each file must be used one time");
            await _fileRepo.CheckExistingByFileIds(distinctFileIds);

            // Remove file not exist in data
            var fileIdsToRemove = new List<string>();
            foreach (var q in quiz.Questions)
            {
                if (q.MediaFileIds is not null)
                    fileIdsToRemove.AddRange(q.MediaFileIds.Split(","));
                foreach (var a in q.Answers)
                {
                    if (a.MediaFileIds is not null)
                        fileIdsToRemove.AddRange(a.MediaFileIds.Split(","));
                }
            }
            await _fileRepo.DeleteFileByIds(fileIdsToRemove.Where(id => !distinctFileIds.Contains(id)));

            // Update quiz properties
            quiz.Name = data.Name;
            quiz.CategoryId = data.CategoryId;
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
                        MediaFileIds = string.Join(",", q.MediaFileIds),
                        Answers = q.Answers.Select(a => new Answer
                        {
                            Id = IdGenerator.Generate("ANS"),
                            Key = a.Key,
                            StringContent = a.StringContent,
                            MediaFileIds = string.Join(",", a.MediaFileIds),
                        }).ToList()
                    });
                }
                else if (questions.TryGetValue(q.Id, out var existingQuestion))
                {
                    // Update existing question
                    existingQuestion.Order = q.Order;
                    existingQuestion.StringContent = q.StringContent;
                    existingQuestion.MediaFileIds = string.Join(",", q.MediaFileIds);
                    existingQuestion.CorrectKey = q.CorrectKey;
                    // Update answers
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
                                MediaFileIds = string.Join(",", a.MediaFileIds)
                            });
                        }
                        else if (answers.TryGetValue(a.Id, out var existingAnswer))
                        {
                            // Update existing answer
                            existingAnswer.Key = a.Key;
                            existingAnswer.StringContent = a.StringContent;
                            existingAnswer.MediaFileIds = string.Join(",", a.MediaFileIds);
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
