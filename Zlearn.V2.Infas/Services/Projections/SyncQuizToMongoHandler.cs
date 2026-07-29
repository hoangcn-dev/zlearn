using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;
using Zlearn.V2.Domain.CatalogContext.Answers.Events;
using Zlearn.V2.Domain.CatalogContext.Questions.Events;
using Zlearn.V2.Domain.CatalogContext.Quizzes.Events;
using Zlearn.V2.Domain.CatalogContext.Tags.Events;
using Zlearn.V2.Infas.Data;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncQuizToMongoHandler : 
        INotificationHandler<QuizCreatedEvent>,
        INotificationHandler<QuizUpdatedEvent>,
        INotificationHandler<QuizDeletedEvent>,
        INotificationHandler<QuestionDeletedEvent>,
        INotificationHandler<AnswerDeletedEvent>,
        INotificationHandler<TagDeletedEvent>
    {
        private readonly IMongoCollection<QuizDocument> _collection;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly AppDbContext _dbContext;

        public SyncQuizToMongoHandler(IMongoDatabase database, AppDbContext dbContext)
        {
            _collection = database.GetCollection<QuizDocument>("Quizzes");
            _mongoDatabase = database;
            _dbContext = dbContext;
        }

        public async Task Handle(QuizCreatedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, notification.QuizId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var document = new QuizDocument
            {
                Id = notification.QuizId,
                Name = notification.Name,
                Slug = notification.Slug,
                CategoryId = notification.CategoryId,
                CategoryName = notification.CategoryName,
                CategorySlug = notification.CategorySlug,
                IsPublic = notification.IsPublic,
                QuestionCount = notification.Questions.Count,
                SyncedAt = notification.OccurredOn,
                CreatedBy = notification.CreatedBy,
                CreatedAt = notification.CreatedAt,
                Questions = notification.Questions.Select(q => new QuestionDocumentItem
                {
                    Id = q.Id,
                    Slug = q.Slug,
                    StringContent = q.StringContent,
                    MediaFileUrls = q.MediaFileUrls,
                    Explanation = q.Explanation,
                    Order = q.Order,
                    Answers = q.Answers.Select(a => new AnswerDocumentItem
                    {
                        Id = a.Id,
                        Key = a.Key,
                        StringContent = a.StringContent,
                        MediaFileUrls = a.MediaFileUrls,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList(),
                Tags = notification.Tags
            };

            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);

            await UpdateCategoryQuizCount(notification.CategoryId, cancellationToken);
        }

        public async Task Handle(QuizUpdatedEvent notification, CancellationToken cancellationToken)
        {
            var quizFilter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, notification.QuizId);
            var oldDoc = await _collection.Find(quizFilter).FirstOrDefaultAsync(cancellationToken);
            if (oldDoc != null && oldDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }
            var oldCategoryId = oldDoc?.CategoryId;

            var document = new QuizDocument
            {
                Id = notification.QuizId,
                Name = notification.Name,
                Slug = notification.Slug,
                CategoryId = notification.CategoryId,
                CategoryName = notification.CategoryName,
                CategorySlug = notification.CategorySlug,
                IsPublic = notification.IsPublic,
                QuestionCount = notification.Questions.Count,
                SyncedAt = notification.OccurredOn,
                CreatedAt = oldDoc?.CreatedAt ?? (notification.LastModifiedAt ?? DateTimeOffset.UtcNow),
                CreatedBy = oldDoc?.CreatedBy ?? "unknown",
                LastModifiedAt = notification.LastModifiedAt,
                ModifiedBy = notification.ModifiedBy,
                Questions = notification.Questions.Select(q => new QuestionDocumentItem
                {
                    Id = q.Id,
                    Slug = q.Slug,
                    StringContent = q.StringContent,
                    MediaFileUrls = q.MediaFileUrls,
                    Explanation = q.Explanation,
                    Order = q.Order,
                    Answers = q.Answers.Select(a => new AnswerDocumentItem
                    {
                        Id = a.Id,
                        Key = a.Key,
                        StringContent = a.StringContent,
                        MediaFileUrls = a.MediaFileUrls,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                }).ToList(),
                Tags = notification.Tags
            };

            await _collection.ReplaceOneAsync(quizFilter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);

            await UpdateCategoryQuizCount(notification.CategoryId, cancellationToken);
            if (oldCategoryId != null && oldCategoryId != notification.CategoryId)
            {
                await UpdateCategoryQuizCount(oldCategoryId, cancellationToken);
            }
        }

        public async Task Handle(QuizDeletedEvent notification, CancellationToken cancellationToken)
        {
            var quizFilter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, notification.Id);
            var oldDoc = await _collection.Find(quizFilter).FirstOrDefaultAsync(cancellationToken);

            await _collection.DeleteOneAsync(quizFilter, cancellationToken);

            if (oldDoc?.CategoryId != null)
            {
                await UpdateCategoryQuizCount(oldDoc.CategoryId, cancellationToken);
            }
        }

        public async Task Handle(QuestionDeletedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<QuizDocument>.Filter.Eq("Questions.Id", notification.Id);
            var update = Builders<QuizDocument>.Update.PullFilter("Questions", Builders<QuestionDocumentItem>.Filter.Eq(q => q.Id, notification.Id));
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task Handle(AnswerDeletedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<QuizDocument>.Filter.Eq("Questions.Answers.Id", notification.Id);
            var update = Builders<QuizDocument>.Update.PullFilter("Questions.$[].Answers", Builders<AnswerDocumentItem>.Filter.Eq(a => a.Id, notification.Id));
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task Handle(TagDeletedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<QuizDocument>.Filter.AnyEq(q => q.Tags, notification.Name);
            var update = Builders<QuizDocument>.Update.Pull(q => q.Tags, notification.Name);
            await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
        }

        private async Task UpdateCategoryQuizCount(string categoryId, CancellationToken cancellationToken)
        {
            var count = await _dbContext.Quizzes.CountAsync(q => q.CategoryId == categoryId, cancellationToken);
            var categoryFilter = Builders<CategoryDocument>.Filter.Eq(doc => doc.Id, categoryId);
            var update = Builders<CategoryDocument>.Update.Set(doc => doc.QuizCount, (int)count);
            await _mongoDatabase.GetCollection<CategoryDocument>("Categories")
                .UpdateOneAsync(categoryFilter, update, cancellationToken: cancellationToken);
        }
    }
}
