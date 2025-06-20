using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZLearn.Domain.Common
{
    public class BaseEntity
    {
        private readonly List<BaseEvent> _events = new();

        [MaxLength(16)]
        public string Id { get; set; }

        [NotMapped]
        public IReadOnlyCollection<BaseEvent> Events => _events.AsReadOnly();
        public void AddEvent(BaseEvent e) => _events.Add(e);
        public void RemoveEvent(BaseEvent e) =>_events.Remove(e);
        public void ClearEvents() => _events.Clear();
    }
}
