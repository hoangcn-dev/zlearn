using System;

namespace ZLearn.Application.Exams.DTOs
{
    public class GradedResultCacheDto
    {
        public double Score { get; set; }
        public int Correct { get; set; }
        public int Completed { get; set; }
        public string? SelectedAnswers { get; set; }
        public DateTimeOffset FirstCheckIn { get; set; }
        public DateTimeOffset LastCheckOut { get; set; }
    }
}
