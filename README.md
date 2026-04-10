# KiemDinhChatLuongPhamMem - Selenium Automation Test

Dự án này là bộ công cụ kiểm thử tự động (Automation Test) sử dụng **C# .NET 8** và **Selenium WebDriver** để kiểm tra toàn bộ luồng nghiệp vụ khách hàng của hệ thống `duck123.runasp.net`. Đặc biệt, dự án tự động sinh báo cáo kết quả kiểm thử ngay trên **Google Sheets** và đính kèm trực tiếp ảnh chụp màn hình (thông qua **Google Drive API**).

## 🚀 Tính Năng Chính
- **Đọc Test Case tự động:** Phân tích dữ liệu JSON và kịch bản test từ Google Spreadsheet.
- **Auto Test với 2 Chế Độ Chạy Linh Hoạt:**
  - *Tùy chọn 1 (Chạy qua Test Service - MSTest):* Thiết kế để chạy tự động toàn bộ kịch bản đăng nhập (thành công, gõ sai mật khẩu, thiếu trường...) bằng lệnh `dotnet test`. Tool tự động chạy, đánh giá kết quả `PASS/FAIL`, chụp màn hình, tự động upload Drive và dán ảnh báo cáo trả về Sheet Google cực kỳ ngon lành. Rất phù hợp để tích hợp vào CI/CD pipeline.
  - *Tùy chọn 2 (Chạy qua Console Menu - Trả về WebDriver):* Rất hữu dụng khi Automation Tester muốn khởi tạo nhanh phiên làm việc. Chạy console app để tự động vượt qua ải "Đăng Nhập", sau đó thì **không tắt Chrome mà trả lại cho bạn luôn cái WebDriver đang ở trạng thái đã đăng nhập**. Nhờ vậy, bạn chỉ việc cầm cái WebDriver đó rồi viết tiếp các hàm `FindElement` để test nghiệp vụ bên trong mà không cần lặp lại bước đăng nhập.
- **Tự động chụp ảnh màn hình (Screenshots):** Chụp lại ngay khoảnh khắc chuyển trang hoặc báo lỗi.
- **Tự động đính kèm ảnh vào Google Sheets:**
  - Tự động tạo thư mục `Dam_Bao_Chat_Luong_Screenshots` trên phần vùng Google Drive của bạn.
  - Upload ảnh báo cáo lỗi lên Drive và gắn quyền truy cập (public reader).
  - Sử dụng API ghi đè hàm `=IMAGE()` lên Sheets để in ảnh trực quan bên trong ô tính.

---

## 📊 Danh Sách Test Cases

### Sheet "Đăng nhập" (Login Tests)
| ID | Mô tả | Category |
|----|--------|----------|
| TC_LOGIN_01 | Đăng nhập Admin hợp lệ | Login, Admin |
| TC_LOGIN_02 | Đăng nhập Staff hợp lệ | Login, Staff |
| TC_LOGIN_03 | Đăng nhập Manager hợp lệ | Login, Manager |
| TC_LOGIN_04 | Đăng nhập sai credentials | Login, Negative |
| TC_LOGIN_05 | Đăng nhập bỏ trống fields | Login, Negative |
| TC_LOGIN_06 | Trang đăng nhập load OK | Login, Smoke |

### Sheet "Khách hàng" (Customer GUI Tests)

