# Thiết kế Lớp Cơ sở hạ tầng (Infrastructure Layer)

Lớp **Infrastructure** cung cấp các cài đặt kỹ thuật chi tiết để giao tiếp với hệ thống bên ngoài (PostgreSQL, MongoDB, Redis, Cloudinary, API AI), xử lý bảo mật (Identity, JWT), chạy tác vụ nền, và giám sát hệ thống (Serilog).

Thư mục chính: [Zlearn.V2.Infas](file:///d:/projects/zlearn/Zlearn.V2.Infas)

---

## 1. Cơ sở dữ liệu & Data Access

Dự án áp dụng mô hình dữ liệu CQRS:
- **Write Db (PostgreSQL)**: Toàn bộ quá trình thay đổi dữ liệu được commit vào PostgreSQL qua EF Core.
- **Read Db (MongoDB)**: Toàn bộ quá trình truy vấn đọc dữ liệu lấy trực tiếp từ MongoDB qua Read Repository để tối đa tốc độ phản hồi.

### PostgreSQL & EF Core
- **DbContext**: [AppDbContext.cs](file:///d:/projects/zlearn/Zlearn.V2.Infas/Data/AppDbContext.cs) kế thừa từ `IdentityDbContext<AppIdentityUser, AppIdentityRole, string>` để quản lý cả thực thể nghiệp vụ và thực thể danh tính.
- **Cơ chế Fluent API**: Các cấu hình bảng dữ liệu, khóa ngoại, chỉ mục được tổ chức tách biệt tại thư mục [Data/Configurations/](file:///d:/projects/zlearn/Zlearn.V2.Infas/Data/Configurations).
- **Database Initializer**: [Initializer.cs](file:///d:/projects/zlearn/Zlearn.V2.Infas/Data/Initializer.cs) tự động chạy migrations khi khởi động (`MigrateAsync()`) và tạo dữ liệu mồi (Seed Data) như tài khoản Admin mặc định, các Roles hệ thống.

### MongoDB & Read Repositories
- Cung cấp triển khai cho `IReadRepo<TDocument>` trỏ tới MongoDB, giúp truy xuất thông tin đề thi và câu hỏi dạng tài liệu JSON chuẩn hóa, giảm tải tối đa cho PostgreSQL.

### EF Core SaveChanges Interceptors
Dự án đăng ký các interceptor chạy trước và sau khi lưu dữ liệu vào PostgreSQL:
1. **[AuditableEntityInterceptor](file:///d:/projects/zlearn/Zlearn.V2.Infas/Data/Interceptors/AuditableEntityInterceptor.cs)**: Tự động ghi nhận `CreatedBy`, `CreatedAt` khi thêm mới và `ModifiedBy`, `LastModifiedAt` khi cập nhật dữ liệu kế thừa từ `AuditableEntity`.
2. **[HandleEventsInterceptor](file:///d:/projects/zlearn/Zlearn.V2.Infas/Data/Interceptors/HandleEventsInterceptor.cs)**: Tự động quét các thực thể `AggregateRoot` có chứa `UncommittedEvents`, tuần tự hóa (serialize) các sự kiện này thành bản ghi `OutboxEvent` lưu vào PostgreSQL trong cùng một Transaction qua phương thức `SavingChangesAsync`. Tiếp tục gửi MediatR Notification trong `SavedChangesAsync`.

---

## 2. Xác thực & Phân quyền (Authentication & Authorization)

Dự án sử dụng cơ chế xác thực kết hợp giữa **JWT Bearer Token** (cho các API) và **Cookies/Google OAuth** (cho Web).

### Identity & User Management
- **AppIdentityUser** & **AppIdentityRole**: Quản lý thông tin tài khoản và nhóm quyền người dùng.
- **IdentityService** (`Data/Services/IdentityService.cs`): Thực hiện các nghiệp vụ đăng nhập (`AuthenticateAsync`), đăng nhập Google (`AuthenticateWithGoogle`), đăng xuất (`EndSessionAsync`), gia hạn token (`RefreshToken`), lấy thông tin và cập nhật profile người dùng.

### JwtManager (JWT & Blacklist qua Redis)
- **IssueToken**: Sinh cặp Access Token (thuật toán `HmacSha256`) và Refresh Token.
- **Refresh Token Management**: Refresh Token được lưu trữ tại Redis dưới key `RedisKeys.REFRESH_TOKEN` ánh xạ theo `userId`.
- **Revoke & Blacklist**: Khi người dùng đăng xuất, Access Token cũ được đưa vào Redis Blacklist với key `RedisKeys.REVOKED_ACCESS_TOKEN` để ngăn chặn việc tái sử dụng.
- **JwtMiddleware** (`ZLearn.Web.Middlewares.JwtMiddleware`): Kiểm tra từng request, nếu token nằm trong blacklist Redis thì lập tức từ chối và trả về HTTP 401.

---

## 3. Caching & Redis

Sử dụng thư viện **StackExchange.Redis** được cấu hình trong `AddRedisService`:
- Interface **IRedisService** cung cấp các hàm tiện ích: `Get`, `Set`, `Delete`, `IsExists`, `UpdateAndKeepTTL` truy cập trực tiếp vào Redis Server.
- Dùng để quản lý Refresh Token, Blacklist Access Token, và lưu vết tạm thời của các phiên thi trực tuyến (Exam tracking).

---

## 4. Truyền thông Thời gian thực (SignalR Hubs)

Dự án sử dụng **ASP.NET Core SignalR** để cập nhật trạng thái thời gian thực cho người dùng và quản trị viên.

1. **AccessTrackingHub**: Mapped tới `/access-tracking`.
   - Kết hợp [AccessTrackingService](file:///d:/projects/zlearn/Zlearn.V2.Infas/Services/AccessTracking/AccessTrackingService.cs) lưu trữ số lượng kết nối đồng thời từ các địa chỉ IP khác nhau.
2. **ExamHub**: Mapped tới `/hubs/exam`.
   - Phục vụ cho tính năng giám sát trực tuyến quá trình làm bài thi của các thí sinh (Exam tracking).

---

## 5. Tác vụ nền & Lập lịch (Background Jobs & Schedulers)

Hệ thống tích hợp cả **Quartz.NET** và **Hangfire** phục vụ cho các loại tác vụ nền khác nhau:

- **Quartz.NET**: Xử lý các công việc cần lập lịch chính xác có lưu trạng thái bền vững giữa các lần khởi động ứng dụng (ví dụ: Tự động Mở/Khóa kỳ thi theo đúng giờ hẹn).
- **Outbox Processor Job** (`Data/Services/OutboxProcessorJob.cs`): Chạy ngầm định kỳ quét bảng `OutboxEvents` trong PostgreSQL và publish chúng sang các handlers để đồng bộ dữ liệu PostgreSQL sang MongoDB qua các Projections (như `SyncQuizToMongoHandler.cs`, `SyncExamToMongoHandler.cs`).
- **Exam Grading Service** (`Data/Services/ExamGradingBackgroundService.cs`): Hosted Service chạy ngầm lấy bài thi đã nộp từ Redis queue và thực hiện chấm điểm tự động.
- **Hosted Services**:
  - `DatabaseBackupService`: Chạy định kỳ, sử dụng `pg_dump` để tự động sao lưu PostgreSQL ra file backup cục bộ.
  - `RemoveUnusedFilesService`: Quét bảng `MediaFiles`, tìm các file có trường `IsUsing == false` và xóa chúng khỏi Cloudinary.

---

## 6. Các Dịch vụ Tích hợp bên ngoài (External Services)

- **Groq AI Service**: Kết nối với Groq Cloud API, tự động giải thích câu hỏi trắc nghiệm, tạo câu hỏi tự động.
- **Cloudinary Storage**: Thực hiện lưu trữ file đa phương tiện.
- **QuestPDF & OpenXml**: Thực hiện kết xuất đề thi ra file Word (.docx) và PDF chất lượng cao.

---

## 7. Ghi nhật ký (Serilog Logging)

- **Serilog** ghi nhận nhật ký hệ thống ra cả Console và Rolling Files.
- **LogMiddleware**: Custom Middleware bắt tất cả các Request đi vào hệ thống, đo thời gian xử lý và ghi nhận log chi tiết thông tin API.
