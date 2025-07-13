using ZLearn.Domain.Constants;

namespace ZLearn.Application.Quizzes.Commands.Update
{
    public class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
    {
        public UpdateQuizCommandValidator()
        {
            RuleFor(x => x.Data.Name)
                .NotEmpty().WithMessage("Quiz name is required.")
                .MaximumLength(StringLengths.QuizNameMaxLength).WithMessage($"Quiz name cannot exceed {StringLengths.QuizNameMaxLength} characters.")
                .MinimumLength(StringLengths.QuizNameMinLength).WithMessage($"Quiz name cannot less {StringLengths.QuizNameMinLength} characters");

            RuleFor(x => x.Data.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");

            RuleFor(x => x.Data.Tags)
                .NotEmpty().WithMessage("At least one tag is required.")
                .Must(tags => tags.All(tag => tag.Length >= StringLengths.TagNameMinLength && tag.Length <= StringLengths.TagNameMaxLength))
                .WithMessage($"Each tag must be between {StringLengths.TagNameMinLength} and {StringLengths.TagNameMaxLength} characters long.");

            RuleFor(x => x.Data.Questions)
                .NotEmpty().WithMessage("At least one question is required.")
                .Must(questions => questions.All(q => q.Answers.Count >= 2)).WithMessage("Each question must have at least two answers.")
                .Must(questions => questions.All(q => q.Answers.All(a => !string.IsNullOrEmpty(q.StringContent) || q.ImageIds.Count > 0))).WithMessage("Each question answer must have string content or image.")
                .Must(questions => questions.All(q => !string.IsNullOrEmpty(q.StringContent) || q.ImageIds.Count > 0 || q.AudioIds.Count > 0)).WithMessage($"Each question must have string content or image/audio.");
        }
    }
}