| ID | Mô tả | Category |
|----|--------|----------|
| **ĐĂNG KÝ** | | |
| II.1_DK_01 | Đăng ký thành công | KhachHang, DangKy |
| II.1_DK_02 | Đăng ký trùng Email/SĐT | KhachHang, DangKy |
| II.1_DK_03 | Đăng ký mật khẩu không khớp | KhachHang, DangKy, GUI |
| II.1_DK_04 | Đăng ký bỏ trống tất cả | KhachHang, DangKy, GUI |
| **ĐĂNG NHẬP GUI** | | |
| II.1_LG_01 | Trang đăng nhập hiển thị đúng | KhachHang, GUI, DangNhap |
| II.1_LG_02 | Đăng nhập để trống Email | KhachHang, GUI, DangNhap |
| II.1_LG_03 | Đăng nhập để trống Password | KhachHang, GUI, DangNhap |
| II.1_LG_04 | Đăng nhật sai mật khẩu | KhachHang, GUI, DangNhap |
| **CHUYỂN HƯỚNG** | | |
| II.2_CH_01 | Chuyển hướng sau đăng nhập | KhachHang, ChuyenHuong |
| **TÌM KIẾM** | | |
| II.3_TK_01 | Tìm kiếm chuyến xe hợp lệ | KhachHang, TimKiem |
| II.3_TK_02 | Tìm kiếm ngày quá khứ | KhachHang, TimKiem |
| II.3_TK_03 | Không tìm thấy chuyến | KhachHang, TimKiem |
| **ĐẶT VÉ** | | |
| II.4_CG_01 | Chọn ghế tính tiền tự động | KhachHang, DatVe |
| II.4_CG_02 | Không cho chọn ghế đã bán | KhachHang, DatVe |
| II.4_TT_01 | Auto-fill thông tin khách | KhachHang, DatVe |
| II.4_TT_02 | Thanh toán Momo | KhachHang, DatVe |
| **QUẢN LÝ TÀI KHOẢN** | | |
| II.5_LS_01 | Xem lịch sử đặt vé | KhachHang, QuanLy |
| II.5_CN_01 | Xem thông tin cá nhân | KhachHang, QuanLy |
| II.5_ECN_01 | Sửa thông tin cá nhân | KhachHang, QuanLy |
| II.5_MK_01 | Form đổi mật khẩu hiển thị | KhachHang, QuanLy, GUI |
| II.5_MK_02 | Đổi MK sai mật khẩu cũ | KhachHang, QuanLy, GUI |
| II.5_DX_01 | Đăng xuất thành công | KhachHang, QuanLy, GUI |
| **NAVIGATION** | | |
| II.6_NAV_01 | Navbar hiển thị đúng | KhachHang, GUI, Navigation |
| II.6_NAV_02 | Điều hướng tất cả trang | KhachHang, GUI, Navigation |
| II.6_NAV_03 | Trang Về chúng tôi | KhachHang, GUI, Navigation |
| II.6_NAV_04 | Trang Lịch trình | KhachHang, GUI, Navigation |
| **FLOW ĐẶT VÉ E2E** | | |
| II.6_FLOW_01 | Flow đặt vé A→Z (9 steps) | KhachHang, FlowDatVe, E2E |

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

### 🔧 Bước 0: Setup test cases lên Google Spreadsheet (Chạy 1 lần)

Trước khi chạy test lần đầu, bạn cần append các test cases mới lên Google Spreadsheet:

```bash
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=Setup" --logger "console;verbosity=detailed"
```

### 🏃 Cách 1: Chạy TẤT CẢ tests (A → Z) — Bao gồm GUI + Integration + Flow đặt vé E2E

Chạy toàn bộ test cases từ đầu đến cuối, bao gồm cả Flow đặt vé E2E (Integration test lớn).

```bash
dotnet test Dam_Bao_Chat_Luong.Tests --logger "console;verbosity=detailed"
```

### 🏃 Cách 2: Chạy tất cả TRỪ Integration tests (Flow đặt vé E2E)

Chạy tất cả test cases nhỏ (GUI, Login, Đăng ký, Tìm kiếm, Navigation, ...) **KHÔNG** chạy Flow đặt vé E2E.

```bash
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory!=E2E&TestCategory!=Setup" --logger "console;verbosity=detailed"
```

### 🏃 Cách 3: Chạy CHỈ Integration tests (Flow đặt vé E2E)

Chạy riêng Flow đặt vé End-to-End (Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán → Kiểm tra lịch sử → Đăng xuất).

```bash
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=E2E" --logger "console;verbosity=detailed"
```

### 🎯 Chạy theo nhóm tính năng cụ thể

```bash
# Chạy nhóm Login (sheet Đăng nhập):
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=Login"

# Chạy nhóm GUI (tất cả test cases GUI mới):
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=GUI"

# Chạy nhóm KhachHang (tất cả test trên sheet Khách hàng):
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=KhachHang"

# Chạy nhóm Đăng ký:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=DangKy"

# Chạy nhóm Đăng nhập GUI:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=DangNhap"

# Chạy nhóm Navigation:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=Navigation"

# Chạy nhóm Tìm Kiếm:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=TimKiem"

# Chạy nhóm Đặt Vé:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=DatVe"

# Chạy nhóm Quản lý tài khoản:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=QuanLy"

# Chạy nhóm Admin:
dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=Admin"
```

### Cách 4: Khởi tạo WebDriver (Chạy qua Console App)
Phù hợp khi bạn là Automation Tester muốn máy tự động đăng nhập hộ, sau đó nhường lại trình duyệt để bạn code/chạy test các tính năng bên trong hệ thống (như Đặt vé xe).

Mở terminal/powershell và chạy:

```bash
cd Dam_Bao_Chat_Luong/Dam_Bao_Chat_Luong
dotnet run
```
Khi menu hiện lên, hãy chọn Option `2` (Chạy multi luồng). Trình duyệt sẽ được bật, tự động vượt qua form login và trả về cho bạn sử dụng.

