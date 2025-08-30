using ZLearn.Application.Common.Commands;
using ZLearn.Domain.Entities;
namespace ZLearn.Application.Exams.Commands.ChangeExamStatus
{
    public class ChangeExamStatusCommandHandler : BaseCommandHandler, IRequestHandler<ChangeExamStatusCommand>
    {
        private readonly IExamRepo _examRepo;

        public ChangeExamStatusCommandHandler(
            IMapper mapper,
            IMediator mediator,
            IExamRepo examRepo) : base(mapper, mediator)
        {
            _examRepo = examRepo;
        }
        public async Task Handle(ChangeExamStatusCommand request, CancellationToken cancellationToken)
        {
            var exam = await _examRepo.Get(request.ExamId)
                ?? throw new NotFoundException(nameof(Exam), request.ExamId);
            exam.Status = request.Status;
            _examRepo.Update(exam);
            await _examRepo.SaveChanges();
        }
    }
}
