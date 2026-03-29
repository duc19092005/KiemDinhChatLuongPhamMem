# KiemDinhChatLuongPhamMem - Selenium Automation Test

Dự án này là bộ công cụ kiểm thử tự động (Automation Test) sử dụng **C# .NET 8** và **Selenium WebDriver** để kiểm tra luồng đăng nhập của hệ thống `duck123.runasp.net`. Đặc biệt, dự án tự động sinh báo cáo kết quả kiểm thử ngay trên **Google Sheets** và đính kèm trực tiếp ảnh chụp màn hình (thông qua **Google Drive API**).

## 🚀 Tính Năng Chính
- **Đọc Test Case tự động:** Phân tích dữ liệu JSON và kịch bản test từ Google Spreadsheet.
- **Auto Test với 2 Chế Độ Chạy Linh Hoạt:**
  - *Tùy chọn 1 (Chạy đơn luồng - Tự động hóa bảng Test Case):* Thiết kế để chạy 1 mạch toàn bộ kịch bản (kể cả test case thành công, gõ sai mật khẩu, nhập thiếu trường...). Tool tự động chạy từ trên xuống dưới, đánh giá kết quả `PASS/FAIL`, tự động chụp màn hình, tự động upload Drive và dán ảnh báo cáo trả về Sheet Google cực kỳ ngon lành.
  - *Tùy chọn 2 (Chạy Multi luồng - Vượt ải Login và trả về WebDriver):* Rất hữu dụng khi Automation Tester muốn viết script C# để test sâu hơn vào các phân hệ bên trong hệ thống (như chức năng Đặt vé, Thêm xe, Quản lý chuyến...). Code sẽ dùng tài khoản trên Sheet để tự động vượt qua ải "Đăng Nhập" nhanh chóng, sau đó thì **không tắt Chrome mà trả lại cho bạn luôn cái WebDriver đang ở trạng thái đã đăng nhập**. Nhờ vậy, bạn chỉ việc cầm cái WebDriver đó rồi viết tiếp các hàm `FindElement` để test nghiệp vụ mà không cần tốn thì giờ tự code lại nguyên cái luồng đăng nhập loằng ngoằng.
- **Tự động chụp ảnh màn hình (Screenshots):** Chụp lại ngay khoảnh khắc chuyển trang hoặc báo lỗi.
- **Tự động đính kèm ảnh vào Google Sheets:**
  - Tự động tạo thư mục `Dam_Bao_Chat_Luong_Screenshots` trên phần vùng Google Drive của bạn.
  - Upload ảnh báo cáo lỗi lên Drive và gắn quyền truy cập (public reader).
  - Sử dụng API ghi đè hàm `=IMAGE()` lên Sheets để in ảnh trực quan bên trong ô tính.

---

## 🛠️ Yêu Cầu Cài Đặt (Prerequisites)
Để chạy được tool, máy tính của bạn cần có:
1. **.NET 8.0 SDK** ([Tải về tại đây](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
2. Trình duyệt **Google Chrome** đời mới nhất.
3. Tài khoản Google (Để setup Google Cloud Console và ghi file).

---

## ⚙️ Hướng Dẫn Setup Cấu Hình (Dành cho Developer/Tester Khác)

Vì dự án liên kết với Google Sheets và Google Drive API, bắt buộc phải thiết lập **OAuth 2.0 Client ID** riêng cho mỗi môi trường chạy gốc.

### Bước 1: Thiết lập Google Cloud Console
1. Truy cập [Google Cloud Console](https://console.cloud.google.com/).
2. Tạo một **Project** mới (hoặc dùng project cũ).
3. Vào phần **APIs & Services > Library**, tìm và gạt nút **ENABLE** cho 2 thư viện:
   - `Google Sheets API`
   - `Google Drive API`
4. Vào **APIs & Services > OAuth consent screen**:
   - Chọn loại **External** rồi tạo.
   - Điền tên App, Email Hỗ trợ (điền tạm).
   - Ở bước **Test users** (RẤT QUAN TRỌNG): Nhấn `+ ADD USERS` và thêm chính địa chỉ gmail cá nhân mà bạn định dùng để chạy test tool.
5. Vào **APIs & Services > Credentials**:
   - Click `+ CREATE CREDENTIALS` -> chọn **OAuth client ID**.
   - Tại mục Application Type chọn **Desktop app** (Ứng dụng dành cho máy tính để bàn).
   - Click Create để tạo. Sao chép thông tin `Client ID` và `Client Secret`.

### Bước 2: Khởi tạo file cấu hình cho dự án
Do lý do bảo mật, file `appsettings.Development.json` sẽ bị ignore trên Git. Bạn phải tự tạo nó thủ công.
Tạo file `appsettings.Development.json` trong thư mục `Dam_Bao_Chat_Luong/Dam_Bao_Chat_Luong/` với nội dung sau:

```json
{
  "Google": {
    "ClientId": "<DÁN_CLIENT_ID_CỦA_BẠN_VÀO_ĐÂY>",
    "ClientSecret": "<DÁN_CLIENT_SECRET_CỦA_BẠN_VÀO_ĐÂY>"
  }
}
```

Dán thông tin bạn vừa copy từ Bước 1 vào là hoàn tất cấu hình!

---

## ▶️ Hướng Dẫn Chạy Test (How to run)

Mở terminal/powershell trỏ vào thư mục codebase (nơi chứa file đuôi `.csproj`) và chạy:

```bash
cd Dam_Bao_Chat_Luong/Dam_Bao_Chat_Luong
dotnet build
dotnet run
```

### 🔓 Đăng nhập Google ở lần đầu tiên chạy:
- Ở lần chạy đầu tiên, tool sẽ mở 1 tab trình duyệt web yêu cầu bạn **đăng nhập vào Google**.
- Bạn hãy dùng chính Email mà bạn đã add vào "Test users" ở Bước 1.
- Màn hình sẽ cảnh báo "Ứng dụng chưa được xác minh". Nhấn vào `Nâng cao (Advanced)` -> chọn `Đi tới {Tên app} (Không an toàn)`.
- **(QUAN TRỌNG)** Nhớ đánh thẻ V (Tick) vào tất cả các ô hỏi quyền truy cập: **Chỉnh sửa Google Sheets** và **Chỉnh sửa Google Drive**.
- Ấn Continue. Khi thành công, token sẽ được lưu lại (folder `token_store`) nên bạn không cần đăng nhập lại ở các lần duyệt sau.
- Tận hưởng quá trình automation báo cáo lên Sheets!

---

## ⁉️ Khắc Phục Lỗi Phổ Biến (Troubleshooting)

**1. Lỗi `Error 403: access_denied` khi đăng nhập Google**
- Giải quyết: Email bạn dùng để đăng nhập chưa được cho vào danh sách "Test users" ở trang OAuth Consent Screen. Vui lòng thêm email vào (Bước 1.4) hoặc Publish ứng dụng bên Google Cloud.

**2. Quá trình Test bị Treo (Hang) không xử lý được các field báo lỗi**
- Giải quyết: Phiên bản UI web đã thay đổi, tool không bắt được các class lỗi client-side. Mở `SeleniumTestService.cs`, tìm hàm `GetErrorMessages()` và sửa CSS Selector cho đúng lại với web hiện tại.

**3. `Warning: Package Downgrade Newtonsoft.Json` khi build**
- Giải quyết: Xung đột khi tải package Google API. Gõ lệnh: `dotnet add package Newtonsoft.Json -v 13.0.3` để đồng bộ thủ công.
