using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Domain.Events.CategoryV2;
using ZLearn.Infras.Data.Outbox;

namespace ZLearn.Infras.Services.Projections
{
    public class SyncCategoryToMongoHandler : INotificationHandler<DomainEventNotificationWrapper<CategoryCreatedEvent>>
    {
        private readonly IMongoCollection<CategoryDocument> _collection;

        public SyncCategoryToMongoHandler(IMongoDatabase database)
        {
            _collection = database.GetCollection<CategoryDocument>("Categories");
        }

        public async Task Handle(DomainEventNotificationWrapper<CategoryCreatedEvent> notification, CancellationToken cancellationToken)
        {
            var domainEvent = notification.DomainEvent;
            var document = new CategoryDocument
            {
                Id = domainEvent.CategoryId,
                Name = domainEvent.Name,
                Slug = domainEvent.Slug,
                SyncedAt = DateTimeOffset.UtcNow
            };

            var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, document.Id);

            // Ghi đè hoặc chèn mới (Upsert) vào MongoDB
            await _collection.ReplaceOneAsync(
                filter,
                document,
                new ReplaceOptions { IsUpsert = true },
                cancellationToken
            );
        }
    }
}
