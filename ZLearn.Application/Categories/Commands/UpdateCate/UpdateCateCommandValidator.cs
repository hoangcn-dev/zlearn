using ZLearn.Domain.Constants;

namespace ZLearn.Application.Categories.Commands.UpdateCate
{
    public class UpdateCateCommandValidator : AbstractValidator<UpdateCateCommand>
    {
        public UpdateCateCommandValidator()
        {
            RuleFor(uc => uc.Data.Name)
                .MaximumLength(StringLengths.CateNameMaxLength).WithMessage($"Length must not be greater than {StringLengths.CateNameMaxLength}")
                .MinimumLength(StringLengths.CateNameMinLength).WithMessage($"Length must be greater than {StringLengths.CateNameMinLength}")
                .NotEmpty();
        }
    }
}
