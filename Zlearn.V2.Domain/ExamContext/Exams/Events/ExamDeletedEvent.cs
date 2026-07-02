using System;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Domain.ExamContext.Exams.Events
{
    public record ExamDeletedEvent : DeletedEvent
    {
        public ExamDeletedEvent(string Id) : base(Id)
        {
        }
    }
}
