using ZLearn.Domain.Constants;

namespace ZLearn.Application.Categories.Commands.CreateCate
{
    public class CreateCateCommandValidator : AbstractValidator<CreateCateCommand>
    {
        public CreateCateCommandValidator()
        {
            RuleFor(cc => cc.Name)
                .MaximumLength(StringLengths.CateNameMaxLength)
                .MinimumLength(StringLengths.CateNameMinLength)
                .NotEmpty();
        }
    }
}
