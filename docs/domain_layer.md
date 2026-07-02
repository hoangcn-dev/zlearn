# Thiết kế Lớp Nghiệp vụ Lõi (Domain Layer)

Lớp **Domain** chứa toàn bộ các thực thể nghiệp vụ cốt lõi, quy tắc logic nghiệp vụ không đổi và các sự kiện nghiệp vụ (Domain Events). Đây là thành phần cô lập nhất, nằm ở trung tâm kiến trúc và không có bất kỳ phụ thuộc nào bên ngoài.

Thư mục chính: [ZLearn.Domain](file:///d:/projects/zlearn/ZLearn.Domain)

---

## 🏛️ Cấu trúc Thiết kế Cơ bản (Base Classes & Domain Events)

Tất cả thực thể trong hệ thống đều thừa kế trực tiếp hoặc gián tiếp từ lớp cơ sở dưới đây:

### 1. [BaseEntity](file:///d:/projects/zlearn/ZLearn.Domain/Common/BaseEntity.cs)
Mọi đối tượng định danh trong database đều kế thừa lớp này:
*   `Id` (string, `MaxLength(16)`): Định danh chính của thực thể. Sử dụng ID dạng chuỗi ngắn gọn được sinh ngẫu nhiên theo tiền tố nghiệp vụ thông qua `IdGenerator` (ví dụ: `QUI123456...`).
*   `Events` (`IReadOnlyCollection<BaseEvent>`): Danh sách các sự kiện nghiệp vụ phát sinh trên thực thể này trước khi lưu vào DB.
*   Phương thức quản lý sự kiện: `AddEvent()`, `RemoveEvent()`, `ClearEvents()`.

### 2. [AuditableEntity](file:///d:/projects/zlearn/ZLearn.Domain/Common/AuditableEntity.cs)
Kế thừa từ `BaseEntity`, bổ sung thông tin lịch sử chỉnh sửa dữ liệu:
*   `CreatedAt` (`DateTimeOffset`): Thời điểm tạo thực thể.
*   `CreatedBy` (`string`): Định danh người tạo.
*   `LastModifiedAt` (`DateTimeOffset?`): Thời điểm cập nhật cuối cùng.
*   `ModifiedBy` (`string?`): Người cập nhật cuối cùng.
*   *Lưu ý: Các trường này được tự động cập nhật bởi `AuditableEntityInterceptor` ở lớp Infrastructure.*

### 3. [BaseEvent](file:///d:/projects/zlearn/ZLearn.Domain/Common/BaseEvent.cs)
Đóng vai trò là Base Class cho mọi sự kiện xảy ra trong Domain. Lớp này kế thừa `INotification` của **MediatR**, giúp nó dễ dàng được lắng nghe bởi các `INotificationHandler` trong dự án.

---

## 💾 Danh sách và Chi tiết các Thực thể (Entities)

### 1. [Category](file:///d:/projects/zlearn/ZLearn.Domain/Entities/Category.cs)
Phân loại các bộ đề trắc nghiệm (Quiz).
*   `Name` (string): Tên danh mục.
*   `Slug` (string): Nhãn thân thiện đường dẫn (URL).
*   `ParentId` (string?): Id của danh mục cha (hỗ trợ phân cấp nhiều tầng).
*   *Liên kết*: Chứa danh sách các `Quiz` và danh sách các `Exam` liên thuộc.

### 2. [Quiz](file:///d:/projects/zlearn/ZLearn.Domain/Entities/Quiz.cs)
Đại diện cho một bộ đề trắc nghiệm chứa câu hỏi và đáp án.
*   `Name` (string): Tên bộ đề.
*   `Slug` (string): Nhãn thân thiện đường dẫn.
*   `CategoryId` (string): Mã danh mục liên kết.
*   `DownloadCount` (int): Số lượt tải đề.
*   `IsPublic` (bool): Đánh dấu đề thi công khai hay nội bộ.
*   *Liên kết*: `Category` (Danh mục), `Tags` (Thẻ), `Questions` (Danh sách câu hỏi), `Exams` (Danh sách các kỳ thi sử dụng đề này).

### 3. [Question](file:///d:/projects/zlearn/ZLearn.Domain/Entities/Question.cs)
Đại diện cho câu hỏi trong một bộ đề Quiz.
*   `QuizId` (string): Bộ đề chứa câu hỏi này.
*   `Order` (int): Thứ tự câu hỏi trong đề.
*   `Slug` (string): Nhãn được sinh tự động dựa trên nội dung câu hỏi.
*   `StringContent` (string?): Nội dung câu chữ của câu hỏi.
*   `MediaFileUrls` (string): Chứa các đường dẫn ảnh hoặc âm thanh đính kèm (dưới dạng chuỗi phân cách bởi dấu phẩy `,`).
*   `CorrectKey` (int): Key (mã) của đáp án đúng.
*   `Explanation` (string?): Lời giải thích đáp án chi tiết.
*   `AttemptCount` (int): Số lần người học đã thử làm câu hỏi này.
*   *Liên kết*: `Quiz` (Bộ đề liên kết), `Answers` (Danh sách các đáp án lựa chọn).

### 4. [Answer](file:///d:/projects/zlearn/ZLearn.Domain/Entities/Answer.cs)
Đáp án lựa chọn cho câu hỏi.
*   `QuestionId` (string): Câu hỏi sở hữu đáp án này.
*   `Key` (int): Giá trị định danh đáp án (ví dụ: `1` cho A, `2` cho B, `3` cho C, v.v.).
*   `StringContent` (string?): Văn bản hiển thị đáp án.
*   `MediaFileUrls` (string): Đường dẫn đa phương tiện đính kèm.

### 5. [Exam](file:///d:/projects/zlearn/ZLearn.Domain/Entities/Exam.cs)
Một kỳ thi/phòng thi cụ thể được khởi tạo từ một bộ đề `Quiz`.
*   `Name` (string): Tên kỳ thi.
*   `Alias` (string): Mã bí danh dùng cho đường dẫn URL vào phòng thi trực tuyến.
*   `Note` (string?): Ghi chú / Quy chế phòng thi.
*   `JoinPass` (string?): Mật khẩu bắt buộc để tham gia kỳ thi.
*   `LockAccess` (bool): Khóa không cho phép người tham gia mới vào phòng.
*   `ShowAnswerAndKey` (bool): Cho phép xem lại đáp án và kết quả sau khi nộp bài.
*   `MixQuestions` (bool): Đảo thứ tự câu hỏi cho từng người thi.
*   `MixAnswers` (bool): Đảo thứ tự các phương án lựa chọn trong câu hỏi.
*   `RequireJoinWithCode` (bool): Bắt buộc người thi phải nhập mã số học sinh/sinh viên.
*   `RequireJoinWithName` (bool): Bắt buộc khai báo tên.
*   `AllowLateSubmit` (bool): Cho phép nộp muộn (khi quá giờ quy định).
*   `StartTime` (`DateTimeOffset`): Thời điểm bắt đầu kỳ thi.
*   `EndTime` (`DateTimeOffset?`): Thời điểm kết thúc kỳ thi.
*   `StartJobId` (string?): Mã Job lập lịch mở phòng thi (Quartz/Hangfire).
*   `EndJobId` (string?): Mã Job lập lịch đóng phòng thi.
*   `Status` (`ExamStatus` enum): Trạng thái phòng thi (`Pending`, `Running`, `Finished`).
*   `QuizId` (string): Bộ đề được sử dụng để thi.
*   `MaxParticipants` (int): Số lượng người tham gia tối đa.
*   *Liên kết*: `Quiz` (Bộ đề), `Participants` (Danh sách những người tham gia thi).

### 6. [ExamParticipant](file:///d:/projects/zlearn/ZLearn.Domain/Entities/ExamParticipant.cs)
Bảng ghi nhận thông tin và kết quả làm bài của một thí sinh trong kỳ thi.
*   `ExamId` (string): Kỳ thi tham gia.
*   `UserId` (string): Mã tài khoản người thi.
*   `ParticipantCode` (string?): Mã số định danh tự khai báo (ví dụ: MSSV).
*   `ParticipantName` (string): Tên hiển thị tự khai báo.
*   `FirstCheckIn` (`DateTimeOffset?`): Thời gian bắt đầu vào làm bài.
*   `LastCheckOut` (`DateTimeOffset?`): Thời gian nộp bài hoặc rời khỏi phòng thi.
*   `Status` (`ParticipantStatus` enum): Trạng thái của thí sinh (`Joining`, `Submitted`, `Timeout`).
*   `Correct` (int): Số câu trả lời đúng.
*   `Completed` (int): Số câu đã hoàn thành.
*   `IsBanned` (bool): Đánh dấu thí sinh bị cấm thi (do gian lận, vi phạm quy chế).
*   `Score` (double): Điểm số đạt được.
*   `SelectedAnswers` (string): Chuỗi JSON hoặc định dạng đặc biệt lưu vết các đáp án đã chọn.

### 7. [MediaFile](file:///d:/projects/zlearn/ZLearn.Domain/Entities/MediaFile.cs)
Quản lý các tệp tin hình ảnh, âm thanh được tải lên hệ thống.
*   `FileName` (string): Tên file gốc.
*   `Extension` (string): Phần mở rộng của file.
*   `Url` (string): Link CDN lưu trữ tệp tin (thường là Cloudinary).
*   `PublicId` (string): Khóa định danh của file trên Cloudinary (dùng để xóa).
*   `BytesSize` (long): Kích thước file.
*   `IsUsing` (bool): Đánh dấu file có đang được nhúng vào đề thi/câu hỏi nào không.
*   `UserId` (string): Người tải tệp lên.

### 8. [LicenseKey](file:///d:/projects/zlearn/ZLearn.Domain/Entities/LicenseKey.cs)
Quản lý bản quyền công cụ bổ trợ (ví dụ: `FlowVeoAutoTool`).
*   `Name` (string): Tên giấy phép.
*   `Type` (`LicenseKeyType` enum): Loại công cụ (`FlowVeoAutoTool`).
*   `Status` (`LicenseKeyStatus` enum): Trạng thái khóa (`Pending`, `Active`, `Revoked`).
*   `Level` (`LicenseKeyLevel` enum): Cấp độ khóa (`Normal`, `Pro`).
*   `Key` (string): Chuỗi khóa bản quyền duy nhất.
*   `LifeDays` (int): Số ngày sử dụng được cấp phép.
*   `HardwareId` (string?): Mã định danh phần cứng máy khách đã kích hoạt khóa này.

### 9. [AccessHistory](file:///d:/projects/zlearn/ZLearn.Domain/Entities/AccessHistory.cs)
Lịch sử thống kê lượt truy cập toàn trang theo ngày.
*   `AccessDate` (`DateTimeOffset`): Ngày thống kê.
*   `Count` (int): Số lượt truy cập tích lũy trong ngày.

---

## 🏷️ Các Enum Quan trọng

Tài liệu tham chiếu các enum phục vụ luồng xử lý:
*   `ExamStatus`: `Pending` (0), `Running` (1), `Finished` (2).
*   `ParticipantStatus`: `Joining` (0), `Submitted` (1), `Timeout` (2).
*   `LicenseKeyStatus`: `Pending` (0), `Active` (1), `Revoked` (2).
*   `LicenseKeyLevel`: `Normal` (0), `Pro` (1).
