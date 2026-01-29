using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Commands;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Files;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Application.Quizzes.Commands.Delete
{
    public class DeleteQuizCommandHandler : BaseCommandHandler, IRequestHandler<DeleteQuizCommand, DeleteResponseDto>
    {
        private readonly IQuizRepo _quizRepo;
        private readonly IFileRepo _fileRepo;

        public DeleteQuizCommandHandler(IMapper mapper, IMediator mediator, IQuizRepo quizRepo, IFileRepo fileRepo) : base(mapper, mediator)
        {
            _quizRepo = quizRepo;
            _fileRepo = fileRepo;
        }

        public async Task<DeleteResponseDto> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
        {
            var QuizzesToDelete = new List<Quiz>();
            foreach (var id in request.Ids)
            {
                if (!await _quizRepo.Any(q => q.CreatedBy == request.OwnerId)) continue;
                var quiz = await _quizRepo.GetFullQuizContent(id);
                if (quiz is null) continue;

                // Check if quiz has any ongoing exams (WaitStart or InProgress)
                var hasOngoingExams = quiz.Exams.Any(e => 
                    e.Status == ExamStatus.WaitStart || 
                    e.Status == ExamStatus.InProgress);
                
                if (hasOngoingExams)
                {
                    throw new BadRequestException(
                        $"Không thể xóa đề '{quiz.Name}' vì còn bài kiểm tra đang diễn ra hoặc chưa bắt đầu. " +
                        "Vui lòng kết thúc tất cả các bài kiểm tra trước khi xóa.");
                }

                // Remove media files associated with the quiz
                var fileIdsToRemove = new List<string>();
                foreach (var q in quiz.Questions)
                {
                    if (q.MediaFileUrls is not null)
                        fileIdsToRemove.AddRange(q.MediaFileUrls.Split(","));
                    foreach (var a in q.Answers)
                    {
                        if (a.MediaFileUrls is not null)
                            fileIdsToRemove.AddRange(a.MediaFileUrls.Split(","));
                    }
                }
                await _fileRepo.DeleteFileByUrls(fileIdsToRemove);
                QuizzesToDelete.Add(quiz);
            };

            _quizRepo.Delete(QuizzesToDelete);
            await _quizRepo.SaveChanges();

            return new DeleteResponseDto
            {
                DeletedAt = DateTimeOffset.UtcNow,
                DeletedIds = QuizzesToDelete.Select(q => q.Id).ToList(),
            };
        }
    }
}
