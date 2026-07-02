# Thiết kế Lớp Nghiệp vụ Lõi (Domain Layer)

Lớp **Domain** chứa toàn bộ các thực thể nghiệp vụ cốt lõi, quy tắc logic nghiệp vụ không đổi và các sự kiện nghiệp vụ (Domain Events). Đây là thành phần cô lập nhất, nằm ở trung tâm kiến trúc và không có bất kỳ phụ thuộc nào bên ngoài.

Thư mục chính: [Zlearn.V2.Domain](file:///d:/projects/zlearn/Zlearn.V2.Domain)

---

## Cấu trúc Thiết kế Cơ bản (Base Classes & Domain Events)

Tất cả các thực thể (Entities) trong hệ thống đều kế thừa trực tiếp hoặc gián tiếp từ các lớp cơ sở nằm tại thư mục [Common/](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common):

### 1. [BaseEntity](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common/BaseEntity.cs)
Mọi đối tượng định danh trong database đều kế thừa lớp này:
- `Id` (string, `MaxLength(16)`): Định danh duy nhất của thực thể. Sử dụng ID dạng chuỗi ngắn gọn được sinh ngẫu nhiên theo tiền tố nghiệp vụ qua `IdGenerator` (ví dụ: `QUI...`, `QUE...`).

### 2. [AuditableEntity](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common/AuditableEntity.cs)
Kế thừa từ `BaseEntity`, bổ sung thông tin lịch sử tạo/sửa đổi dữ liệu:
- `CreatedAt` (`DateTimeOffset`): Thời điểm tạo thực thể.
- `CreatedBy` (`string`): Mã tài khoản người tạo.
- `LastModifiedAt` (`DateTimeOffset?`): Thời điểm cập nhật cuối cùng.
- `ModifiedBy` (`string?`): Mã tài khoản người cập nhật cuối cùng.
- *Lưu ý: Các trường này được tự động cập nhật bởi `AuditableEntityInterceptor` ở lớp Infrastructure.*

### 3. [AggregateRoot](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common/AggregateRoot.cs)
Kế thừa từ `AuditableEntity`. Đại diện cho một Aggregate Root trong thiết kế DDD:
- `UncommittedEvents` (`IReadOnlyCollection<DomainEvent>`): Danh sách các sự kiện nghiệp vụ chưa được commit.
- Phương thức: `RaiseEvent(DomainEvent @event)` để đăng ký sự kiện và `ClearUncommittedEvents()` để dọn dẹp sau khi đã dispatch.

### 4. [DomainEvent](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common/DomainEvent.cs) & [IDomainEvent](file:///d:/projects/zlearn/Zlearn.V2.Domain/Common/IDomainEvent.cs)
Đóng vai trò là Base Class cho mọi sự kiện xảy ra trong Domain. Lớp này kế thừa `IDomainEvent` và qua đó kế thừa `INotification` của **MediatR**, giúp hệ thống tự động dispatch thông qua MediatR Handlers.

---

## Danh sách Bounded Contexts và các Thực thể (Entities)

Lớp Domain được chia tách rõ ràng thành các Bounded Contexts:

### 1. CatalogContext (Quản lý danh mục & Bộ đề)
Nằm tại thư mục [CatalogContext/](file:///d:/projects/zlearn/Zlearn.V2.Domain/CatalogContext):
- **Category** (`Categories/Category.cs`): Phân loại các bộ đề trắc nghiệm (Quiz).
  - `Name` (string): Tên danh mục.
  - `Slug` (string): Nhãn thân thiện đường dẫn (URL).
  - `ParentId` (string?): Id của danh mục cha (hỗ trợ phân cấp cây).
- **Quiz** (`Quizzes/Quiz.cs`): Đại diện cho một bộ đề trắc nghiệm, kế thừa từ `AggregateRoot`.
  - `Name` (string): Tên bộ đề.
  - `Slug` (string): Nhãn thân thiện đường dẫn.
  - `CategoryId` (string): Mã danh mục liên kết.
  - `DownloadCount` (int): Số lượt tải đề.
  - `IsPublic` (bool): Đề thi công khai hay nội bộ.
  - Phương thức: `IncDownloadCount()` để nâng sự kiện `QuizDownloadedEvent`.
- **Question** (`Questions/Question.cs`): Đại diện cho câu hỏi trong một bộ đề Quiz, kế thừa từ `AggregateRoot`.
  - `QuizId` (string): Bộ đề chứa câu hỏi này.
  - `Order` (int): Thứ tự câu hỏi trong đề.
  - `Slug` (string): Nhãn được sinh tự động dựa trên nội dung câu hỏi.
  - `StringContent` (string?): Nội dung văn bản câu hỏi.
  - `CorrectKey` (int): Mã key của đáp án đúng.
  - `Explanation` (string?): Giải thích đáp án chi tiết.
  - `AttemptCount` (int): Số lượt học sinh làm câu hỏi này.
  - Phương thức: `IncAttemptCount()` nâng sự kiện `QuestionAttemptedEvent`.
- **Answer** (`Answers/Answer.cs`): Đáp án lựa chọn cho câu hỏi.
  - `QuestionId` (string): Câu hỏi sở hữu đáp án này.
  - `Key` (int): Giá trị định danh đáp án (ví dụ: 0, 1, 2, 3 tương ứng với A, B, C, D).
  - `StringContent` (string?): Văn bản hiển thị đáp án.
- **Tag** (`Tags/Tag.cs`): Thẻ gắn thẻ phân loại Quiz.

### 2. ExamContext (Tổ chức kỳ thi & Làm bài)
Nằm tại thư mục [ExamContext/](file:///d:/projects/zlearn/Zlearn.V2.Domain/ExamContext):
- **Exam** (`Exams/Exam.cs`): Kỳ thi/phòng thi cụ thể được khởi tạo từ một bộ đề `Quiz`.
  - `Name` (string): Tên kỳ thi.
  - `Alias` (string): Mã bí danh dùng cho đường dẫn URL vào phòng thi trực tuyến.
  - `JoinPass` (string?): Mật khẩu tham gia kỳ thi.
  - `LockAccess` (bool): Khóa không cho phép người tham gia mới vào phòng.
  - `ShowAnswerAndKey` (bool): Cho phép xem lại đáp án và kết quả sau khi nộp bài.
  - `StartTime` (`DateTimeOffset`): Thời điểm bắt đầu kỳ thi.
  - `EndTime` (`DateTimeOffset?`): Thời điểm kết thúc kỳ thi.
  - `Status` (`ExamStatus` enum): Trạng thái phòng thi (`Pending`, `Running`, `Finished`).
  - `QuizId` (string): Bộ đề được sử dụng để thi.
- **ExamParticipant** (`Participants/ExamParticipant.cs`): Thông tin và kết quả làm bài của một thí sinh trong kỳ thi.
  - `ExamId` (string): Kỳ thi tham gia.
  - `UserId` (string): Mã tài khoản người thi.
  - `ParticipantCode` (string?): Mã số định danh (ví dụ: MSSV).
  - `ParticipantName` (string): Tên thí sinh.
  - `Status` (`ParticipantStatus` enum): Trạng thái của thí sinh (`Joining`, `Submitted`, `Timeout`).
  - `Score` (double): Điểm số đạt được (thang điểm 10).
  - `SelectedAnswers` (string): Dữ liệu lưu vết các đáp án đã chọn.

### 3. FileContext (Tài nguyên đa phương tiện)
Nằm tại thư mục [FileContext/](file:///d:/projects/zlearn/Zlearn.V2.Domain/FileContext):
- **MediaFile** (`MediaFiles/MediaFile.cs`): Quản lý hình ảnh, âm thanh, video được tải lên hệ thống.
  - `Url` (string): Link CDN lưu trữ tệp tin.
  - `PublicId` (string): Khóa định danh của file trên Cloudinary (dùng để xóa).
  - `IsUsing` (bool): Đánh dấu file có đang được dùng hay không để dọn dẹp file rác.

### 4. IdentityContext (Quản lý danh tính)
Nằm tại thư mục [IdentityContext/](file:///d:/projects/zlearn/Zlearn.V2.Domain/IdentityContext):
- **AppIdentityUser** (`AppIdentityUser.cs`): Tài khoản người dùng hệ thống.
- **AppIdentityRole** (`AppIdentityRole.cs`): Phân quyền hệ thống.

---

## Các Enum Quan trọng

Tài liệu tham chiếu các enum phục vụ luồng xử lý:
- `ExamStatus`: `Pending` (0), `Running` (1), `Finished` (2).
- `ParticipantStatus`: `Joining` (0), `Submitted` (1), `Timeout` (2).
