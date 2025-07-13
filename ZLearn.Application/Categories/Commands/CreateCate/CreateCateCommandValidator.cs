using ZLearn.Domain.Constants;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommandValidator : AbstractValidator<CreateCateCommand>
    {
        public CreateCateCommandValidator()
        {
            RuleFor(cc => cc.Name)
                .MaximumLength(StringLengths.CateNameMaxLength)
                    .WithMessage($"Category name cannot exceed {StringLengths.CateNameMaxLength} characters.")
                .MinimumLength(StringLengths.CateNameMinLength)
                    .WithMessage($"Category name must be at least {StringLengths.CateNameMinLength} characters long.")
                .NotEmpty()
                    .WithMessage("Category name is required.");
        }
    }
}