### 🔓 Đăng nhập Google ở lần đầu tiên chạy:
- Ở lần chạy đầu tiên, tool sẽ mở 1 tab trình duyệt web yêu cầu bạn **đăng nhập vào Google**.
- Bạn hãy dùng chính Email mà bạn đã add vào "Test users" ở Bước 1.
- Màn hình sẽ cảnh báo "Ứng dụng chưa được xác minh". Nhấn vào `Nâng cao (Advanced)` -> chọn `Đi tới {Tên app} (Không an toàn)`.
- **(QUAN TRỌNG)** Nhớ đánh thẻ V (Tick) vào tất cả các ô hỏi quyền truy cập: **Chỉnh sửa Google Sheets** và **Chỉnh sửa Google Drive**.
- Ấn Continue. Khi thành công, token sẽ được lưu lại (folder `token_store`) nên bạn không cần đăng nhập lại ở các lần duyệt sau.
- Tận hưởng quá trình automation báo cáo lên Sheets!

---

## 📁 Cấu Trúc Dự Án

```
KiemDinhChatLuongPhamMem/
├── Dam_Bao_Chat_Luong/                    # Project chính (Service Layer)
│   ├── Services/
│   │   ├── KhachHang/
│   │   │   ├── KhachHangSeleniumService.cs    # Selenium test cho tất cả GUI
│   │   │   ├── KhachHangExcelReaderService.cs # Đọc test cases từ Google Sheets
│   │   │   ├── KhachHangExcelWriterService.cs # Ghi kết quả lên Google Sheets
│   │   │   └── KhachHangTestCaseAppender.cs   # Append test cases mới lên Google Sheets
│   │   ├── SeleniumTestService.cs             # Selenium test cho Đăng nhập
│   │   └── ThirdPersonServices/
│   │       └── GoogleSpreadSheetService.cs    # Service Google Sheets API
│   ├── Models/
│   │   ├── KhachHang/
│   │   │   ├── KhachHangTestCaseModel.cs      # Model test case khách hàng
│   │   │   └── KhachHangTestResult.cs         # Model kết quả test
│   │   ├── TestCaseModel.cs                   # Model test case đăng nhập
│   │   └── TestResult.cs                      # Model kết quả test đăng nhập
│   └── appsettings.Development.json           # Cấu hình OAuth2 (Git-ignored)
│
├── Dam_Bao_Chat_Luong.Tests/              # Project test (MSTest)
│   ├── LoginTests.cs                          # Tests đăng nhập (sheet Đăng nhập)
│   └── KhachHang/
│       ├── DangKyTests.cs                     # Tests đăng ký (DK_01 → DK_04)
│       ├── DangNhapGuiTests.cs                # Tests GUI đăng nhập (LG_01 → LG_04)
│       ├── ChuyenHuongTests.cs                # Tests chuyển hướng (CH_01)
│       ├── TimKiemTests.cs                    # Tests tìm kiếm (TK_01 → TK_03)
│       ├── DatVeTests.cs                      # Tests đặt vé (CG_01, CG_02, TT_01, TT_02)
│       ├── QuanLyTaiKhoanTests.cs             # Tests quản lý (LS_01, CN_01, ECN_01, MK_01, MK_02, DX_01)
│       ├── NavigationTests.cs                 # Tests navigation (NAV_01 → NAV_04)
│       ├── FlowDatVeTests.cs                  # Tests Flow E2E đặt vé (FLOW_01 - 9 steps)
│       └── SpreadsheetSetupTests.cs           # Append test cases mới lên Google Sheets
│
└── README.md
```

---

## ⁉️ Khắc Phục Lỗi Phổ Biến (Troubleshooting)

**1. Lỗi `Error 403: access_denied` khi đăng nhập Google**
- Giải quyết: Email bạn dùng để đăng nhập chưa được cho vào danh sách "Test users" ở trang OAuth Consent Screen. Vui lòng thêm email vào (Bước 1.4) hoặc Publish ứng dụng bên Google Cloud.

**2. Quá trình Test bị Treo (Hang) không xử lý được các field báo lỗi**
- Giải quyết: Phiên bản UI web đã thay đổi, tool không bắt được các class lỗi client-side. Mở `SeleniumTestService.cs`, tìm hàm `GetErrorMessages()` và sửa CSS Selector cho đúng lại với web hiện tại.

**3. `Warning: Package Downgrade Newtonsoft.Json` khi build**
- Giải quyết: Xung đột khi tải package Google API. Gõ lệnh: `dotnet add package Newtonsoft.Json -v 13.0.3` để đồng bộ thủ công.

**4. Test case không tìm thấy trên Spreadsheet**
- Giải quyết: Chạy lệnh setup trước: `dotnet test Dam_Bao_Chat_Luong.Tests --filter "TestCategory=Setup"`
