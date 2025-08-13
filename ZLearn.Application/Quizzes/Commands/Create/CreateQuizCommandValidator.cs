using ZLearn.Domain.Constants;

namespace ZLearn.Application.Quizzes.Commands.Create
{
    public class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
    {
        public CreateQuizCommandValidator()
        {
            RuleFor(x => x.Data.Name)
                .NotEmpty().WithMessage("Tên đề trắc nghiệm trống.")
                .MaximumLength(StringLengths.QuizNameMaxLength).WithMessage($"Tên đề có độ dài không vượt quá {StringLengths.QuizNameMaxLength} kí tự.")
                .MinimumLength(StringLengths.QuizNameMinLength).WithMessage($"Tên đề có độ dài lớn hơn {StringLengths.QuizNameMinLength} kí tự");

            RuleFor(x => x.Data.CategoryId)
                .NotEmpty().WithMessage("Danh mục trống.");

            RuleFor(x => x.Data.Tags)
                .NotEmpty().WithMessage("Vui lòng thêm ít nhất 1 tag.")
                .Must(tags => tags.All(tag => tag.Length >= StringLengths.TagNameMinLength && tag.Length <= StringLengths.TagNameMaxLength))
                .WithMessage($"Độ dài tag phải nằm trong khoảng {StringLengths.TagNameMinLength} - {StringLengths.TagNameMaxLength} kí tự.");

            RuleFor(x => x.Data.Questions)
                .NotEmpty().WithMessage("Chưa thêm câu hỏi nào.")
                .Must(questions => questions.All(q => q.Answers.Count >= 2)).WithMessage("Mỗi câu hỏi trắc nghiệm phải có ít nhất 2 lựa chọn.")
                .Must(questions => questions.All(q => q.Answers.All(a => !string.IsNullOrEmpty(q.StringContent) || q.MediaFileUrls.Count > 0))).WithMessage("Mỗi đáp án phải có nội dung văn bản hoặc ảnh.")
                .Must(questions => questions.All(q => !string.IsNullOrEmpty(q.StringContent) || q.MediaFileUrls.Count > 0)).WithMessage($"Mỗi câu hỏi trắc nghiệm phải có nội dung văn bản hoặc media.");
        }
    }
}
