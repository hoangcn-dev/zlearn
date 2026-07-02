# Thiết kế Lớp Hiển thị (Presentation Layer)

Lớp **Presentation** đóng vai trò là giao diện tương tác với thế giới bên ngoài (qua REST API, trang web MVC hoặc ứng dụng desktop quản trị). Hệ thống ZLearn cung cấp 3 đầu mối hiển thị chính.

---

## 🌐 1. Web Application & API - [ZLearn.Web](file:///d:/projects/zlearn/ZLearn.Web)

Project **ZLearn.Web** là một ứng dụng ASP.NET Core MVC (Model-View-Controller) tích hợp. Nó vừa phục vụ giao diện Web cho người dùng ôn thi trực tuyến, vừa đóng vai trò API Host chính (bao gồm cả SignalR Realtime và toàn bộ các REST API endpoints phục vụ cho ứng dụng Desktop/Mobile).

*   **Controllers & Views (Razor Views)**:
    *   `HomeController`: Phục vụ trang chủ, trang tĩnh của website.
    *   `ExamsController`: Điều phối giao diện làm bài thi trực tuyến, hiển thị bộ đếm thời gian ngược, và gửi đáp án qua SignalR hoặc HTTP POST.
    *   `QuestionsController` & `QuizzesController`: Các trang xem thông tin đề thi và câu hỏi.
*   **API Controllers (nằm trong thư mục [Controllers/API](file:///d:/projects/zlearn/ZLearn.Web/Controllers/API))**:
    *   `AuthController`: Cung cấp API đăng nhập, đăng ký, đăng xuất, Google Sign-in và làm mới token (Refresh Token).
    *   `QuizzesController`: Các API tạo, cập nhật, xóa, lấy chi tiết, và phân trang danh sách bộ đề.
    *   `ExamsController`: Các API tương tác với bài thi.
    *   `FilesController`: Endpoint phục vụ việc đăng tải tệp tin đa phương tiện lên hệ thống.
    *   `LicenseKeysController`: API quản lý License Keys.
    *   `LogsController`: API xem log của hệ thống.
    *   `SystemController`: Cung cấp thông tin trạng thái hoạt động hệ thống.
*   **Đặc điểm kỹ thuật quan trọng**:
    *   **Forwarded Headers**: Cấu hình `ForwardedHeadersOptions` giúp giải quyết triệt để lỗi chuyển hướng sai URL (`redirect_uri`) khi chạy Google OAuth phía sau máy chủ Reverse Proxy Nginx (Nginx redirect từ HTTPS sang HTTP).
    *   **Realtime SignalR Host**: Đăng ký và Map các Hub truyền thông trực tiếp như `ExamHub` và `AccessTrackingHub`.

---

## 🖥️ 2. Admin Desktop Application - [ZLearn.AdminDesktopApp](file:///d:/projects/zlearn/ZLearn.AdminDesktopApp)

Ứng dụng quản trị dạng Desktop viết bằng **WPF (.NET 8.0)** dùng để quản lý toàn bộ hệ thống đề thi, danh mục, người dùng, xem log hệ thống và phân tích dữ liệu.

### 🏛️ Mẫu Thiết kế MVVM (Model-View-ViewModel)
Ứng dụng sử dụng gói thư viện **CommunityToolkit.Mvvm** hiện đại để đơn giản hóa việc viết code giao diện:
*   Sử dụng thuộc tính `[ObservableProperty]` để tự động sinh mã thông báo thay đổi giao diện (INotifyPropertyChanged).
*   Sử dụng `[RelayCommand]` thay thế cho việc viết `ICommand` thủ công.

### 🗺️ Cơ chế Điều hướng (Navigation System)
Desktop App sử dụng mô hình điều hướng không đổi trang (Single Window - Multiple Views) thông qua:
*   **NavigationStore**: Lưu trữ ViewModel hiện tại (`CurrentViewModel`) và điểm đến hiện tại (`CurrentDestination`).
*   **MainViewModel**: Lắng nghe sự thay đổi của `NavigationStore` và đổi giao diện hiển thị động trên `MainWindow.xaml` thông qua `DataTemplate` định nghĩa trong `App.xaml`.

### 💾 Các Kho Lưu Trữ Trạng Thái (Stores)
1.  **[VariableStore](file:///d:/projects/zlearn/ZLearn.AdminDesktopApp/Stores/VariableStore.cs)**: Lưu trữ các biến môi trường và thông tin phiên làm việc hiện tại (`AccessToken`, `RefreshToken`, `UserName`, `Role`, `UserId`, `ImageUrl`). Hỗ trợ xóa sạch thông tin khi kết thúc phiên (`EndSession`).
2.  **TaskStatusStore**: Quản lý trạng thái xử lý tác vụ bất đồng bộ (Loading). Khi UI đang gọi API, `Loading` set bằng `true` sẽ tự động hiển thị thanh quay (Busy Indicator) chặn người dùng thao tác trùng lặp.

### 📦 Tổ chức theo Tính năng (Features)
Thay vì gom toàn bộ View/ViewModel vào các thư mục dùng chung, Desktop App gom nhóm các màn hình có cùng ngữ cảnh nghiệp vụ vào thư mục `Features/`:
*   **QuizFeature**:
    *   Quản lý danh mục Quiz (`QuizCateViewModel`, `QuizCateView`).
    *   Quản lý đề thi (`ListQuizViewModel`, `ListQuizView`).
    *   Thống kê bộ đề (`QuizStatViewModel`).
*   **SystemFeature**:
    *   Xem log hệ thống (`ListLogsViewModel`, `ListLogsView`).
    *   Quản lý tài khoản người dùng (`ListUsersViewModel`, `ListUsersView`).
    *   Thống kê hệ thống (`SystemStatViewModel`).
