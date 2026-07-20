# ZLearn Documentation Index

Chào mừng bạn đến với hệ thống tài liệu kiến trúc của dự án **ZLearn**. Tài liệu này được thiết kế và cấu trúc đặc biệt để hỗ trợ các nhà phát triển và đặc biệt là trợ lý AI (như Antigravity) dễ dàng truy xuất thông tin, hiểu rõ cấu trúc dự án, từ đó xây dựng các tính năng mới một cách chuẩn xác nhất theo đúng thiết kế hệ thống.

---

## Bản đồ Tài liệu (Documentation Map)

Hãy bấm trực tiếp vào các liên kết dưới đây để xem chi tiết từng thành phần:

1.  **[Tổng quan Kiến trúc (Architecture Overview)](file:///d:/projects/zlearn/docs/architecture_overview.md)**
    *   Mô tả mô hình Clean/Onion Architecture của dự án.
    *   Phân tích sơ đồ quan hệ phụ thuộc giữa các project (Layers Dependency).
    *   Danh sách Công nghệ chính (Technology Stack).
2.  **[Lớp Nghiệp vụ Lõi (Domain Layer)](file:///d:/projects/zlearn/docs/domain_layer.md)**
    *   Cơ chế thiết kế của `BaseEntity`, `AuditableEntity`.
    *   Thiết kế hệ thống Domain Events thông qua `BaseEvent` và cơ chế gom sự kiện.
    *   Chi tiết các thực thể (Entities) trong hệ thống và thuộc tính của chúng.
3.  **[Lớp Ứng dụng (Application Layer)](file:///d:/projects/zlearn/docs/application_layer.md)**
    *   Mẫu kiến trúc CQRS (Command Query Responsibility Segregation) dùng MediatR.
    *   Cách thức tổ chức Command, Query, Handler, Validator, DTO.
    *   Cơ chế Pipeline Behaviors xử lý Validation tự động.
    *   Cách sử dụng các Script Scaffolding (`create-command.bat`, `create-query.bat`, `create-event.bat`).
4.  **[Lớp Cơ sở Hạ tầng (Infrastructure Layer)](file:///d:/projects/zlearn/docs/infrastructure_layer.md)**
    *   Cơ chế truy xuất dữ liệu: EF Core + PostgreSQL (`AppDbContext`, Configurations, Repositories).
    *   EF Core Interceptors: Xử lý tự động Audit fields và Dispatch Domain Events.
    *   Authentication & Security: JWT, Cookies, Google Auth, Authorization Policies.
    *   Caching & Real-time: Redis Service và SignalR Hubs (`AccessTrackingHub`, `ExamHub`).
    *   Background Jobs & Tasks: Quartz.NET, Hangfire, Hosted Services.
    *   External Services: Groq AI và Cloudinary Store.
    *   Logging: Serilog và LogMiddleware.
5.  **[Lớp Hiển thị (Presentation Layer)](file:///d:/projects/zlearn/docs/presentation_layer.md)**
    *   Web App (`ZLearn.Web`): Controller MVC, API Controllers (đã hợp nhất từ `ZLearn.API` cũ), Views, ViewComponents, wwwroot.
    *   Admin Desktop App (`ZLearn.AdminDesktopApp`): Ứng dụng WPF sử dụng CommunityToolkit.Mvvm, quản lý NavigationStore, tổ chức theo Features.
6.  **[Triển khai & Vận hành (DevOps & Deployment)](file:///d:/projects/zlearn/docs/devops_deployment.md)**
    *   Dockerization: Phân tích `Dockerfile` và `docker-compose.yaml`.
    *   Các tệp Script hỗ trợ build, deploy và run (`build.bat`, `deploy.bat`, `run.sh`, `commit.sh`).
    *   Cơ chế sao lưu dữ liệu tự động (Database Backup) và dọn dẹp file thừa (File Cleanup).

---

## Hướng dẫn nhanh cho AI khi nhận nhiệm vụ mới (AI Prompt & Context Guideline)

Mỗi lần thực hiện nâng cấp hoặc phát triển một tính năng mới trong dự án này, AI **phải tuân thủ** các bước sau:
1.  **Đọc lớp Domain đầu tiên**: Truy cập [domain_layer.md](file:///d:/projects/zlearn/docs/domain_layer.md) để xác định xem thực thể nghiệp vụ đã có chưa. Nếu cần thêm/sửa Entity, định nghĩa nó trước.
2.  **Đọc lớp Application**: Truy cập [application_layer.md](file:///d:/projects/zlearn/docs/application_layer.md) để biết cách tạo Command/Query/Event. Sử dụng các file script `.bat` tại thư mục gốc để sinh code mẫu thay vì viết chay từ đầu.
3.  **Đọc lớp Infrastructure**: Truy cập [infrastructure_layer.md](file:///d:/projects/zlearn/docs/infrastructure_layer.md) nếu cần tương tác database, đăng ký Repository mới, hoặc tương tác với Redis, SignalR, Hosted Services.
4.  **Tuân thủ các quy tắc cốt lõi của dự án**:
    *   Không viết các đoạn điều kiện/cấu hình fallback dư thừa, nếu lỗi thì throw exception rõ ràng.
    *   Dùng các Id dạng chuỗi sinh bởi `IdGenerator.Generate("[PREFIX]")`.
    *   Không tự ý viết các câu truy vấn phức tạp hoặc raw SQL nếu Repository đã hỗ trợ phương thức cơ bản.
