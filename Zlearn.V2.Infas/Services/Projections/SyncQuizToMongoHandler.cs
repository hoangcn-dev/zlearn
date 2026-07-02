using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Newtonsoft.Json;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;
using Zlearn.V2.Domain.CatalogContext.Quizzes.Events;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncQuizToMongoHandler : INotificationHandler<OutboxEvent>
    {
        private readonly IMongoCollection<QuizDocument> _collection;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly OutboxFallbackHelper _fallbackHelper;

        public SyncQuizToMongoHandler(IMongoDatabase database, AppDbContext dbContext, IMapper mapper, OutboxFallbackHelper fallbackHelper)
        {
            _collection = database.GetCollection<QuizDocument>("Quizzes");
            _mongoDatabase = database;
            _dbContext = dbContext;
            _mapper = mapper;
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

            bool isHandled = false;
            string? categoryIdToUpdate = null;

            if (domainEvent is QuizCreatedEvent createdEvent)
            {
                var document = new QuizDocument
                {
                    Id = createdEvent.QuizId,
                    Name = createdEvent.Name,
                    Slug = createdEvent.Slug,
                    CategoryId = createdEvent.CategoryId,
                    CategoryName = createdEvent.CategoryName,
                    CategorySlug = createdEvent.CategorySlug,
                    IsPublic = createdEvent.IsPublic,
                    QuestionCount = createdEvent.Questions.Count,
                    SyncedAt = DateTimeOffset.UtcNow,
                    CreatedBy = createdEvent.CreatedBy,
                    CreatedAt = createdEvent.CreatedAt,
                    Questions = createdEvent.Questions.Select(q => new QuestionDocumentItem
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
                    Tags = createdEvent.Tags
                };

                var filter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, document.Id);
                await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                
                categoryIdToUpdate = createdEvent.CategoryId;
                isHandled = true;
            }
            else if (domainEvent is QuizUpdatedEvent updatedEvent)
            {
                var quizFilter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, updatedEvent.QuizId);
                var oldDoc = await _collection.Find(quizFilter).FirstOrDefaultAsync(cancellationToken);
                var oldCategoryId = oldDoc?.CategoryId;

                var document = new QuizDocument
                {
                    Id = updatedEvent.QuizId,
                    Name = updatedEvent.Name,
                    Slug = updatedEvent.Slug,
                    CategoryId = updatedEvent.CategoryId,
                    CategoryName = updatedEvent.CategoryName,
                    CategorySlug = updatedEvent.CategorySlug,
                    IsPublic = updatedEvent.IsPublic,
                    QuestionCount = updatedEvent.Questions.Count,
                    SyncedAt = DateTimeOffset.UtcNow,
                    CreatedAt = oldDoc?.CreatedAt ?? (updatedEvent.LastModifiedAt ?? DateTimeOffset.UtcNow),
                    CreatedBy = oldDoc?.CreatedBy ?? "unknown",
                    LastModifiedAt = updatedEvent.LastModifiedAt,
                    ModifiedBy = updatedEvent.ModifiedBy,
                    Questions = updatedEvent.Questions.Select(q => new QuestionDocumentItem
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
                    Tags = updatedEvent.Tags
                };

                await _collection.ReplaceOneAsync(quizFilter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                
                categoryIdToUpdate = updatedEvent.CategoryId;
                if (oldCategoryId != null && oldCategoryId != updatedEvent.CategoryId)
                {
                    await UpdateCategoryQuizCount(oldCategoryId, cancellationToken);
                }
                isHandled = true;
            }
            else if (domainEvent is DeletedEvent deletedEvent)
            {
                if (deletedEvent.Id.StartsWith("QUI"))
                {
                    var quizFilter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, deletedEvent.Id);
                    var oldDoc = await _collection.Find(quizFilter).FirstOrDefaultAsync(cancellationToken);
                    
                    await _collection.DeleteOneAsync(quizFilter, cancellationToken);
                    
                    if (oldDoc?.CategoryId != null)
                    {
                        categoryIdToUpdate = oldDoc.CategoryId;
                    }
                    isHandled = true;
                }
                else if (deletedEvent.Id.StartsWith("QUE"))
                {
                    var filter = Builders<QuizDocument>.Filter.Eq("Questions.Id", deletedEvent.Id);
                    var update = Builders<QuizDocument>.Update.PullFilter("Questions", Builders<QuestionDocumentItem>.Filter.Eq(q => q.Id, deletedEvent.Id));
                    await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
                    isHandled = true;
                }
                else if (deletedEvent.Id.StartsWith("ANS"))
                {
                    var filter = Builders<QuizDocument>.Filter.Eq("Questions.Answers.Id", deletedEvent.Id);
                    var update = Builders<QuizDocument>.Update.PullFilter("Questions.$[].Answers", Builders<AnswerDocumentItem>.Filter.Eq(a => a.Id, deletedEvent.Id));
                    await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
                    isHandled = true;
                }
                else if (deletedEvent.Id.StartsWith("TAG"))
                {
                    if (domainEvent is Zlearn.V2.Domain.CatalogContext.Tags.Events.TagDeletedEvent tagDeletedEvent)
                    {
                        var filter = Builders<QuizDocument>.Filter.AnyEq(q => q.Tags, tagDeletedEvent.Name);
                        var update = Builders<QuizDocument>.Update.Pull(q => q.Tags, tagDeletedEvent.Name);
                        await _collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
                    }
                    isHandled = true;
                }
            }

            if (categoryIdToUpdate != null)
            {
                await UpdateCategoryQuizCount(categoryIdToUpdate, cancellationToken);
            }

            // Mark Outbox Event as processed
            if (isHandled)
            {
                var outboxEvent = await _dbContext.OutboxEvents.FindAsync(new object[] { notification.Id }, cancellationToken);
                if (outboxEvent != null)
                {
                    outboxEvent.ProcessedOn = DateTimeOffset.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }
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
