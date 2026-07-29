using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Moq;
using Xunit;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Files;
using Zlearn.V2.Application.Categories.Commands.CreateCategory;
using Zlearn.V2.Application.Categories.Queries.GetCategoryById;
using Zlearn.V2.Application.Categories.DTOs;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Infas.Data;
using Zlearn.V2.Infas.Data.Repositories;
using Zlearn.V2.Infas.Data.Interceptors;
using Zlearn.V2.Infas.Data.Outbox;
using Zlearn.V2.Infas.Services.Projections;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Categories.Events;

namespace ZLearn.UnitTests
{
    public class CategoryV2Tests
    {
        private readonly IMapper _mapper;

        public CategoryV2Tests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Category, CreateResponseDto>();
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public async Task CreateCategory_Should_SaveToPostgres_And_GenerateOutboxEvent()
        {
            // Arrange: Setup EF Core InMemory DbContext với OutboxInterceptor và AuditableEntityInterceptor
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_" + Guid.NewGuid())
                .AddInterceptors(new AuditableEntityInterceptor(mockHttpContextAccessor.Object), new HandleEventsInterceptor(new OutboxSignalChannel())) // Tự động chụp audit và outbox
                .Options;

            using var context = new AppDbContext(options);

            var writeRepo = new WriteRepo<Category>(context);
            
            var mockFileRepo = new Mock<IFileRepo>();
            mockFileRepo.Setup(f => f.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Zlearn.V2.Domain.FileContext.MediaFiles.MediaFile, bool>>>()))
                .ReturnsAsync(true); // Giả định tệp Thumbnail có tồn tại

            var mockMediator = new Mock<IMediator>();

            var handler = new CreateCategoryCommandHandler(writeRepo, _mapper, mockMediator.Object, mockFileRepo.Object, mockHttpContextAccessor.Object);

            var command = new CreateCategoryCommand
            {
                Name = "Công nghệ thông tin",
                Slug = "cong-nghe-thong-tin",
                Description = "Danh mục CNTT",
                ThumbnailUrl = "http://example.com/thumbnail.png"
            };

            // Act: Thực thi Command tạo Category
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Kiểm tra dữ liệu được lưu thành công trong Postgres
            var categoryInDb = await context.Categories.FirstOrDefaultAsync(c => c.Id == result.Id);
            Assert.NotNull(categoryInDb);
            Assert.Equal("Công nghệ thông tin", categoryInDb.Name);

            // Assert: Kiểm tra OutboxEvent được sinh ra tự động nhờ Interceptor
            var outboxEvents = await context.OutboxEvents.ToListAsync();
            Assert.Single(outboxEvents);
            Assert.Contains(nameof(CategoryCreatedEvent), outboxEvents[0].Type);
            Assert.Null(outboxEvents[0].ProcessedOn);
        }

        [Fact]
        public async Task SyncCategoryToMongoHandler_Should_Call_MongoReplaceOne_With_Upsert()
        {
            // Arrange: Setup mock MongoDB
            var mockCollection = new Mock<IMongoCollection<CategoryDocument>>();
            var mockDatabase = new Mock<IMongoDatabase>();

            var mockCursor = new Mock<IAsyncCursor<CategoryDocument>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<CategoryDocument>());
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>())).Returns(true).Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
            mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<CategoryDocument>>(), It.IsAny<FindOptions<CategoryDocument, CategoryDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockDatabase.Setup(d => d.GetCollection<CategoryDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mockCollection.Object);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ZLearnTestDb_SyncCategoryToMongoHandler")
                .Options;
            using var context = new AppDbContext(options);

            var handler = new SyncCategoryToMongoHandler(mockDatabase.Object);
            
            var domainEvent = new CategoryCreatedEvent(
                "CAT123", 
                "Lập trình C#", 
                "lap-trinh-csharp", 
                "Mô tả", 
                "http://thumbnail.com/img.png"
            )
            {
                CreatedBy = "user1",
                CreatedAt = DateTimeOffset.UtcNow
            };

            // Act: Chạy Handler xử lý Projection
            await handler.Handle(domainEvent, CancellationToken.None);

            // Assert: Xác nhận Mongo C# Driver ReplaceOneAsync được gọi đúng tham số
            mockCollection.Verify(
                c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<CategoryDocument>>(),
                    It.Is<CategoryDocument>(doc => doc.Id == "CAT123" && doc.Name == "Lập trình C#"),
                    It.Is<ReplaceOptions>(opt => opt.IsUpsert),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCategoryByIdQueryHandler_Should_Query_MongoDB_Directly()
        {
            // Arrange: Setup mock Mongo Read Repo
            var mockReadRepo = new Mock<IReadRepo<CategoryDocument>>();
            
            var expectedDocument = new CategoryDocument
            {
                Id = "CAT456",
                Name = "Ngoại ngữ",
                Slug = "ngoai-ngu",
                SyncedAt = DateTimeOffset.UtcNow
            };

            mockReadRepo.Setup(r => r.GetByIdAsync("CAT456"))
                .ReturnsAsync(expectedDocument);

            var mockMediator = new Mock<IMediator>();
            var mockMapper = new Mock<IMapper>();

            var handler = new GetCategoryByIdQueryHandler(mockReadRepo.Object, mockMapper.Object, mockMediator.Object);

            // Act: Truy vấn Category bằng ID
            var query = new GetCategoryByIdQuery { Id = "CAT456" };
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert: Trả về kết quả khớp với document mock
            Assert.NotNull(result);
            Assert.Equal("CAT456", result.Id);
            Assert.Equal("Ngoại ngữ", result.Name);
            mockReadRepo.Verify(r => r.GetByIdAsync("CAT456"), Times.Once);
        }

        [Fact]
        public async Task SyncCategoryToMongoHandler_Should_Skip_Stale_Event()
        {
            // Arrange
            var mockCollection = new Mock<IMongoCollection<CategoryDocument>>();
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockCursor = new Mock<IAsyncCursor<CategoryDocument>>();

            var existingDocument = new CategoryDocument
            {
                Id = "CAT123",
                Name = "C# Nâng Cao",
                SyncedAt = DateTimeOffset.UtcNow.AddMinutes(5) // SyncedAt hiện tại là mới hơn
            };

            mockCursor.Setup(_ => _.Current).Returns(new List<CategoryDocument> { existingDocument });
            mockCursor.SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor.SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            mockCollection.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<CategoryDocument>>(), It.IsAny<FindOptions<CategoryDocument, CategoryDocument>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            mockDatabase.Setup(d => d.GetCollection<CategoryDocument>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mockCollection.Object);

            var handler = new SyncCategoryToMongoHandler(mockDatabase.Object);

            var staleEvent = new CategoryCreatedEvent(
                "CAT123",
                "Lập trình C# Cũ",
                "lap-trinh-c-cu",
                "Mô tả cũ",
                null
            );

            // Act
            await handler.Handle(staleEvent, CancellationToken.None);

            // Assert: ReplaceOneAsync KHÔNG ĐƯỢC GỌI vì event cũ bị từ chối (Idempotent Guard)
            mockCollection.Verify(
                c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<CategoryDocument>>(),
                    It.IsAny<CategoryDocument>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }
    }
}
