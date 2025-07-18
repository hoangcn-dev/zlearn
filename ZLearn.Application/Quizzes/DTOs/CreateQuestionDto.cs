namespace ZLearn.Application.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
        public string? StringContent { get; set; }
        public List<string> MediaFileIds { get; set; } = new();
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<CreateAnswerDto> Answers { get; set; } = new();
    }

    public static class CreateQuestionDtoExtension
    {
        public static List<string> GetAllFilesInfo(this CreateQuestionDto q)
        {
            var res = new List<string>();
            res.AddRange(q.MediaFileIds);
            q.Answers.ForEach(a => res.AddRange(a.MediaFileIds));
            return res;
        }

        public static void ReplaceAllFileNameToFileId(this CreateQuestionDto q, Dictionary<string, string> map)
        {
            for (int i = 0; i < q.MediaFileIds.Count; i++)
            {
                if (map.TryGetValue(q.MediaFileIds[i], out var id))
                {
                    q.MediaFileIds[i] = id;
                }
            }
            foreach (var answer in q.Answers)
            {
                for (int i = 0; i < answer.MediaFileIds.Count; i++)
                {
                    if (map.TryGetValue(answer.MediaFileIds[i], out var id))
                    {
                        answer.MediaFileIds[i] = id;
                    }
                }
            }
        }
    }
}
