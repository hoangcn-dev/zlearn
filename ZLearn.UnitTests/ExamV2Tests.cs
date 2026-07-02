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
                .AddInterceptors(new AuditableEntityInterceptor(mockHttpContextAccessor.Object), new HandleEventsInterceptor(new Mock<IMediator>().Object))
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

            mockDatabase.Setup(d => d.GetCollection<ExamDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mockCollection.Object);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_SyncExam_" + Guid.NewGuid())
                .Options;
            using var context = new AppDbContext(options);

            // Seed an exam in Postgres to fetch additional details
            var examId = "EXA_999";
            var examSeed = new Exam(
                examId,
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
                "Note test"
            );
            examSeed.CreatedBy = "user_id_123";
            examSeed.CreatedAt = DateTimeOffset.UtcNow;
            context.Exams.Add(examSeed);
            await context.SaveChangesAsync();

            var handler = new SyncExamToMongoHandler(mockDatabase.Object, context);

            var createdEvent = new ExamCreatedEvent(
                ExamId: examId,
                Name: "Kiểm tra học kỳ II",
                Alias: "alias123",
                QuizId: "QZ_999",
                StartTime: DateTimeOffset.UtcNow,
                EndTime: DateTimeOffset.UtcNow.AddHours(1),
                Status: "InProgress"
            );

            var outboxEvent = new OutboxEvent
            {
                Id = Guid.NewGuid(),
                Type = typeof(ExamCreatedEvent).AssemblyQualifiedName!,
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(createdEvent),
                OccurredOn = DateTimeOffset.UtcNow
            };
            context.OutboxEvents.Add(outboxEvent);
            await context.SaveChangesAsync();

            // Act
            await handler.Handle(outboxEvent, CancellationToken.None);

            // Assert
            mockCollection.Verify(
                c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<ExamDocument>>(),
                    It.Is<ExamDocument>(d => d.Id == examId && d.Name == "Kiểm tra học kỳ II" && d.Note == "Note test"),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // OutboxEvent should be marked processed
            var outboxInDb = await context.OutboxEvents.FindAsync(outboxEvent.Id);
            Assert.NotNull(outboxInDb);
            Assert.NotNull(outboxInDb.ProcessedOn);
        }
    }
}
