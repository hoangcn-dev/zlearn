using System.Collections.Generic;

namespace Zlearn.V2.Domain.CatalogContext.Quizzes
{
    public record AnswerPayload(
        string Id,
        int Key,
        string? StringContent,
        string MediaFileUrls,
        bool IsCorrect
    );

    public record QuestionPayload(
        string Id,
        int Order,
        string Slug,
        string? StringContent,
        string MediaFileUrls,
        string? Explanation,
        List<AnswerPayload> Answers
    );
}
