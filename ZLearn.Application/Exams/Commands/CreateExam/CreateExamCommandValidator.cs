using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Exams.Commands.CreateExam
{
    public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
    {
        public CreateExamCommandValidator()
        {
            RuleFor(c => c.Data).NotNull().WithMessage($"Dữ liệu trống.");
            RuleFor(c => c.Data.Name)
                .NotEmpty().WithMessage($"Tên bài kiểm tra trống.")
                .MaximumLength(ExamRules.NAME_MAX_LENGTH).WithMessage($"Tên bài kiểm tra không vượt quá {ExamRules.NAME_MAX_LENGTH} kí tự");
            RuleFor(c => c.Data.JoinPass)
                .MaximumLength(ExamRules.JOINPASS_MAX_LENGTH).WithMessage($"Độ dài mã truy cập không vượt quá {ExamRules.JOINPASS_MAX_LENGTH} kí tự");
            RuleFor(c => c.Data.MaxParticipants)
                .GreaterThanOrEqualTo(1).WithMessage($"Số người tham gia tối thiểu là 1.");
        }
    }
}
