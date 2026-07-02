using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.Queries;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Quizzes.DTOs;

namespace Zlearn.V2.Application.Quizzes.Queries.GetUpdateQuizContent
{
    public class GetUpdateQuizContentQueryHandler : BaseQueryHandler<QuizDocument>, IRequestHandler<GetUpdateQuizContentQuery, UpdateQuizDto>
    {
        public GetUpdateQuizContentQueryHandler(
            IReadRepo<QuizDocument> readRepo,
            IMapper mapper,
            IMediator mediator) : base(readRepo, mapper, mediator)
        {
        }

        public async Task<UpdateQuizDto> Handle(GetUpdateQuizContentQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Id))
                throw new ArgumentException("Quiz ID must be provided.");

            var upperId = request.Id.ToUpper();

            if (!await _readRepo.AnyAsync(q => q.CreatedBy == request.OwnerId && q.Id == upperId))
                throw new UnauthorizedAccessException("You do not have permission to access this quiz.");

            var quizData = await _readRepo.GetByIdAsync(upperId)
                ?? throw new NotFoundException(nameof(QuizDocument), request.Id);

            quizData.Questions.Sort((a, b) => a.Order - b.Order);

            return _mapper.Map<UpdateQuizDto>(quizData);
        }
    }
}

