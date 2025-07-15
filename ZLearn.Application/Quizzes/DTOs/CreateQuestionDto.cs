using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Quizzes.DTOs
{
    public class CreateQuestionDto
    {
        public string? StringContent { get; set; }
        public List<string> ImageIds { get; set; } = new();
        public List<string> AudioIds { get; set; } = new();
        public int CorrectKey { get; set; }
        public int Order { get; set; }
        public List<CreateAnswerDto> Answers { get; set; } = new();
    }

    public static class CreateQuestionDtoExtension
    {
        public static List<string> GetAllFilesInfo(this CreateQuestionDto q)
        {
            var res = new List<string>();
            res.AddRange(q.AudioIds);
            res.AddRange(q.ImageIds);
            q.Answers.ForEach(a => res.AddRange(a.ImageIds));
            return res;
        }

        public static void ReplaceAllFileNameToFileId(this CreateQuestionDto q, Dictionary<string, string> map)
        {
            for (int i = 0; i < q.ImageIds.Count; i++)
            {
                if (map.TryGetValue(q.ImageIds[i], out var id))
                {
                    q.ImageIds[i] = id;
                }
            }
            for (int i = 0; i < q.AudioIds.Count; i++)
            {
                if (map.TryGetValue(q.AudioIds[i], out var id))
                {
                    q.AudioIds[i] = id;
                }
            }
            foreach (var answer in q.Answers)
            {
                for (int i = 0; i < answer.ImageIds.Count; i++)
                {
                    if (map.TryGetValue(answer.ImageIds[i], out var id))
                    {
                        answer.ImageIds[i] = id;
                    }
                }
            }
        }
    }
}
