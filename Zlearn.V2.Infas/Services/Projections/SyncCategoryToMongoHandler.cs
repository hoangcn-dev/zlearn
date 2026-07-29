using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncCategoryToMongoHandler : 
        INotificationHandler<CategoryCreatedEvent>,
        INotificationHandler<CategoryUpdatedEvent>,
        INotificationHandler<CategoryDeletedEvent>
    {
        private readonly IMongoCollection<CategoryDocument> _collection;

        public SyncCategoryToMongoHandler(IMongoDatabase database)
        {
            _collection = database.GetCollection<CategoryDocument>("Categories");
        }

        public async Task Handle(CategoryCreatedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, notification.CategoryId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var document = new CategoryDocument
            {
                Id = notification.CategoryId,
                Name = notification.Name,
                Slug = notification.Slug,
                Description = notification.Description,
                ThumbnailUrl = notification.ThumbnailUrl,
                QuizCount = existingDoc?.QuizCount ?? 0,
                SyncedAt = notification.OccurredOn,
                CreatedAt = notification.CreatedAt,
                CreatedBy = notification.CreatedBy,
                LastModifiedAt = notification.CreatedAt,
                ModifiedBy = notification.CreatedBy
            };

            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }

        public async Task Handle(CategoryUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, notification.CategoryId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var document = new CategoryDocument
            {
                Id = notification.CategoryId,
                Name = notification.Name,
                Slug = notification.Slug,
                Description = notification.Description,
                ThumbnailUrl = notification.ThumbnailUrl,
                QuizCount = existingDoc?.QuizCount ?? 0,
                SyncedAt = notification.OccurredOn,
                CreatedAt = existingDoc?.CreatedAt ?? (notification.LastModifiedAt ?? DateTimeOffset.UtcNow),
                CreatedBy = existingDoc?.CreatedBy ?? notification.ModifiedBy ?? "unknown",
                LastModifiedAt = notification.LastModifiedAt,
                ModifiedBy = notification.ModifiedBy
            };

            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }

        public async Task Handle(CategoryDeletedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, notification.Id);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
