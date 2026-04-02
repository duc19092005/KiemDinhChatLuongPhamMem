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
│   ├── KhachHangExcelReaderService.cs   # Đọc kịch bản đầu vào từ Google Sheets
│   ├── KhachHangExcelWriterService.cs   # Cập nhật kết quả chi tiết, merge cell, upload hình ảnh báo cáo (Google Drive)
│   └── KhachHangSeleniumService.cs      # Chứa "Bộ não" thao tác Automation (Chứa cả Fallback JS cho Click)

Dam_Bao_Chat_Luong.Tests/KhachHang/
├── DangKyTests.cs           # Test đăng ký mới, trùng email (II.1)
├── ChuyenHuongTests.cs      # Test chuyển hướng login an toàn (II.2)
├── TimKiemTests.cs          # Test logic bộ lọc tìm kiếm (II.3)
├── DatVeTests.cs            # Test chọn ghế, tự động điền info, thanh toán (II.4)
├── QuanLyTaiKhoanTests.cs   # Lịch sử, Xem thông tin, Sửa thông tin cá nhân (II.5)
└── FlowDatVeTests.cs        # Mô phỏng liên hoàn E2E 8 bước thực tế (II.6)
```

> **Lưu ý:** Trước đây dự án dùng file gộp Standalone `Gop_Test_FlowDatVe.cs`, nhưng hiện tại các logic cốt lõi như *Thuật toán Vượt chặn Click Ghế bằng JS* đều đã được hợp nhất chuẩn xác vào `KhachHangSeleniumService.cs` và các file Test Module riêng lẻ phía trên để đảm bảo kiến trúc sạch (Clean Architecture) và tối ưu khả năng bảo trì.

---

## 2. Danh sách Kịch bản (Test Cases)

Toàn bộ 14 hàm Test Case liên quan đã hoàn thành bao gồm:

| Phân hệ | Requirement ID | Test Case ID | Hàm trong Selenium Service | Mô tả kịch bản | Nơi thực thi (Class) |
|---|---|---|---|---|---|
| Đăng ký | II.1_DangKy | II.1_DK_01 | `Test_DK01` | Đăng ký tài khoản khách hàng thành công | DangKyTests.cs |
| Đăng ký | II.1_DangKy | II.1_DK_02 | `Test_DK02` | Đăng ký thất bại do trùng Email/SĐT | DangKyTests.cs |
| Chuyển hướng | II.2_ChuyenHuong | II.2_CH_01 | `Test_CH01` | Chuyển hướng đúng về trang đặt vé sau đăng nhập | ChuyenHuongTests.cs |
| Tìm kiếm | II.3_TimKiem | II.3_TK_01 | `Test_TK01` | Tìm kiếm chuyến xe hợp lệ | TimKiemTests.cs |
| Tìm kiếm | II.3_TimKiem | II.3_TK_02 | `Test_TK02` | Tìm kiếm với ngày đi trong quá khứ | TimKiemTests.cs |
| Tìm kiếm | II.3_TimKiem | II.3_TK_03 | `Test_TK03` | Không tìm thấy chuyến xe (Trả về Empty) | TimKiemTests.cs |
| **Đặt vé** | **II.4_DatVe** | **II.4_CG_01** | `Test_CG01` | **Chọn ghế trên sơ đồ và tính tiền tự động** | DatVeTests.cs |
| **Đặt vé** | **II.4_DatVe** | **II.4_CG_02** | `Test_CG02` | **Không cho phép chọn ghế đã bán** | DatVeTests.cs |
| **Đặt vé** | **II.4_DatVe** | **II.4_TT_01** | `Test_TT01` | **Thông tin hành khách được tự động điền** | DatVeTests.cs |
| **Đặt vé** | **II.4_DatVe** | **II.4_TT_02** | `Test_TT02` | **Đặt vé và Thanh toán Momo thành công** | DatVeTests.cs |
| Tài Khoản | II.5_QuanLyTaiKhoan | II.5_LS_01 | `Test_LS01` | Xem lịch sử đặt vé | QuanLyTaiKhoanTests.cs |
| Tài Khoản | II.5_QuanLyTaiKhoan | II.5_CN_01 | `Test_CN01` | Xem thông tin cá nhân | QuanLyTaiKhoanTests.cs |
| Tài Khoản | II.5_QuanLyTaiKhoan | II.5_ECN01 | `Test_ECN01` | Chỉnh sửa thông tin cá nhân | QuanLyTaiKhoanTests.cs |
| **Flow E2E** | **II.6_FlowDatVe** | **II.6_FLOW_01** | `Test_Flow_DatVe_E2E` | **Toàn trình: Đăng nhập → Chọn chuyến → Chọn ghế → Momo → Đăng xuất** | FlowDatVeTests.cs |

---

## 3. Câu lệnh Terminal thực thi kiểm thử

Mở Terminal tại thư mục Root (nơi chứa file đuôi `.sln`) để chạy các lệnh chuyên biệt sau:

```bash
# 1. Chạy TẤT CẢ các kịch bản của toàn bộ Cụm Khách Hàng (Tầm 14 Test Cases)
dotnet test --filter "TestCategory=KhachHang" --logger "console;verbosity=detailed"

# 2. CHỈ chạy nhánh xử lý Đặt Vé, Chọn Ghế và Thanh toán (rất quan trọng)
dotnet test --filter "TestCategory=DatVe|TestCategory=FlowDatVe" --logger "console;verbosity=detailed"

# 3. CHỈ chạy nhánh các luồng còn lại (Đăng ký, Chuyển hướng, Tìm kiếm, Quản lý tài khoản)
dotnet test --filter "TestCategory=DangKy"
dotnet test --filter "TestCategory=ChuyenHuong"
dotnet test --filter "TestCategory=TimKiem"
dotnet test --filter "TestCategory=QuanLy"

# 4. CHỈ chạy đích danh luồng siêu dài E2E (Mô phỏng y hệt thật - Xuyên suốt vòng đời Booking)
dotnet test --filter "TestCategory=FlowDatVe" --logger "console;verbosity=detailed"
```

---

## 4. Ghi chú cốt lõi (Core Implementations)

- **Cơ chế Vượt chặn Ghế bằng JS (Fallback JS Click)**: Trong bộ `KhachHangSeleniumService.cs`, nếu thuật toán Selenium gặp lỗi che khuất Element (`Element Intercepted Exception`) lúc bấm nút thanh toán hoặc tương tác ghế ngồi, hệ thống sẽ tự động invoke ngược lệnh Browser `IJavaScriptExecutor` để can thiệp ép nhận diện hành động một cách an toàn.
- **Tự động hóa ScreenCapture**: Khi qua từng chặng Test thành công hoặc gặp lỗi, Auto-Screenshot được kích hoạt, lưu vào `/Screenshots/KhachHang` và được Upload thẳng lên folder *Google Drive*, link ảnh `=`IMAGE()` sau chót sẽ rớt xuống ngược lại vào Cột Notes trên Spreadsheet.
- **Tự động Tracking Data Ảo**: Khắc phục "Tài khoản tồn tại" trong DB bằng cách Automation sẽ sinh ngẫu nhiên Random string `yyyyMMdd` gắn vào Prefix `email`. Data ảo được khai sinh này ngay lập tức sẽ được log đè ngược lại cột `Test Data` để tester biết Account nào vừa được sử dụng cho Unit Testing.
