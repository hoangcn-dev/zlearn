namespace Zlearn.V2.Application.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
        public string? StringContent { get; set; }
        public List<string> MediaFileUrls { get; set; } = new();
        public string? Explanation { get; set; }
        public int Order { get; set; }
        public List<CreateAnswerDto> Answers { get; set; } = new();
    }

    public static class CreateQuestionDtoExtension
    {
        public static List<string> GetAllFilesInfo(this CreateQuestionDto q)
        {
            var res = new List<string>();
            res.AddRange(q.MediaFileUrls);
            q.Answers.ForEach(a => res.AddRange(a.MediaFileUrls));
            return res;
        }

        public static void ReplaceAllFileNameToFileUrl(this CreateQuestionDto q, Dictionary<string, string> map)
        {
            for (int i = 0; i < q.MediaFileUrls.Count; i++)
            {
                if (map.TryGetValue(q.MediaFileUrls[i], out var id))
                {
                    q.MediaFileUrls[i] = id;
                }
            }
            foreach (var answer in q.Answers)
            {
                for (int i = 0; i < answer.MediaFileUrls.Count; i++)
                {
                    if (map.TryGetValue(answer.MediaFileUrls[i], out var id))
                    {
                        answer.MediaFileUrls[i] = id;
                    }
                }
            }
        }
    }
}
