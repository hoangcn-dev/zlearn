
namespace Zlearn.V2.Domain.Common
{
    public record DeletedEvent(string Id) : DomainEvent
    {
        public override string AggregateId => Id;
    }
}
