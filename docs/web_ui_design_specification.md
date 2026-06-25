# Đặc tả Thiết kế Giao diện Web (Web UI Design Specification)

Tài liệu này đặc tả chi tiết ngôn ngữ thiết kế (Design Language) hiện tại của hệ thống giao diện Web thuộc dự án **ZLearn** (`ZLearn.Web`). Tất cả các thay đổi hoặc tính năng giao diện mới trên Web bắt buộc phải tuân theo tài liệu đặc tả này.

---

## 🎨 1. Hệ thống Màu sắc (Color Tokens)

Hệ thống sử dụng các mã màu chủ đạo được định nghĩa trong biến CSS `:root` tại [site.css](file:///d:/projects/zlearn/ZLearn.Web/wwwroot/css/site.css):

| Token | Tên Biến CSS | Mã Màu (HEX/RGB) | Vai trò & Ứng dụng |
| :--- | :--- | :--- | :--- |
| **Primary Deep Teal** | `--main-color` | `#095a71` | Màu thương hiệu chính. Dùng cho Header, Footer, nút chính (Primary Button), các Tab được chọn (Selected Tabs). |
| **Secondary Bright Blue** | `--secondary-color` | `#00B0F0` | Màu nhấn (Accent). Dùng cho viền câu trả lời đã chọn, Info tags, và hiệu ứng hover phát sáng. |
| **Body Background** | `--bg-color` | `#F6F6F6` | Màu nền trang. Tạo cảm giác nhẹ nhàng, dễ chịu khi đọc văn bản thi cử. |
| **Success Green** | `--success-color` | `#00e87f` | Màu trạng thái thành công. Dùng cho Success tags, câu trả lời đúng, thông báo thành công. |
| **Error Red** | `--error-color` | `#f00000` | Màu trạng thái lỗi/cảnh báo. Dùng cho Error tags, countdown đếm ngược thời gian sắp hết, nút xóa/hủy. |

---

## ✍️ 2. Hệ thống Font chữ (Typography)

*   **Font Family mặc định (`--main-font-family`)**: `-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif` (hệ font sans-serif hệ điều hành để đảm bảo tốc độ tải trang tối đa).
*   **Font Weights**:
    *   `fw-light` (300): Dùng cho mô tả phụ, thông tin meta (số lượt làm, danh mục).
    *   `fw-normal` (400): Dùng cho nội dung câu hỏi, văn bản chính.
    *   `fw-semibold` (600) / `fw-bold` (700): Dùng cho tiêu đề lớn, số lượng đề thi, đếm ngược thời gian.

---

## 🏢 3. Cấu trúc Layout chung (Main Layout Structure)

Hệ thống layout của ZLearn Web được định nghĩa thống nhất tại [_Layout.cshtml](file:///d:/projects/zlearn/ZLearn.Web/Views/Shared/_Layout.cshtml), chia làm các phần chính sau:

### 3.1. Thanh thanh tiêu đề phía trên (Topbar / Header)
*   **Màu nền:** `var(--main-color)` (Deep Teal).
*   **Avatar người dùng (`#avt`):** Kích thước tròn `36x36px` (`rounded-pill object-fit-cover`).
*   **Tên hiển thị (`#userName`):** Chữ mảnh trắng (`fw-light text-white text-nowrap`), giới hạn chiều rộng `120px` kèm dấu ba chấm (`text-overflow: ellipsis`).
*   **Nhãn phân quyền (`#roles`):** Chữ in đậm màu vàng sáng (`fw-bold`, font size `11px`, màu `var(--secondary-color)`).
*   **Menu thả xuống (Dropdown actions):** Menu hình chữ nhật sắc cạnh (`rounded-0`), hỗ trợ các hành động nhanh.
*   **Dialog Đăng nhập (`#login-dialog`):** Overlay phủ toàn màn hình màu xám mờ (`vw-100 vh-100 bg-secondary bg-opacity-50`, `z-index: 1`) kết hợp box trắng ở giữa chứa logo và nút Google Sign-In.

### 3.2. Thanh điều hướng chính (Navbar)
*   **Màu nền:** Trắng (`bg-white`) kết hợp đổ bóng nhẹ phía dưới (`shadow-sm`).
*   **Các Tab điều hướng (`.nav-tab`):** Chữ in đậm (`fw-bold`), căn giữa.
    *   *Trạng thái thường:* Mờ nhẹ (`opacity: 0.6`). Hover vào sẽ sáng rõ (`opacity: 1`) và đổi sang màu xanh chính `var(--main-color)`.
    *   *Trạng thái được chọn (`.nav-tab.selected`):* Sáng rõ (`opacity: 1`), đổi màu chữ và có đường viền gạch chân dày `3px` cùng màu (`border-bottom: 3px solid var(--main-color)`).

### 3.3. Khu vực Nội dung chính (Main Content Area)
*   Tất cả nội dung được bao bọc trong thẻ `<section>` chứa container có khoảng cách lề trên dưới nhẹ và chiều cao tối thiểu bằng toàn bộ màn hình để tránh lỗi layout footer dâng lên:
    `<div class="my-3" style="min-height: 100vh;"> @RenderBody() </div>`

### 3.4. Nút cuộn nhanh lên đầu trang (Scroll To Top Bubble - `#bubble`)
*   Vị trí cố định góc dưới bên phải (`position-fixed end-0 bottom-0 m-4 shadow-lg`).
*   Kích thước tròn `50x50px` (`rounded-pill`), nền màu `var(--main-color)`, chứa icon chevron đi lên màu trắng.

### 3.5. Chân trang (Footer)
*   **Màu nền:** `var(--main-color)` (Deep Teal).
*   **Cấu trúc Grid:** Chia cột đều bằng Bootstrap `row g-3`:
    *   Cột 1 (`col-4`): Chứa logo, mô tả ngắn dạng chữ mảnh (`fw-light`) và danh sách mạng xã hội.
    *   Cột 2 (`col-4`): Danh sách liên kết nội bộ (`d-flex flex-column`, chữ trắng mảnh `.text-light .fw-light`).
    *   Cột 3 (`col-4`): Form đăng ký bản tin qua email và thông tin liên hệ.
    *   Cột bản quyền (`col-12`): Căn giữa, có đường kẻ phân cách phía trên (`text-center border-top pt-2`).

---

## ⚡ 4. Các Trạng thái Tương tác (Interactive States & Hover Effects)

Giao diện của ZLearn rất sống động nhờ vào các lớp hiệu ứng hover đặc trưng:

1.  **Opacity Hover (`.opacity-hover`)**:
    *   *Hiệu ứng:* Giảm opacity về `0.6` và đổi con trỏ chuột thành `pointer`.
    *   *Sử dụng:* Cho các icon phụ, nút đóng, nút menu dạng text không viền.
2.  **Highlight Hover (`.highlight-hover`)**:
    *   *Hiệu ứng:* Tạo bóng đổ màu xanh sáng (`box-shadow: 0px 0px 5px var(--secondary-color) !important`) và tăng độ mờ của nội dung bên trong lên 1.
    *   *Sử dụng:* Cho các thẻ danh mục (category card), nút hành động chính, các ô câu hỏi/đề thi gợi ý.
3.  **Standard Shadow Hover (`.hover-shadow`)**:
    *   *Hiệu ứng:* Tạo bóng đổ nhẹ (`box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075)`).
    *   *Sử dụng:* Cho các liên kết danh sách hoặc bài viết dạng block.

---

## 🧱 5. Các Thành phần Giao diện chuẩn (UI Components)

### 5.1. Thẻ thông tin (Cards)
*   Sử dụng cấu trúc Card của Bootstrap 5 kết hợp shadow nhẹ: `.card .shadow-sm .mb-3`.
*   Header của Card nên có nền trắng để tạo cảm giác phẳng, hiện đại: `.card-header .bg-white`.

### 5.2. Hệ thống Nút (Buttons)
*   **Nút Phẳng viền xám (`Rounded-0 Button`):** 
    *   *Cấu trúc:* `.btn .bg-white .border .rounded-0 .shadow-sm .highlight-hover`.
    *   *Đặc trưng:* Không bo góc (`rounded-0`), nền trắng, bóng đổ nhẹ, khi hover sẽ phát sáng màu xanh accent.
*   **Nút Hành động (Action Buttons):** Sử dụng các class của Bootstrap nhưng đồng bộ icon FontAwesome:
    *   Làm bài ngay: `.btn-primary` (màu Main color) + `<i class="fa-solid fa-play me-2"></i>`.
    *   Tạo đề kiểm tra: `.btn-outline-success` + `<i class="fa-solid fa-clipboard-check me-2"></i>`.

### 5.3. Các Thẻ Trạng thái (Tags)
Sử dụng cấu trúc tag bo góc `5px`, viền dày `2px`, chữ đậm vừa `500` và nền có độ trong suốt cao (opacity ~20-30%):
*   **Info Tag (`.info-tag`):** Chữ và viền màu xanh dương --secondary-color, nền màu xanh dương nhạt trong suốt.
*   **Success Tag (`.success-tag`):** Chữ và viền màu xanh lá --success-color, nền xanh lá nhạt trong suốt.
*   **Error Tag (`.error-tag`):** Chữ và viền màu đỏ --error-color, nền đỏ nhạt trong suốt.
*   **Gray Tag (`.gray-tag`):** Chữ và viền màu xám, nền xám nhạt trong suốt.

### 5.4. Thanh điều hướng (Breadcrumbs)
*   Giao diện sử dụng thành phần Breadcrumb chuẩn thông qua `ViewBag.Breadcrumbs` truyền từ Controller dưới dạng danh sách `List<(string Text, string Url)>`.
*   Hiển thị trong một thẻ `<ol class="breadcrumb mb-0">` nằm trong container có padding nhẹ (`py-2 mt-3`).

---

### 5.5. Hệ thống Phân trang (Pagination System)

Hệ thống ZLearn sử dụng 2 cơ chế phân trang tùy thuộc vào ngữ cảnh sử dụng để tối ưu hóa trải nghiệm người dùng:

### 5.5.1. Phân trang truyền thống (Standard Pagination)
*   **Ngữ cảnh sử dụng:** Sử dụng trong các trang quản trị (Admin/Moderator Dashboard), bảng danh sách người dùng, danh sách License Keys, hoặc các bảng dữ liệu dạng lưới lớn nơi người dùng cần biết chính xác tổng số lượng trang và cần khả năng nhảy trực tiếp tới một trang bất kỳ.
*   **Thiết kế & Layout:**
    *   Sử dụng thẻ `<nav>` chứa danh sách liên kết phân trang `.pagination` của Bootstrap 5.
    *   Các nút bấm phân trang bắt buộc phải dùng class `.rounded-0` để giữ góc vuông phẳng, đồng bộ thiết kế chung.
    *   Nút của trang hiện tại (active) có nền màu Deep Teal `var(--main-color)` và chữ màu trắng. Các nút khác có nền trắng, viền xám nhẹ.

### 5.5.2. Tải thêm / Cuộn vô hạn (Infinity Scroll / "Xem thêm" Button)
*   **Ngữ cảnh sử dụng:** Sử dụng cho các danh sách nội dung mang tính khám phá hoặc bảng tin (Feed) như: danh sách đề thi trắc nghiệm công khai ở Trang chủ, danh sách câu hỏi trong đề, phần bình luận (Comments) hoặc danh sách bài viết gợi ý.
*   **Thiết kế & Layout:**
    *   Tránh tự động tải khi cuộn chuột sát đáy trang (auto infinite scroll) để người dùng không bị mất kiểm soát khu vực Chân trang (Footer) và nâng cao hiệu năng. Thay vào đó, sử dụng một nút bấm **"Xem thêm"** ở dưới cùng danh sách.
    *   Nút "Xem thêm" được thiết kế dạng nút phẳng viền xám: `.btn .bg-white .border .rounded-0 .shadow-sm .highlight-hover` kèm biểu tượng mũi tên chỉ xuống hoặc icon tải trang (ví dụ: `<i class="fa-solid fa-chevron-down me-2"></i>` hoặc `<i class="fa-solid fa-spinner me-2"></i>`).
    *   Khi người dùng nhấp nút "Xem thêm", hệ thống sẽ kích hoạt spinner loading nhẹ và tiến hành gọi API để tải dữ liệu trang tiếp theo và nối tiếp (append) vào danh sách hiện tại mà không làm tải lại trang.

---

## ⚙️ 6. Cơ chế UI Logic & Thông báo (Toast/Loading/API)

Để đảm bảo logic hoạt động đồng bộ và không phá vỡ quy tắc UI, phía Web UI sử dụng các hàm Javascript dùng chung được định nghĩa tại [site.js](file:///d:/projects/zlearn/ZLearn.Web/wwwroot/js/site.js):

### 6.1. Hệ thống Thông báo Toast (`showMess`)
*   **Hàm sử dụng:** `showMess(msg, isSuccess)`
*   **Đặc điểm giao diện:**
    *   Tự động sinh ra banner thông báo nằm chính giữa phía trên màn hình (`position-fixed start-50 top-0 translate-middle-x`).
    *   Sử dụng icon FontAwesome tương ứng trạng thái: `fa-circle-check` (màu xanh lá) cho thành công, và `fa-triangle-exclamation` (màu đỏ) cho thất bại.
    *   Hiệu ứng slide xuống (`slideDown`) khi xuất hiện, hiển thị trong **3 giây** và tự động slide lên (`slideUp`) rồi xóa khỏi DOM.
*   *Quy tắc:* Tuyệt đối không sử dụng hàm `alert()` mặc định của trình duyệt hoặc các thư viện toast bên thứ ba để đảm bảo trải nghiệm thống nhất.

### 6.2. Hộp thoại Xác nhận (`showConfirm`)
*   **Hàm sử dụng:** `showConfirm(msg, callback)`
*   **Đặc điểm giao diện:** Tạo một overlay xám mờ phủ toàn màn hình, hiển thị hộp thoại xác nhận ở giữa với nút "Xác nhận" (màu chính) và "Hủy" (màu đỏ).
*   *Quy tắc:* Thay thế hoàn toàn cho hàm `confirm()` của trình duyệt.

### 6.3. Trạng thái Loading toàn trang (`showLoading`/`hideLoading`)
*   **Hàm sử dụng:** `showLoading()`, `hideLoading()`
*   **Đặc điểm:** Bật/tắt overlay `#loader-container` để chặn các thao tác trùng lặp từ người dùng khi hệ thống đang xử lý dữ liệu.

### 6.4. Giao tiếp API chuẩn hóa
Hệ thống đóng gói các hàm Ajax để quản lý trạng thái và lỗi tập trung:
*   `getData(url, callback)`: Gọi API GET, tự động quản lý trạng thái loading.
*   `postJsonData(url, data, callback)`: Gọi API POST với dữ liệu JSON.
*   `putJsonData(url, data, callback)`: Gọi API PUT với dữ liệu JSON.
*   *Luồng bảo mật tự động:* Các hàm này khi nhận được HTTP `401` từ API sẽ tự động kích hoạt `showLoginDialog()`, nhận `403` sẽ xóa session và redirect về `/forbidden` để ngăn chặn rò rỉ thông tin.

