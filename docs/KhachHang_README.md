# Hướng dẫn Khởi chạy & Kiến trúc Automation Test - Phân hệ Khách Hàng

Tài liệu này hướng dẫn cách cấu hình, kiến trúc và lệnh thực thi chuẩn xác cho các bài test tự động (Selenium) thuộc phân hệ Khách Hàng trong hệ thống Đặt Vé Xe.

---

## 1. Cấu trúc mã nguồn

Hệ thống Automation Test của Khách Hàng được thiết kế theo mô hình tách biệt dịch vụ (Service-Pattern) để dễ dàng bảo trì và tích hợp với Google Sheets:

```text
Dam_Bao_Chat_Luong/
├── Models/KhachHang/
│   ├── KhachHangTestCaseModel.cs   # Model map với cấu trúc dòng/cột Google Spreadsheet
│   └── KhachHangTestResult.cs      # Model kết quả test của từng bộ Step
├── Services/KhachHang/
│   ├── KhachHangExcelReaderService.cs # Đọc kịch bản đầu vào từ Google Sheets
│   ├── KhachHangExcelWriterService.cs # Cập nhật kết quả chi tiết kèm Screenshot (Google Drive)
│   ├── KhachHangSeleniumService.cs    # Chứa logic Selenium thao tác Automation & Fallback JS
│   └── KhachHangTestCaseAppender.cs   # [MỚI] Tự động append test cases mới lên Spreadsheet
```

```text
Dam_Bao_Chat_Luong.Tests/KhachHang/
├── DangKyTests.cs           # Test đăng ký mới, mật khẩu không khớp, bỏ trống (II.1)
├── DangNhapGuiTests.cs      # [MỚI] Test GUI Đăng nhập: UI, Thiếu field, Sai MK (II.1)
├── ChuyenHuongTests.cs      # Test chuyển hướng login an toàn (II.2)
├── TimKiemTests.cs          # Test logic bộ lọc tìm kiếm (II.3)
├── DatVeTests.cs            # Test chọn ghế, tự động điền info, thanh toán (II.4)
├── QuanLyTaiKhoanTests.cs   # Lịch sử, Đổi MK, Đăng xuất, Sửa thông tin (II.5)
├── NavigationTests.cs       # [MỚI] Test Navbar và điều hướng trang (II.6)
├── FlowDatVeTests.cs        # Mô phỏng liên hoàn E2E 9 BƯỚC (II.6)
└── SpreadsheetSetupTests.cs # [MỚI] Helper để khởi tạo test cases trên Sheets
```

---

## 2. Danh sách Kịch bản (Test Cases) mở rộng

Dưới đây là danh sách các kịch bản quan trọng mới được bổ sung và cập nhật:

### 2.1. Nhóm Đăng ký & Đăng nhập (Mở rộng)
| Requirement ID | Test Case ID | Hàm Service | Mô tả kịch bản | Nơi thực thi |
|---|---|---|---|---|
| II.1_DangKy | II.1_DK_03 | `Test_DK03` | Đăng ký thất bại: Mật khẩu không khớp | DangKyTests.cs |
| II.1_DangKy | II.1_DK_04 | `Test_DK04` | Đăng ký thất bại: Bỏ trống tất cả fields | DangKyTests.cs |
| II.1_DangNhap | II.1_LG_01 | `Test_LG01` | Kiểm tra giao diện form Đăng nhập | DangNhapGuiTests.cs |
| II.1_DangNhap | II.1_LG_02 | `Test_LG02` | Đăng nhập để trống Email | DangNhapGuiTests.cs |
| II.1_DangNhap | II.1_LG_03 | `Test_LG03` | Đăng nhập để trống Mật khẩu | DangNhapGuiTests.cs |
| II.1_DangNhap | II.1_LG_04 | `Test_LG04` | Đăng nhập sai mật khẩu | DangNhapGuiTests.cs |

### 2.2. Nhóm Navigation & Quản lý
| Requirement ID | Test Case ID | Hàm Service | Mô tả kịch bản | Nơi thực thi |
|---|---|---|---|---|
| II.6_Navigation | II.6_NAV_01 | `Test_NAV01` | Kiểm tra Navbar hiển thị đúng sau login | NavigationTests.cs |
| II.6_Navigation | II.6_NAV_02 | `Test_NAV02` | Điều hướng đến tất cả trang từ Navbar | NavigationTests.cs |
| II.6_Navigation | II.6_NAV_03 | `Test_NAV03` | Kiểm tra trang Về chúng tôi | NavigationTests.cs |
| II.6_Navigation | II.6_NAV_04 | `Test_NAV04` | Kiểm tra trang Lịch trình | NavigationTests.cs |
| II.5_QuanLy | II.5_MK_01 | `Test_MK01` | Kiểm tra form đổi mật khẩu hiển thị | QuanLyTaiKhoanTests.cs |
| II.5_QuanLy | II.5_MK_02 | `Test_MK02` | Đổi MK: Nhập sai mật khẩu cũ | QuanLyTaiKhoanTests.cs |
| II.5_QuanLy | II.5_DX_01 | `Test_DX01` | Đăng xuất thành công | QuanLyTaiKhoanTests.cs |

### 2.3. Flow Đặt Vé E2E (9 Bước)
Đã nâng cấp từ 8 bước lên **9 bước**, thêm bước xác thực quan trọng tại trang Lịch sử.
- **Hành động**: Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán MoMo → **Kiểm tra lịch sử vé** → Đăng xuất.
- **Mã test**: `II.6_FLOW_01` thực thi trong `FlowDatVeTests.cs`.

---

## 3. Câu lệnh Terminal thực thi kiểm thử

Mở Terminal tại thư mục Root để chạy các lệnh chuyên biệt sau:

```bash
# [QUAN TRỌNG] Bước 0: Tạo/Cập nhật test cases lên Spreadsheet (Chạy 1 lần)
dotnet test --filter "TestCategory=Setup"

# 1. Chạy TẤT CẢ các kịch bản Khách Hàng (bao gồm E2E)
dotnet test --filter "TestCategory=KhachHang" --logger "console;verbosity=detailed"

# 2. Chạy tất cả TRỪ luồng E2E 9 bước
dotnet test --filter "TestCategory=KhachHang&TestCategory!=E2E" --logger "console;verbosity=detailed"

# 3. CHỈ chạy luồng E2E 9 bước (Mô phỏng xuyên suốt)
dotnet test --filter "TestCategory=E2E" --logger "console;verbosity=detailed"

# 4. Chạy theo nhóm GUI mới
dotnet test --filter "TestCategory=GUI"
```

---

## 4. Ghi chú cốt lõi (Core Implementations)

- **Cơ chế xác thực Lịch sử (Step 8 E2E)**: Sau khi thanh toán, hệ thống tự động điều hướng vào `/Auth/History` để tìm kiếm mã vé hoặc thông tin chuyến xe vừa đặt, đảm bảo dữ liệu đã được lưu thành công vào Database.
- **Khởi tạo Spreadsheet tự động**: `KhachHangTestCaseAppender` cho phép developer thêm kịch bản test mới vào code và tự động "đẩy" chúng lên Google Sheets mà không cần thao tác tay, tránh lỗi sai ID hoặc sai định dạng.
- **Cơ chế Vượt chặn Ghế bằng JS**: Vẫn được duy trì để xử lý các UI phức tạp trên trang Chọn Ghế khi Selenium click thông thường bị chặn.
- **Quản lý phiên (Session Management)**: Luôn kết thúc bằng hành động Logout (`Test_DX01`) để đảm bảo các bài test sau không bị dính session của bài test trước.
