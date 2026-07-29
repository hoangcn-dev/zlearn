using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Xunit;
using IRedisService = Zlearn.V2.Application.Common.Interfaces.IRedisService;
using ISchedulerService = Zlearn.V2.Application.Common.Interfaces.ISchedulerService;
using Microsoft.Extensions.Caching.Memory;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Quizzes;
using Zlearn.V2.Application.Exams.Commands.CreateExam;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Repositories;
using Zlearn.V2.Infas.Data.Interceptors;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Zlearn.V2.Infas.Services.Projections;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Exams.Events;
using Zlearn.V2.Domain.CatalogContext.Quizzes;

namespace ZLearn.UnitTests
{
    public class ExamV2Tests
    {
        private readonly IMapper _mapper;

        public ExamV2Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Exam, CreateResponseDto>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task CreateExam_Should_SaveToPostgres_And_GenerateOutboxEvent()
        {
            // Arrange
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user_id_123") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_Exam_" + Guid.NewGuid())
                .AddInterceptors(new AuditableEntityInterceptor(mockHttpContextAccessor.Object), new HandleEventsInterceptor(new OutboxSignalChannel()))
                .Options;

            using var context = new AppDbContext(options);

            var mockQuizRepo = new Mock<IQuizWriteRepo>();
            mockQuizRepo.Setup(q => q.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Quiz, bool>>>()))
                .ReturnsAsync(true);

