# Thiết kế Lớp Cơ sở hạ tầng (Infrastructure Layer)

Lớp **Infrastructure** cung cấp các cài đặt kỹ thuật chi tiết để giao tiếp với hệ thống bên ngoài (PostgreSQL, Redis, Cloudinary, API AI), xử lý bảo mật (Identity, JWT), chạy tác vụ nền, và giám sát hệ thống (Serilog).

Thư mục chính: [ZLearn.Infras](file:///d:/projects/zlearn/ZLearn.Infras)

---

## 🗄️ 1. Cơ sở dữ liệu & EF Core (Data Access)

Dự án sử dụng **Entity Framework Core (EF Core)** để kết nối với cơ sở dữ liệu **PostgreSQL**.

*   **DbContext**: [AppDbContext.cs](file:///d:/projects/zlearn/ZLearn.Infras/Data/AppDbContext.cs) kế thừa từ `IdentityDbContext<AppUser, AppRole, string>` để quản lý cả thực thể nghiệp vụ và thực thể xác thực.
*   **Cơ chế Fluent API**: Các cấu hình bảng dữ liệu, khóa ngoại, chỉ mục được tổ chức tách biệt tại thư mục [Data/Configurations/](file:///d:/projects/zlearn/ZLearn.Infras/Data/Configurations).
*   **Database Initializer**: [Initializer.cs](file:///d:/projects/zlearn/ZLearn.Infras/Data/Initializer.cs) tự động chạy migrations khi khởi động (`MigrateAsync()`) và tạo dữ liệu mồi (Seed Data) như tài khoản Admin mặc định, các Roles hệ thống.

### 🛡️ EF Core SaveChanges Interceptors
Dự án đăng ký hai bộ interceptor chạy trước khi lưu dữ liệu vào PostgreSQL:
1.  **[AuditableEntityInterceptor](file:///d:/projects/zlearn/ZLearn.Infras/Data/Interceptors/AuditableEntityInterceptor.cs)**:
    *   Tự động phát hiện các thực thể kế thừa từ `AuditableEntity`.
    *   Ghi đè `CreatedBy`, `CreatedAt` khi thêm mới.
    *   Ghi đè `ModifiedBy`, `LastModifiedAt` khi thêm/sửa đổi.
    *   Sử dụng `IHttpContextAccessor` để lấy UserId hiện tại từ token xác thực của request.
2.  **[DispatchEventsInterceptor](file:///d:/projects/zlearn/ZLearn.Infras/Data/Interceptors/DispatchEventsInterceptor.cs)**:
    *   Quét qua toàn bộ thực thể kế thừa từ `BaseEntity` đang được ChangeTracker quản lý.
    *   Lấy ra danh sách các `Events` nghiệp vụ và dọn sạch danh sách sự kiện trong thực thể (`ClearEvents()`).
    *   Publish bất đồng bộ toàn bộ sự kiện thông qua MediatR `_mediator.Publish(e)` trước khi commit vào database.

### 🏛️ Mẫu thiết kế Repository (Repository Pattern)
*   **Base Repo**: [BaseRepo.cs](file:///d:/projects/zlearn/ZLearn.Infras/Data/Repositories/BaseRepo.cs) triển khai interface generic `IBaseRepo<TEntity>` cung cấp các thao tác cơ bản: `Any()`, `Create()`, `CreateRange()`, `Delete()`, `Get()`, `GetAll()`, `GetPaging()` (phân trang trả về `PaginatedDto`), và `SaveChanges()`.
*   **Các Repository Chuyên biệt**: Kế thừa từ `BaseRepo<TEntity>` và triển khai thêm các query tối ưu (ví dụ: `ExamRepo`, `QuizRepo`).

---

## 🔒 2. Xác thực & Phân quyền (Authentication & Authorization)

Dự án sử dụng cơ chế xác thực kết hợp giữa **JWT Bearer Token** (cho các API) và **Cookies/Google OAuth** (cho Web/External).

### 🎫 Identity & User Management
*   **AppUser**: Kế thừa `IdentityUser<string>`, bổ sung các trường: `ImageUrl`, `FirstName`, `LastName`, `NickName`, `IsShowNickName`, `LastLogin`, `IsActive`.
*   **AppRole**: Kế thừa `IdentityRole<string>`.
*   **IdentityService**: Thực hiện các nghiệp vụ đăng nhập (`AuthenticateAsync`), đăng nhập bên thứ 3 (`AuthenticateWithGoogle`), đăng xuất (`EndSessionAsync`), gia hạn token (`RefreshToken`), lấy thông tin và cập nhật profile người dùng.

### 🔑 JwtManager (JWT & Blacklist qua Redis)
[JwtManager.cs](file:///d:/projects/zlearn/ZLearn.Infras/Services/Identity/JwtManager.cs) quản lý vòng đời của token:
*   **IssueToken**: Sinh cặp Access Token (ký bằng thuật toán `HmacSha256`, chứa thông tin định danh và Roles) và Refresh Token (chuỗi ngẫu nhiên 32 ký tự).
*   **Refresh Token Management**: Refresh Token được lưu trữ tại Redis dưới key `RedisKeys.REFRESH_TOKEN` ánh xạ theo `userId`, thời gian sống theo cấu hình `RTExpirationMinutes`.
*   **Revoke & Blacklist**: Khi người dùng đăng xuất:
    *   Lưu Access Token vào Redis Blacklist với key `RedisKeys.REVOKED_ACCESS_TOKEN` để ngăn chặn việc tái sử dụng Token này (thời gian sống bằng thời gian còn lại của Access Token).
    *   Xóa Refresh Token khỏi Redis.
*   **JwtMiddleware** (`ZLearn.Web.Middlewares.JwtMiddleware`): Kiểm tra từng request, nếu token nằm trong blacklist Redis thì lập tức từ chối và trả về HTTP 401.

---

## ⚡ 3. Caching (Redis Service)

Sử dụng thư viện **StackExchange.Redis** được cấu hình trong `AddRedisService`:
*   Interface **IRedisService** cung cấp các hàm tiện ích: `Get`, `Set`, `Delete`, `IsExists`, `UpdateAndKeepTTL` truy cập trực tiếp vào Redis Server.
*   Dùng để quản lý Refresh Token, Blacklist Access Token, và lưu vết tạm thời của các phiên thi trực tuyến (Exam tracking).

---

## 📡 4. Truyền thông Thời gian thực (SignalR Hubs)

Dự án sử dụng **ASP.NET Core SignalR** để cập nhật trạng thái thời gian thực cho người dùng và quản trị viên.

1.  **AccessTrackingHub**: Mapped tới `/access-tracking`.
    *   Sử dụng [AccessTrackingService.cs](file:///d:/projects/zlearn/ZLearn.Infras/Services/AccessTracking/AccessTrackingService.cs) lưu trữ số lượng kết nối đồng thời từ các địa chỉ IP khác nhau thông qua một `ConcurrentDictionary<string, int>`.
    *   Mỗi khi có kết nối mới hoặc ngắt kết nối, SignalR Hub sẽ kích hoạt để đếm số lượng người dùng đang online và phát đi thông điệp cập nhật `UpdateAccessCount` tới tất cả các client đang kết nối.
2.  **ExamHub**: Mapped tới `ExamHub.HUB_URL`.
    *   Phục vụ cho tính năng giám sát trực tuyến quá trình làm bài thi của các thí sinh (Exam tracking).
    *   Cập nhật trạng thái thí sinh tham gia, thí sinh nộp bài hoặc mất kết nối thời gian thực cho quản trị viên.

---

## 🗓️ 5. Tác vụ nền & Lập lịch (Background Jobs & Schedulers)

Hệ thống tích hợp cả **Quartz.NET** và **Hangfire** phục vụ cho các loại tác vụ nền khác nhau:

*   **Quartz.NET**: Lưu trữ các Jobs trực tiếp trong PostgreSQL (`UsePostgres`). Sử dụng cho các công việc cần lập lịch chính xác có lưu trạng thái bền vững giữa các lần khởi động ứng dụng (ví dụ: Tự động Mở/Khóa kỳ thi theo đúng giờ đã hẹn).
*   **Hangfire**: Sử dụng bộ nhớ tạm (`UseMemoryStorage`) để xếp hàng xử lý các tác vụ nền nhanh chóng, không chặn tiến trình chính.
*   **IHostedService (Hosted Services)**:
    *   **DatabaseBackupService**: Chạy định kỳ, sử dụng công cụ dòng lệnh `pg_dump` (được cài đặt sẵn trong Docker container) để tự động sao lưu cấu trúc và dữ liệu của PostgreSQL ra các file backup và lưu trữ cục bộ.
    *   **RemoveUnusedFilesService**: Chạy định kỳ quét bảng `MediaFiles`, tìm các file có trường `IsUsing == false` và đã quá tuổi quy định để xóa chúng khỏi Cloudinary và cơ sở dữ liệu nhằm tiết kiệm dung lượng.

---

## 🤖 6. Các Dịch vụ Tích hợp bên ngoài (External Services)

*   **Groq AI Service** (`GroqService`): Kết nối với Groq Cloud API, cung cấp interface `IAIService` để thực hiện tạo các nội dung văn bản thông minh (ví dụ: Tự động giải thích câu hỏi trắc nghiệm, tạo câu hỏi tự động).
*   **Cloudinary Storage** (`CloudinaryService`): Thực hiện lưu trữ file đa phương tiện. Khi người dùng upload ảnh câu hỏi/đáp án/avatar, tệp tin được đẩy trực tiếp lên Cloudinary CDN và trả về URL ảnh.

---

## 📝 7. Ghi nhật ký (Serilog Logging)

*   Cấu hình thông qua `AddLogService` trong [DependencyInjection.cs](file:///d:/projects/zlearn/ZLearn.Infras/DependencyInjection.cs).
*   **Serilog** ghi nhận nhật ký hệ thống ra cả Console (định dạng màu sắc dễ đọc khi debug) và ghi ra Rolling Files đặt tại thư mục build của ứng dụng (Tự động xoay vòng file log theo ngày).
*   **LogMiddleware**: Custom Middleware bắt tất cả các Request đi vào hệ thống, đo thời gian xử lý và ghi nhận log chi tiết thông tin API, mã trạng thái HTTP trả về, hỗ trợ đắc lực cho việc giám sát lỗi vận hành.
