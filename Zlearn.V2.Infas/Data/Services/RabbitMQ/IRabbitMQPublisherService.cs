using System.Threading.Tasks;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Messaging
{
    public interface IRabbitMQPublisherService
    {
        Task PublishEventAsync(OutboxEvent outboxEvent);
    }
}
