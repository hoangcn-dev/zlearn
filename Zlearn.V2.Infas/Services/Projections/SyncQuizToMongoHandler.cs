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

        public SyncQuizToMongoHandler(IMongoDatabase database, AppDbContext dbContext, IMapper mapper)
        {
            _collection = database.GetCollection<QuizDocument>("Quizzes");
            _mongoDatabase = database;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task Handle(OutboxEvent notification, CancellationToken cancellationToken)
        {
            var type = Type.GetType(notification.Type);
            if (type == null) return;

            var domainEvent = JsonConvert.DeserializeObject(notification.Content, type);
            if (domainEvent == null) return;

            bool isHandled = false;
            string? categoryIdToUpdate = null;

            if (domainEvent is QuizCreatedEvent createdEvent)
            {
                var quiz = await _dbContext.Quizzes
                    .Include(q => q.Category)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .Include(q => q.Tags)
                    .FirstOrDefaultAsync(q => q.Id == createdEvent.QuizId, cancellationToken);

                if (quiz != null)
                {
                    var document = _mapper.Map<QuizDocument>(quiz);
                    document.SyncedAt = DateTimeOffset.UtcNow;

                    var filter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, document.Id);
                    await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                    
                    categoryIdToUpdate = quiz.CategoryId;
                    isHandled = true;
                }
            }
            else if (domainEvent is QuizUpdatedEvent updatedEvent)
            {
                // Retrieve the old category before replacement to update QuizCount if changed
                var quizFilter = Builders<QuizDocument>.Filter.Eq(doc => doc.Id, updatedEvent.QuizId);
                var oldDoc = await _collection.Find(quizFilter).FirstOrDefaultAsync(cancellationToken);
                var oldCategoryId = oldDoc?.CategoryId;

                var quiz = await _dbContext.Quizzes
                    .Include(q => q.Category)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .Include(q => q.Tags)
                    .FirstOrDefaultAsync(q => q.Id == updatedEvent.QuizId, cancellationToken);

                if (quiz != null)
                {
                    var document = _mapper.Map<QuizDocument>(quiz);
                    document.SyncedAt = DateTimeOffset.UtcNow;

                    await _collection.ReplaceOneAsync(quizFilter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                    
                    categoryIdToUpdate = quiz.CategoryId;
                    if (oldCategoryId != null && oldCategoryId != quiz.CategoryId)
                    {
                        // Update old category count as well
                        await UpdateCategoryQuizCount(oldCategoryId, cancellationToken);
                    }
                    isHandled = true;
                }
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
