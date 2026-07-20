using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using Newtonsoft.Json;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams.Events;
using Zlearn.V2.Domain.Common;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Outbox;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncExamToMongoHandler : INotificationHandler<OutboxEvent>
    {
        private readonly IMongoCollection<ExamDocument> _collection;
        private readonly OutboxFallbackHelper _fallbackHelper;

        public SyncExamToMongoHandler(IMongoDatabase database, OutboxFallbackHelper fallbackHelper)
        {
            _collection = database.GetCollection<ExamDocument>("Exams");
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

            if (domainEvent is ExamCreatedEvent createdEvent)
            {
                var document = new ExamDocument
                {
                    Id = createdEvent.ExamId,
                    Name = createdEvent.Name,
                    Alias = createdEvent.Alias,
                    StartTime = createdEvent.StartTime,
                    EndTime = createdEvent.EndTime,
                    Status = createdEvent.Status,
                    QuizId = createdEvent.QuizId,
                    SyncedAt = DateTimeOffset.UtcNow
                };

                document.Note = createdEvent.Note;
                document.JoinPass = createdEvent.JoinPass;
                document.LockAccess = createdEvent.LockAccess;
                document.ShowAnswerAndKey = createdEvent.ShowAnswerAndKey;
                document.MixQuestions = createdEvent.MixQuestions;
                document.MixAnswers = createdEvent.MixAnswers;
                document.RequireJoinWithCode = createdEvent.RequireJoinWithCode;
                document.RequireJoinWithName = createdEvent.RequireJoinWithName;
                document.AllowLateSubmit = createdEvent.AllowLateSubmit;
                document.MaxParticipants = createdEvent.MaxParticipants;
                document.CreatedBy = createdEvent.CreatedBy;
                document.CreatedAt = createdEvent.CreatedAt;

                var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, document.Id);
                await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
            }
            else if (domainEvent is DeletedEvent deletedEvent && deletedEvent.Id.StartsWith("EXA"))
            {
                var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, deletedEvent.Id);
                await _collection.DeleteOneAsync(filter, cancellationToken);
            }
        }
    }
}
