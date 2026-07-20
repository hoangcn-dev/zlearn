# Thiết kế Lớp Hiển thị (Presentation Layer)

Lớp **Presentation** đóng vai trò là giao diện tương tác với người dùng và hệ thống bên ngoài thông qua ứng dụng Web MVC và các REST API.

---

## Web Application & API - [ZLearn.Web](file:///d:/projects/zlearn/ZLearn.Web)

Project **ZLearn.Web** là một ứng dụng ASP.NET Core MVC (Model-View-Controller) tích hợp. Nó phục vụ giao diện Web cho người dùng ôn thi trực tuyến và đóng vai trò API Host chính (bao gồm cả SignalR Realtime và toàn bộ các REST API endpoints phục vụ hệ thống).

### Controllers & Views (Razor Views)
- `HomeController`: Phục vụ trang chủ, trang tĩnh của website.
- `ExamsController`: Điều phối giao diện làm bài thi trực tuyến, hiển thị bộ đếm thời gian ngược, và gửi đáp án qua SignalR hoặc HTTP POST.
- `QuestionsController` & `QuizzesController`: Các trang xem thông tin đề thi và câu hỏi.

### API Controllers (Thư mục [Controllers/API](file:///d:/projects/zlearn/ZLearn.Web/Controllers/API))
- `AuthController`: Cung cấp API đăng nhập, đăng ký, đăng xuất, Google Sign-in và làm mới token (Refresh Token).
- `QuizzesController`: Các API tạo, cập nhật, xóa, lấy chi tiết, và phân trang danh sách bộ đề.
- `ExamsController`: Các API tương tác với bài thi.
- `FilesController`: Endpoint phục vụ việc đăng tải tệp tin đa phương tiện lên hệ thống.
- `LicenseKeysController`: API quản lý License Keys.
- `LogsController`: API xem log của hệ thống.
- `SystemController`: Cung cấp thông tin trạng thái hoạt động hệ thống.

### Đặc điểm kỹ thuật quan trọng
- **Forwarded Headers**: Cấu hình `ForwardedHeadersOptions` giúp giải quyết triệt để lỗi chuyển hướng sai URL (`redirect_uri`) khi chạy Google OAuth phía sau máy chủ Reverse Proxy Nginx.
- **Realtime SignalR Host**: Đăng ký và cấu hình các Hub truyền thông trực tiếp như `ExamHub` và `AccessTrackingHub`.
