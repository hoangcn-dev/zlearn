# Tài liệu Vận hành & Triển khai (DevOps & Deployment)

Tài liệu này hướng dẫn cách cấu hình Docker, cách chạy và triển khai hệ thống ZLearn lên môi trường máy chủ (Production) bằng các công cụ tự động hóa đi kèm dự án.

Thư mục chứa các file scripts: [Thư mục gốc của project](file:///d:/projects/zlearn)

---

## 1. Dockerization

### [Dockerfile](file:///d:/projects/zlearn/Dockerfile)
Dự án sử dụng cơ chế **Multi-stage build** để tối ưu hóa kích thước Image chạy cuối cùng:
1.  **Stage 1: Build (`mcr.microsoft.com/dotnet/sdk:8.0`)**
    *   Copy mã nguồn vào container, khôi phục dependencies (`dotnet restore`) và publish ứng dụng ra thư mục đầu ra dưới cấu hình Release (`dotnet publish -c Release -o out`).
2.  **Stage 2: Runtime (`mcr.microsoft.com/dotnet/aspnet:8.0`)**
    *   Thiết lập múi giờ Việt Nam (`TZ=Asia/Ho_Chi_Minh`) và cập nhật đồng hồ hệ thống của container.
    *   Cài đặt **PostgreSQL Client (version 16)** từ kho lưu trữ chính thức của PostgreSQL để phục vụ chức năng sao lưu dữ liệu tự động (`pg_dump`).
    *   Copy mã nguồn đã build từ Stage 1 sang và chạy ứng dụng thông qua lệnh `dotnet ZLearn.Web.dll`.
    *   Mở cổng (Expose) 80 cho container.

### [docker-compose.example.yaml](file:///d:/projects/zlearn/docker-compose.example.yaml)
Cung cấp mẫu khai báo các service chạy trong Docker:
*   **Web Service** (`zlearn_web`): Chạy image web, kết nối cổng ngoài `<out-port>` với cổng 80 của container.
*   **Volumes**: Gắn kết thư mục `/hoangcn/backup` trên máy chủ vật lý với thư mục `/app/backup` trong container để lưu trữ các tệp tin backup PostgreSQL được sinh ra hàng ngày.
*   **Mạng chia sẻ** (`shared-network`): Container tham gia vào một mạng chung cùng với hai service phụ thuộc là cơ sở dữ liệu (`pgdb`) và dịch vụ bộ nhớ đệm (`redis`).
*   **Các biến môi trường bắt buộc (Environment Variables)**:
    *   `ADMIN_PASSWORD`: Mật khẩu khởi tạo tài khoản quản trị hệ thống.
    *   `ASPNETCORE_ENVIRONMENT`: Chế độ môi trường chạy ứng dụng (`Production`).
    *   `POSTGRESQL_CONNECTION_STRING`: Chuỗi kết nối tới Postgres DB (`pgdb`).
    *   `JWT_SECRET_KEY`: Khóa bí mật dùng để ký và xác thực JWT token.
    *   `REDIS_CONNECTION_PASSWORD`: Mật khẩu truy cập Redis cache.
    *   `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET`: Khóa API Google Client phục vụ Google OAuth.
    *   `CLOUDINARY_NAME` / `CLOUDINARY_API_KEY` / `CLOUDINARY_API_SECRET`: Thông tin xác thực dịch vụ lưu trữ ảnh Cloudinary.

---

## 2. Quy trình và Công cụ Triển khai (Deployment Workflows)

Dự án hỗ trợ hai phương án triển khai tự động từ môi trường phát triển cục bộ lên máy chủ Production (`hoangcn.com`):

### Phương án A: Triển khai trực tiếp qua Docker Hub
*   Sử dụng file **[build.bat](file:///d:/projects/zlearn/build.bat)**.
*   **Nguyên lý hoạt động**:
    1.  Dừng và xóa bỏ container cũ (`zlearn_web`) và xóa các image cũ trên máy phát triển cục bộ.
    2.  Build Image mới gắn nhãn `zlearn_web:1.0`.
    3.  Gắn tag lại theo tài khoản Docker Hub: `hoangcndev/zlearn:zlearn_web-1.0`.
    4.  Đẩy trực tiếp Image lên Docker Hub registry (`docker push`).
    5.  Trên máy chủ, chỉ cần chạy lệnh pull image mới về và restart lại container.

### Phương án B: Triển khai đóng gói file Tar (Khuyên dùng khi mạng nội bộ bị giới hạn)
*   Sử dụng kết hợp **[deploy.bat](file:///d:/projects/zlearn/deploy.bat)** (ở máy khách) và các file script shell (ở máy chủ).
*   **Các bước tự động của deploy.bat**:
    1.  Xóa image `learn` cũ cục bộ.
    2.  Build lại mã nguồn thành image docker mới mang tên `learn`.
    3.  Đóng gói image vừa build thành một file nén độc lập `new.tar` (`docker save -o new.tar learn:latest`).
    4.  Sử dụng giao thức bảo mật `scp` để tải file `new.tar` lên máy chủ `hoangcn.com` tại đường dẫn `/hoangcn/learn`.
    5.  Tự động đăng nhập SSH vào máy chủ, truy cập thư mục đích và chạy file thực thi kích hoạt **[run.sh](file:///d:/projects/zlearn/run.sh)** trên server.

*   **Kịch bản chạy trên máy chủ ([run.sh](file:///d:/projects/zlearn/run.sh))**:
    ```bash
    docker compose down     # Dừng cụm container cũ
    docker rmi learn        # Xóa image learn cũ khỏi docker engine của máy chủ
    docker load -i new.tar  # Giải nén và nạp image learn mới từ file new.tar
    docker compose up -d    # Khởi động lại toàn bộ cụm container ở chế độ nền
    ```

*   **Xoay vòng và sao lưu phiên bản cũ ([commit.sh](file:///d:/projects/zlearn/commit.sh))**:
    Script này chạy trên server để thực hiện xoay vòng lưu trữ các bản tar đề phòng sự cố cần Rollback:
    1.  Xóa file `backup.tar` (phiên bản cũ thứ hai).
    2.  Đổi tên `cur.tar` (phiên bản đang chạy trước đó) thành `backup.tar`.
    3.  Đổi tên file `new.tar` vừa tải lên thành `cur.tar` (phiên bản hiện tại).

---

## 3. Các tác vụ bảo trì tự động chạy trong Container

### Tự động Sao lưu Cơ sở dữ liệu (Database Backup)
*   Chạy thông qua một hosted service nền: `DatabaseBackupService`.
*   Cấu hình trong mục `Backup` của file `appsettings.json`.
*   Định kỳ chạy lệnh `pg_dump` sao lưu dữ liệu ra file `.sql` nén, lưu vào thư mục `/app/backup` (thư mục này đã được mount ra ổ đĩa máy chủ vật lý qua docker-compose).

### Tự động dọn dẹp file rác (File Cleanup)
*   Chạy thông qua hosted service nền: `RemoveUnusedFilesService`.
*   Định kỳ quét trong cơ sở dữ liệu các file tải lên chưa từng được sử dụng (ví dụ: người dùng chọn ảnh nhưng sau đó hủy không tạo câu hỏi/đề thi), sau đó gọi Cloudinary API để xóa tệp vật lý trên cloud và xóa bản ghi database tương ứng.