            var mockScheduler = new Mock<ISchedulerService>();
            mockScheduler.Setup(s => s.ScheduleCommandExactly(It.IsAny<IRequest>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
                .ReturnsAsync("job_id_123");

            var mockRedis = new Mock<IRedisService>();
            var mockMemoryCache = new Mock<IMemoryCache>();

            var examRepo = new ExamRepo(context, mockScheduler.Object, mockRedis.Object, mockMemoryCache.Object);

            var mockLogger = new Mock<ILogger<CreateExamCommandHandler>>();
            var mockMediator = new Mock<IMediator>();

            var handler = new CreateExamCommandHandler(
                _mapper,
                mockMediator.Object,
                examRepo,
                mockQuizRepo.Object,
                mockScheduler.Object,
                mockLogger.Object
            );

            var command = new CreateExamCommand
            {
                UserId = "user_id_123",
                Data = new CreateExamDto
                {
                    Name = "Kiểm tra học kỳ I",
                    QuizId = "QZ_123",
                    StartTime = DateTimeOffset.UtcNow.AddHours(1),
                    EndTime = DateTimeOffset.UtcNow.AddHours(2),
                    MixQuestions = true,
                    MixAnswers = false,
                    MaxParticipants = 50,
                    ShowAnswerAndKey = true,
                    RequireJoinWithCode = false,
                    RequireJoinWithName = true,
                    AllowLateSubmit = false
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Kiểm tra Postgres DB
            var examInDb = await context.Exams.FirstOrDefaultAsync(e => e.Id == result.Id);
            Assert.NotNull(examInDb);
            Assert.Equal("Kiểm tra học kỳ I", examInDb.Name);
            Assert.Equal("QZ_123", examInDb.QuizId);

            // Assert: Kiểm tra OutboxEvent được sinh ra
            var outboxEvents = await context.OutboxEvents.ToListAsync();
            Assert.Single(outboxEvents);
            Assert.Contains(nameof(ExamCreatedEvent), outboxEvents[0].Type);
        }

        [Fact]
        public async Task SyncExamToMongoHandler_Should_Call_MongoReplaceOne_With_Upsert()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<ExamDocument>>();
            var mockDatabase = new Mock<IMongoDatabase>();

            var mockCursor = new Mock<IAsyncCursor<ExamDocument>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<ExamDocument>());
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<ExamDocument>>(), It.IsAny<FindOptions<ExamDocument, ExamDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockDatabase.Setup(d => d.GetCollection<ExamDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mockCollection.Object);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_SyncExam_" + Guid.NewGuid())
                .Options;
            using var context = new AppDbContext(options);

            // Seed an exam in Postgres to fetch additional details
            var examId = "EXA_999";
            var examSeed = new Exam(
                "Kiểm tra học kỳ II",
                "alias123",
                "QZ_999",
                null,
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                ExamStatus.InProgress,
                true,
                100,
                false,
                false,
                false,
                false,
                false,
                "Note test",
                id: examId
            );
            examSeed.CreatedBy = "user_id_123";
            examSeed.CreatedAt = DateTimeOffset.UtcNow;
            context.Exams.Add(examSeed);
            await context.SaveChangesAsync();

            var handler = new SyncExamToMongoHandler(mockDatabase.Object);

            var createdEvent = new ExamCreatedEvent(
                ExamId: examId,
                Name: "Kiểm tra học kỳ II",
                Alias: "alias123",
                QuizId: "QZ_999",
                StartTime: DateTimeOffset.UtcNow,
                EndTime: DateTimeOffset.UtcNow.AddHours(1),
                Status: "InProgress",
                Note: "Note test",
                JoinPass: null,
                LockAccess: false,
                ShowAnswerAndKey: false,
                MixQuestions: false,
                MixAnswers: false,
                RequireJoinWithCode: false,
                RequireJoinWithName: false,
                AllowLateSubmit: false,
                MaxParticipants: 100
            );

            // Act
            await handler.Handle(createdEvent, CancellationToken.None);

            // Assert
            mockCollection.Verify(
                c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ExamDocument>>(),
                    It.Is<ExamDocument>(d => d.Id == examId && d.Name == "Kiểm tra học kỳ II" && d.Note == "Note test"),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task OutboxProcessorJob_Should_Break_Loop_When_Event_In_Same_Transaction_Fails()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_OutboxBreak_" + Guid.NewGuid())
                .Options;

            using var context = new AppDbContext(options);

            var txId = Guid.NewGuid();
            var event1 = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = txId,
                Type = typeof(ExamCreatedEvent).AssemblyQualifiedName!,
                Content = "INVALID_JSON_CONTENT_TRIGGERING_EXCEPTION",
                OccurredOn = DateTimeOffset.UtcNow.AddSeconds(-10)
            };

            var validEvent = new ExamCreatedEvent("EXA_999", "Exam Test", "alias", "QZ_1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), "Draft", "Note", null, false, false, false, false, false, false, false, 10);
            var event2 = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = txId,
                Type = typeof(ExamCreatedEvent).AssemblyQualifiedName!,
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(validEvent),
                OccurredOn = DateTimeOffset.UtcNow.AddSeconds(-5)
            };

            context.OutboxEvents.AddRange(event1, event2);
            await context.SaveChangesAsync();

            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<OutboxProcessorJob>>();
            var mockChannel = new Mock<IOutboxSignalChannel>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(serviceScopeMock.Object);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(context);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator))).Returns(mockMediator.Object);

            var job = new OutboxProcessorJob(serviceProviderMock.Object, mockChannel.Object, mockLogger.Object);

            // Act: Invoking ProcessOutboxEventsAsync via Reflection
            var method = typeof(OutboxProcessorJob).GetMethod("ProcessOutboxEventsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(job, new object[] { CancellationToken.None })!;

            // Assert
            var dbEvent1 = await context.OutboxEvents.FindAsync(event1.Id);
            var dbEvent2 = await context.OutboxEvents.FindAsync(event2.Id);

            Assert.NotNull(dbEvent1?.Error);
            Assert.Equal(1, dbEvent1.RetryCount);
            Assert.Null(dbEvent1.ProcessedOn);

            // event2 MUST NOT be processed because loop broke on event1 exception!
            Assert.Null(dbEvent2?.ProcessedOn);
            Assert.Equal(0, dbEvent2.RetryCount);
        }

        [Fact]
        public async Task OutboxProcessorJob_Should_Move_Event_To_DeadLetter_When_Max_Retries_Exceeded()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_OutboxDeadLetter_" + Guid.NewGuid())
                .Options;

            using var context = new AppDbContext(options);

            var failedEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                Type = typeof(ExamCreatedEvent).AssemblyQualifiedName!,
                Content = "INVALID_JSON",
                OccurredOn = DateTimeOffset.UtcNow,
                RetryCount = 4 // Retry lần thứ 5 sẽ bị đánh dấu IsDeadLetter = true
            };

            context.OutboxEvents.Add(failedEvent);
            await context.SaveChangesAsync();

            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<OutboxProcessorJob>>();
            var mockChannel = new Mock<IOutboxSignalChannel>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(serviceScopeMock.Object);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(context);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator))).Returns(mockMediator.Object);

            var job = new OutboxProcessorJob(serviceProviderMock.Object, mockChannel.Object, mockLogger.Object);

            // Act
            var method = typeof(OutboxProcessorJob).GetMethod("ProcessOutboxEventsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(job, new object[] { CancellationToken.None })!;

            // Assert
            var dbEvent = await context.OutboxEvents.FindAsync(failedEvent.Id);
            Assert.NotNull(dbEvent);
            Assert.Equal(5, dbEvent.RetryCount);
            Assert.True(dbEvent.IsDeadLetter);
            Assert.NotNull(dbEvent.Error);
        }

        [Fact]
        public async Task OutboxSignalChannel_Should_Notify_And_Signal_Consumer()
        {
            // Arrange
            var channel = new OutboxSignalChannel();

            // Act
            channel.Notify();

            // Assert
            var hasData = await channel.WaitToReadAsync(CancellationToken.None);
            Assert.True(hasData);
        }

        [Fact]
        public async Task OutboxProcessorJob_Should_Publish_To_RabbitMQ_When_Publisher_Is_Registered()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_RabbitMQ_" + Guid.NewGuid())
                .Options;

            using var context = new AppDbContext(options);

            var validEvent = new ExamCreatedEvent("EXA_100", "Exam RabbitMQ", "alias", "QZ_1", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1), "Draft", "Note", null, false, false, false, false, false, false, false, 10);
            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = Guid.NewGuid(),
                Type = typeof(ExamCreatedEvent).AssemblyQualifiedName!,
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(validEvent),
                OccurredOn = DateTimeOffset.UtcNow
            };

            context.OutboxEvents.Add(outboxEvent);
            await context.SaveChangesAsync();

            var mockPublisher = new Mock<Zlearn.V2.Infas.Messaging.IRabbitMQPublisherService>();
            mockPublisher.Setup(p => p.PublishEventAsync(It.IsAny<OutboxEvent>())).Returns(Task.CompletedTask);

            var mockMediator = new Mock<IMediator>();
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<OutboxProcessorJob>>();
            var mockChannel = new Mock<IOutboxSignalChannel>();
            var serviceProviderMock = new Mock<IServiceProvider>();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

            serviceScopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
            serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(serviceScopeMock.Object);

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory))).Returns(serviceScopeFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(AppDbContext))).Returns(context);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMediator))).Returns(mockMediator.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(Zlearn.V2.Infas.Messaging.IRabbitMQPublisherService))).Returns(mockPublisher.Object);

            var job = new OutboxProcessorJob(serviceProviderMock.Object, mockChannel.Object, mockLogger.Object);

            // Act
            var method = typeof(OutboxProcessorJob).GetMethod("ProcessOutboxEventsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method!.Invoke(job, new object[] { CancellationToken.None })!;

            // Assert
            mockPublisher.Verify(p => p.PublishEventAsync(It.Is<OutboxEvent>(e => e.Id == outboxEvent.Id)), Times.Once);
            var dbEvent = await context.OutboxEvents.FindAsync(outboxEvent.Id);
            Assert.NotNull(dbEvent?.ProcessedOn);
        }
    }
}
