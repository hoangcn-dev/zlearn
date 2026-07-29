# Báo Cáo Thử Nghiệm Hiệu Năng: Có Cache vs Không Cache

Báo cáo này trình bày kết quả so sánh hiệu năng giữa hai luồng API lấy thông tin đề thi khi bắt đầu làm bài (dữ liệu được join từ 4-5 bảng chính: `Exams`, `Quizzes`, `Questions`, `Answers`, `Categories`).

Thử nghiệm được thực hiện bằng cách giả lập **500 người dùng đồng thời (VUs)** truy vấn liên tục trong vòng **2 phút (120 giây)**.

---

## 1. Kịch Bản Thử Nghiệm (Test Scenarios)

Cả hai kịch bản đều giả lập cùng một mức tải:
- **Số lượng User đồng thời (VUs)**: 500
- **Thời gian chạy (Duration)**: 2 phút
- **Khoảng thời gian nghỉ giữa các request (Think time)**: 100ms (`sleep(0.1)`)
- **Môi trường chạy**: Local Development (tải ứng dụng được build dưới cấu hình `Release` để tối ưu hóa biên dịch của .NET).

### Kịch bản 1: Không có Cache (`exam-sim-nocache.js`)
- **API Endpoint**: `GET http://127.0.0.1:5087/api/exams/sim-nocache?alias=danh-gia-nang-luc-toan-hoc`
- **Mô tả**: Mỗi yêu cầu từ người dùng sẽ đi thẳng vào cơ sở dữ liệu PostgreSQL thực hiện câu truy vấn JOIN 5 bảng để trả về toàn bộ thông tin đề thi, câu hỏi và các lựa chọn đáp án tương ứng.

### Kịch bản 2: Có sử dụng Cache 2 lớp (`exam-sim-cached.js`)
- **API Endpoint**: `GET http://127.0.0.1:5087/api/exams/sim-cached?alias=danh-gia-nang-luc-toan-hoc`
- **Mô tả**: Yêu cầu đi qua 2 lớp cache bảo vệ:
  - **Lớp 1 (Memory Cache)**: Kiểm tra bộ nhớ đệm RAM trong tiến trình (TTL: 2 phút). Nếu trúng (Hit) trả về ngay lập tức.
  - **Lớp 2 (Redis Cache)**: Nếu Memory Cache trượt (Miss), truy cập Redis đệm ngoài (TTL: 5 phút). Nếu Hit sẽ cập nhật lại Memory Cache và trả về.
  - **Database Fallback**: Chỉ truy cập PostgreSQL khi cả 2 lớp đệm đều trượt, sau đó lưu kết quả vào Memory Cache & Redis.

---

## 2. Kết Quả Thử Nghiệm Chi Tiết

Dưới đây là bảng so sánh trực quan các thông số đo lường thực tế từ công cụ `k6`:

| Thông số đo lường | KHÔNG CÓ CACHE (PostgreSQL JOIN) | CÓ CACHE 2 LỚP (Memory Cache + Redis) | So sánh & Cải thiện |
| :--- | :---: | :---: | :---: |
| **Tổng số Requests hoàn tất** | 18,002 | **526,660** | **Tăng ~29.2 lần** (Gần 3000%) |
| **Thông lượng (Throughput)** | 145.01 req/s | **4,384.74 req/s** | **Tăng ~30.2 lần** |
| **Tỷ lệ thành công (HTTP 200)** | 99.51% (Lỗi 88 requests do quá tải DB) | **100.00%** (0 lỗi) | **Tuyệt đối ổn định** |
| **Thời gian phản hồi trung bình (Avg Latency)** | 3,260 ms (3.26 giây) | **9.93 ms** | **Giảm 328 lần** |
| **Thời gian phản hồi trung vị (Median - 50%)** | 2,510 ms (2.51 giây) | **4.99 ms** | **Giảm 503 lần** |
| **Phân vị 90% (p90 Latency)** | 5,580 ms (5.58 giây) | **22.73 ms** | **Giảm 245 lần** |
| **Phân vị 95% (p95 Latency)** | 6,320 ms (6.32 giây) | **32.41 ms** | **Giảm 195 lần** |
| **Thời gian phản hồi tối đa (Max Latency)** | 16,270 ms (16.27 giây) | **500.78 ms** | **Giảm 32.5 lần** |
| **Băng thông mạng nhận (Network Received)** | 912 kB/s | **28 MB/s** | **Đạt giới hạn phần cứng** |

---

## 3. Phân Tích & Đánh Giá

### Ưu điểm vượt trội khi sử dụng Caches:
1. **Giải phóng hoàn toàn tải cơ sở dữ liệu (PostgreSQL)**:
   Khi không có cache, việc 500 VUs liên tục truy cập thực hiện truy vấn JOIN nặng khiến CPU của PostgreSQL luôn ở trạng thái 100%, gây ra hiện tượng nghẽn cổ chai (bottleneck) dẫn đến thời gian phản hồi bị kéo dài lên tới vài giây, và xuất hiện 88 yêu cầu bị lỗi do quá thời gian kết nối (timeouts).
   Với luồng có cache, PostgreSQL chỉ chịu tải **đúng 1 lần duy nhất** cho yêu cầu đầu tiên. Toàn bộ 526,659 yêu cầu sau đó được phục vụ trực tiếp từ bộ nhớ RAM siêu tốc.

2. **Cải thiện trải nghiệm người dùng (Latency giảm sâu)**:
   Thời gian phản hồi trung bình giảm từ **3.26 giây xuống còn 9.93 mili-giây** (gần như tức thì). Điều này đảm bảo khi hàng nghìn học sinh cùng lúc nhấn vào nút "Bắt đầu làm bài", trang thông tin đề thi sẽ hiển thị ngay lập tức mà không gặp bất kỳ hiện trạng xoay vòng chờ đợi nào.

3. **Khả năng chịu tải và mở rộng hệ thống cực kỳ mạnh mẽ**:
   Hệ thống có khả năng xử lý thông lượng lên tới **hơn 4,380 yêu cầu/giây** trên một cấu hình phần cứng thông thường, điều mà cơ sở dữ liệu truyền thống nếu không có cache sẽ không bao giờ đạt được nếu không nâng cấp phần cứng (Scale-up) cực kỳ tốn kém.

---

## 4. Kết Luận

Giải pháp sử dụng **Cache 2 lớp (In-Memory + Distributed Redis)** là bắt buộc và tối ưu nhất đối với các API có tần suất đọc cực lớn và dữ liệu tương đối ít thay đổi như thông tin đề thi. Nó đem lại sự ổn định tuyệt đối (100% thành công) và tốc độ phản hồi tối đa dưới tải lớn.
