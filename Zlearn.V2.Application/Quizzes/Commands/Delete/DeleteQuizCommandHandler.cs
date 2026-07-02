using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Commands;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Domain.CatalogContext.Quizzes;

namespace Zlearn.V2.Application.Quizzes.Commands.Delete
{
    public class DeleteQuizCommandHandler : BaseCommandHandler<Quiz>, IRequestHandler<DeleteQuizCommand, DeleteResponseDto>
    {
        private readonly IQuizWriteRepo _quizWriteRepo;
        private readonly IFileRepo _fileRepo;

        public DeleteQuizCommandHandler(
            IQuizWriteRepo quizWriteRepo,
            IFileRepo fileRepo,
            IMapper mapper,
            IMediator mediator) : base(quizWriteRepo, mapper, mediator)
        {
            _quizWriteRepo = quizWriteRepo;
            _fileRepo = fileRepo;
        }

        public async Task<DeleteResponseDto> Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
        {
            var quizzesToDelete = new List<Quiz>();
            foreach (var id in request.Ids)
            {
                // Check if user has permission (ownership check matching V1)
                if (!await _quizWriteRepo.AnyAsync(q => q.Id == id && q.CreatedBy == request.OwnerId))
                    continue;

                var quiz = await _quizWriteRepo.GetFullQuizContent(id);
                if (quiz is null) continue;

                // Check for ongoing exams
                if (await _quizWriteRepo.HasOngoingExamsAsync(id))
                {
                    throw new BadRequestException(
                        $"Không thể xóa đề '{quiz.Name}' vì còn bài kiểm tra đang diễn ra hoặc chưa bắt đầu. " +
                        "Vui lòng kết thúc tất cả các bài kiểm tra trước khi xóa.");
                }

                // Collect media files associated with the quiz
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
                var filesToDelete = fileUrlsToRemove.Where(url => !string.IsNullOrEmpty(url)).ToList();
                if (filesToDelete.Count > 0)
                {
                    await _fileRepo.DeleteFileByUrls(filesToDelete);
                }

                // Raise domain event from entity internal method
                quiz.Delete();

                _quizWriteRepo.Delete(quiz);
                quizzesToDelete.Add(quiz);
            }

            await _quizWriteRepo.SaveChangesAsync(cancellationToken);

            return new DeleteResponseDto
            {
                DeletedAt = DateTimeOffset.UtcNow,
                DeletedIds = quizzesToDelete.Select(q => q.Id).ToList()
            };
        }
    }
}

