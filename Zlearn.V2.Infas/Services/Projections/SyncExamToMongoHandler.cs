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
        private readonly AppDbContext _dbContext;

        public SyncExamToMongoHandler(IMongoDatabase database, AppDbContext dbContext)
        {
            _collection = database.GetCollection<ExamDocument>("Exams");
            _dbContext = dbContext;
        }

        public async Task Handle(OutboxEvent notification, CancellationToken cancellationToken)
        {
            var type = Type.GetType(notification.Type);
            if (type == null) return;

            var domainEvent = JsonConvert.DeserializeObject(notification.Content, type);
            if (domainEvent == null) return;

            bool isHandled = false;

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

                // Lấy thông tin chi tiết từ DB Postgres nếu cần điền thêm các trường tùy chọn khác
                var dbExam = await _dbContext.Exams.FindAsync(new object[] { createdEvent.ExamId }, cancellationToken);
                if (dbExam != null)
                {
                    document.Note = dbExam.Note;
                    document.JoinPass = dbExam.JoinPass;
                    document.LockAccess = dbExam.LockAccess;
                    document.ShowAnswerAndKey = dbExam.ShowAnswerAndKey;
                    document.MixQuestions = dbExam.MixQuestions;
                    document.MixAnswers = dbExam.MixAnswers;
                    document.RequireJoinWithCode = dbExam.RequireJoinWithCode;
                    document.RequireJoinWithName = dbExam.RequireJoinWithName;
                    document.AllowLateSubmit = dbExam.AllowLateSubmit;
                    document.MaxParticipants = dbExam.MaxParticipants;
                }

                var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, document.Id);
                await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
                isHandled = true;
            }
            else if (domainEvent is DeletedEvent deletedEvent && deletedEvent.Id.StartsWith("EXA"))
            {
                var filter = Builders<ExamDocument>.Filter.Eq(doc => doc.Id, deletedEvent.Id);
                await _collection.DeleteOneAsync(filter, cancellationToken);
                isHandled = true;
            }

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
    }
}
