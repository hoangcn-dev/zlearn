using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using Newtonsoft.Json;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncCategoryToMongoHandler : INotificationHandler<OutboxEvent>
    {
        private readonly IMongoCollection<CategoryDocument> _collection;
        private readonly OutboxFallbackHelper _fallbackHelper;

        public SyncCategoryToMongoHandler(IMongoDatabase database, OutboxFallbackHelper fallbackHelper)
        {
            _collection = database.GetCollection<CategoryDocument>("Categories");
            _fallbackHelper = fallbackHelper;
        }

        public async Task Handle(OutboxEvent notification, CancellationToken cancellationToken)
        {
            // Fallback: Xử lý các sự kiện bị miss trước đó của AggregateId này
            await _fallbackHelper.ProcessMissedEventsBeforeAsync(notification, cancellationToken);

            var type = Type.GetType(notification.Type);
            if (type == null) return;

            var domainEvent = JsonConvert.DeserializeObject(notification.Content, type);
            if (domainEvent == null) return;

            if (domainEvent is CategoryCreatedEvent createdEvent)
            {
                var document = new CategoryDocument
                {
                    Id = createdEvent.CategoryId,
                    Name = createdEvent.Name,
                    Slug = createdEvent.Slug,
                    Description = createdEvent.Description,
                    ThumbnailUrl = createdEvent.ThumbnailUrl,
                    QuizCount = 0,
                    SyncedAt = DateTimeOffset.UtcNow,
                    CreatedAt = createdEvent.CreatedAt,
                    CreatedBy = createdEvent.CreatedBy,
                    LastModifiedAt = createdEvent.CreatedAt,
                    ModifiedBy = createdEvent.CreatedBy
                };

                var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, document.Id);
                await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            }
            else if (domainEvent is CategoryUpdatedEvent updatedEvent)
            {
                var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, updatedEvent.CategoryId);
                var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

                var document = new CategoryDocument
                {
                    Id = updatedEvent.CategoryId,
                    Name = updatedEvent.Name,
                    Slug = updatedEvent.Slug,
                    Description = updatedEvent.Description,
                    ThumbnailUrl = updatedEvent.ThumbnailUrl,
                    QuizCount = existingDoc?.QuizCount ?? 0,
                    SyncedAt = DateTimeOffset.UtcNow,
                    CreatedAt = existingDoc?.CreatedAt ?? (updatedEvent.LastModifiedAt ?? DateTimeOffset.UtcNow),
                    CreatedBy = existingDoc?.CreatedBy ?? updatedEvent.ModifiedBy ?? "unknown",
                    LastModifiedAt = updatedEvent.LastModifiedAt,
                    ModifiedBy = updatedEvent.ModifiedBy
                };

                await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            }
            else if (domainEvent is DeletedEvent deletedEvent && deletedEvent.Id.StartsWith("CAT"))
            {
                var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, deletedEvent.Id);
                await _collection.DeleteOneAsync(filter, cancellationToken);
            }
        }
    }
}
