using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams.Events;

namespace Zlearn.V2.Infas.Services.Projections
{
    public class SyncExamToMongoHandler : 
        INotificationHandler<ExamCreatedEvent>,
        INotificationHandler<ExamStartedEvent>,
        INotificationHandler<ExamEndedEvent>,
        INotificationHandler<ExamDeletedEvent>
    {
        private readonly IMongoCollection<ExamDocument> _collection;

        public SyncExamToMongoHandler(IMongoDatabase database)
        {
            _collection = database.GetCollection<ExamDocument>("Exams");
        }

        public async Task Handle(ExamCreatedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, notification.ExamId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var document = new ExamDocument
            {
                Id = notification.ExamId,
                Name = notification.Name,
                Alias = notification.Alias,
                StartTime = notification.StartTime,
                EndTime = notification.EndTime,
                Status = notification.Status,
                QuizId = notification.QuizId,
                SyncedAt = notification.OccurredOn,
                Note = notification.Note,
                JoinPass = notification.JoinPass,
                LockAccess = notification.LockAccess,
                ShowAnswerAndKey = notification.ShowAnswerAndKey,
                MixQuestions = notification.MixQuestions,
                MixAnswers = notification.MixAnswers,
                RequireJoinWithCode = notification.RequireJoinWithCode,
                RequireJoinWithName = notification.RequireJoinWithName,
                AllowLateSubmit = notification.AllowLateSubmit,
                MaxParticipants = notification.MaxParticipants,
                CreatedBy = notification.CreatedBy,
                CreatedAt = notification.CreatedAt
            };

            await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        }

        public async Task Handle(ExamStartedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, notification.ExamId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var update = Builders<ExamDocument>.Update
                .Set(doc => doc.Status, "InProgress")
                .Set(doc => doc.SyncedAt, notification.OccurredOn);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task Handle(ExamEndedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, notification.ExamId);
            var existingDoc = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (existingDoc != null && existingDoc.SyncedAt > notification.OccurredOn)
            {
                return;
            }

            var update = Builders<ExamDocument>.Update
                .Set(doc => doc.Status, "Ended")
                .Set(doc => doc.SyncedAt, notification.OccurredOn);
            await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }

        public async Task Handle(ExamDeletedEvent notification, CancellationToken cancellationToken)
        {
            var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, notification.Id);
            await _collection.DeleteOneAsync(filter, cancellationToken);
        }
    }
}
