# Thiết kế Lớp Ứng dụng (Application Layer)

Lớp **Application** định nghĩa các luồng nghiệp vụ cụ thể (Use Cases) của hệ thống. Nó điều phối các Entity trong Domain thông qua các Repositories và Interfaces để hoàn thành các yêu cầu từ phía người dùng.

Thư mục chính: [Zlearn.V2.Application](file:///d:/projects/zlearn/Zlearn.V2.Application)

---

## Kiến trúc CQRS & MediatR

Lớp Application áp dụng mẫu thiết kế **CQRS (Command Query Responsibility Segregation)** thông qua thư viện **MediatR**. Yêu cầu thay đổi trạng thái (Ghi/Xóa/Sửa) và yêu cầu lấy dữ liệu (Đọc) được tách biệt rõ ràng:

- **Commands (Lệnh)**: Đại diện cho hành động thay đổi dữ liệu (tạo mới, cập nhật, xóa). Thường có dạng `[Action][Entity]Command` (ví dụ: `CreateExamCommand`). Các thay đổi được thực hiện qua **IWriteRepo<TEntity>** trên cơ sở dữ liệu PostgreSQL.
- **Queries (Truy vấn)**: Đại diện cho hành động lấy dữ liệu mà không làm thay đổi trạng thái hệ thống. Thường có dạng `[Get/List][Entity]Query` (ví dụ: `ExportQuizzesQuery`). Các truy vấn này đọc trực tiếp từ **IReadRepo<TDocument>** trỏ tới cơ sở dữ liệu MongoDB để tối ưu tốc độ đọc.
- **Handlers (Bộ xử lý)**: Mỗi Command và Query đều đi kèm với một Handler tương ứng chịu trách nhiệm thực thi logic nghiệp vụ.

### Cấu trúc thư mục của một Module Nghiệp vụ
Các chức năng nghiệp vụ được nhóm theo thực thể hoặc context (như `Quizzes`, `Exams`, `Categories`, `Files`, `Identity`). Trong mỗi thực thể, cấu trúc gồm:
- `Commands/`: Chứa các thư mục nhỏ tương ứng với từng hành động viết (ví dụ: `CreateQuiz`, `ChangeExamStatus`), mỗi thư mục chứa:
  - `*Command.cs`: Định nghĩa dữ liệu đầu vào.
  - `*CommandHandler.cs`: Xử lý logic và lưu trữ thông qua Write Repo.
- `Queries/`: Chứa các thư mục nhỏ tương ứng với từng hành động đọc (ví dụ: `GetExamScore`, `ExportQuizzes`), mỗi thư mục chứa:
  - `*Query.cs`: Định nghĩa tham số truy vấn.
  - `*QueryHandler.cs`: Xử lý truy vấn thông qua Read Repo của MongoDB.
- `DTOs/`: Chứa các đối tượng vận chuyển dữ liệu (Data Transfer Objects) phục vụ riêng cho thực thể đó.

---

## Cơ chế Pipeline Behaviors & Validation tự động

Dự án sử dụng tính năng **Pipeline Behavior** của MediatR để tự động can thiệp vào vòng đời xử lý của một Request trước khi nó tới được Handler.

### [ValidationBehaviour](file:///d:/projects/zlearn/Zlearn.V2.Application/Common/Behavours/ValidationBehaviour.cs)
Được đăng ký trong [DependencyInjection.cs](file:///d:/projects/zlearn/Zlearn.V2.Application/DependencyInjection.cs):
- Khi có bất kỳ Request nào được gửi qua `_mediator.Send()`, pipeline sẽ tự động tìm kiếm tất cả các class validator kế thừa `AbstractValidator<TRequest>` (của thư viện **FluentValidation**).
- Nếu phát hiện lỗi dữ liệu không hợp lệ, nó sẽ ném ra `ValidationException` chứa danh sách các lỗi kiểm thử đầu vào.
- Giúp loại bỏ code kiểm tra thủ công trong Controller hoặc Handler, đảm bảo Handler luôn nhận được dữ liệu sạch và đúng định dạng.

---

## Công cụ Sinh mã tự động (Scaffolding Scripts)

Để đẩy nhanh tốc độ code và đảm bảo tính đồng nhất trong thiết kế CQRS, dự án cung cấp các file script batch ở thư mục gốc giúp tự động sinh cấu trúc code mẫu:

### 1. [create-command.bat](file:///d:/projects/zlearn/create-command.bat)
- **Mục đích**: Sinh nhanh Command và CommandHandler tương ứng.
- **Cách chạy**: Mở Terminal tại thư mục gốc và chạy:
  ```powershell
  .\create-command.bat [TênCommand] [KiểuDữLiệuTrảVề]
  # Ví dụ:
  .\create-command.bat CreateQuiz CreateResponseDto
  ```

### 2. [create-query.bat](file:///d:/projects/zlearn/create-query.bat)
- **Mục đích**: Sinh nhanh Query và QueryHandler tương ứng.
- **Cách chạy**:
  ```powershell
  .\create-query.bat [TênQuery] [KiểuDữLiệuTrảVề]
  # Ví dụ:
  .\create-query.bat GetQuizById QuizDetailDto
  ```

### 3. [create-event.bat](file:///d:/projects/zlearn/create-event.bat)
- **Mục đích**: Sinh nhanh Event và EventHandler tương ứng để xử lý Domain Events bất đồng bộ.
- **Cách chạy**:
  ```powershell
  .\create-event.bat [TênEvent]
  ```

---

## Ánh xạ Dữ liệu (AutoMapper)

Dự án đăng ký AutoMapper tự động thông qua `services.AddAutoMapper(Assembly.GetExecutingAssembly())`. Các Profile ánh xạ được khai báo bằng cách thừa kế lớp `Profile` của AutoMapper trong lớp Application, hỗ trợ ánh xạ tự động từ Domain Entities/Documents sang DTO trả về cho client.
