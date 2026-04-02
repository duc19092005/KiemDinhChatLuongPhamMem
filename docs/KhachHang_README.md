# Test Khách Hàng — Hướng dẫn

## Cấu trúc thư mục

```
Dam_Bao_Chat_Luong/
├── Models/KhachHang/
│   ├── KhachHangTestCaseModel.cs   # Model test case (multi-step, multi-row)
│   └── KhachHangTestResult.cs      # Model kết quả test (per-step results)
├── Services/KhachHang/
│   ├── KhachHangExcelReaderService.cs   # Đọc test cases từ Google Spreadsheet
│   ├── KhachHangExcelWriterService.cs   # Ghi kết quả + merge cells + upload ảnh
│   └── KhachHangSeleniumService.cs      # Selenium test runner (11 test cases)

Dam_Bao_Chat_Luong.Tests/KhachHang/
├── DangKyTests.cs           # II.1_DK_01, II.1_DK_02
├── ChuyenHuongTests.cs      # II.2_CH_01
├── TimKiemTests.cs           # II.3_TK_01, II.3_TK_02, II.3_TK_03
├── DatVeTests.cs             # II.4_CG_01, II.4_CG_02, II.4_TT_01, II.4_TT_02
├── QuanLyTaiKhoanTests.cs   # II.5_LS_01, II.5_CN_01, II.5_ECN_01
├── FlowDatVeTests.cs        # II.6_FLOW_01, II.6_FLOW_02 (E2E Flow đặt vé A→Z)
└── README.md                 # File này
```

## Cách chạy test

```bash
# Chạy tất cả test khách hàng
dotnet test --filter "TestCategory=KhachHang" --logger "console;verbosity=detailed"

# Chạy riêng từng nhóm
dotnet test --filter "TestCategory=DangKy"
dotnet test --filter "TestCategory=ChuyenHuong"
dotnet test --filter "TestCategory=TimKiem"
dotnet test --filter "TestCategory=DatVe"
dotnet test --filter "TestCategory=QuanLy"
dotnet test --filter "TestCategory=FlowDatVe"
dotnet test --filter "TestCategory=E2E"

# Chạy 1 test case cụ thể
dotnet test --filter "FullyQualifiedName~Test_II1_DK01"
```

## Test Cases được implement

| TC ID | Mô tả | Test File |
|---|---|---|
| II.1_DK_01 | Đăng ký thành công | DangKyTests.cs |
| II.1_DK_02 | Đăng ký trùng email | DangKyTests.cs |
| II.2_CH_01 | Chuyển hướng sau đăng nhập | ChuyenHuongTests.cs |
| II.3_TK_01 | Tìm kiếm hợp lệ | TimKiemTests.cs |
| II.3_TK_02 | Tìm kiếm ngày quá khứ | TimKiemTests.cs |
| II.3_TK_03 | Không có chuyến xe | TimKiemTests.cs |
| II.4_CG_01 | Chọn ghế + tính tiền | DatVeTests.cs |
| II.4_CG_02 | Ghế đã bán | DatVeTests.cs |
| II.4_TT_01 | Auto-fill thông tin | DatVeTests.cs |
| II.4_TT_02 | Thanh toán Momo | DatVeTests.cs |
| II.5_LS_01 | Lịch sử đặt vé | QuanLyTaiKhoanTests.cs |
| II.5_CN_01 | Xem thông tin cá nhân | QuanLyTaiKhoanTests.cs |
| II.5_ECN_01 | Chỉnh sửa thông tin | QuanLyTaiKhoanTests.cs |
| **II.6_FLOW_01** | **Flow E2E: Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán → Đăng xuất** | **FlowDatVeTests.cs** |
| II.6_FLOW_02 | Flow E2E từ spreadsheet (nếu có) | FlowDatVeTests.cs |

## Test Cases SKIP (không khả thi tự động)

| TC ID | Lý do skip |
|---|---|
| II.4_CG_03 | Concurrency test — cần 2 browser đồng thời, quá phức tạp |
| II.4_TO_01 | Timeout 15 phút — không thể đợi thật |

## Flow E2E — Chi tiết 8 bước

| Step | Hành động | Expected Result |
|---|---|---|
| 1 | Truy cập trang chủ | Hiển thị danh sách chuyến xe |
| 2 | Đăng nhập | Thành công, hiển thị tên user |
| 3 | Chọn chuyến xe → vào sơ đồ ghế | Hiển thị sơ đồ ghế |
| 4 | Chọn ghế trống | Ghế đổi màu, tổng tiền cập nhật |
| 5 | Chọn thanh toán MoMo | Chọn xong phương thức |
| 6 | Bấm Tiếp tục | Chuyển sang trang MoMo (giả lập) |
| 7 | Thanh toán thành công | Booking Success |
| 8 | Đăng xuất | Quay về trang Login |

## Ghi chú quan trọng

- **Test data thay thế**: Nếu test data từ spreadsheet không khả dụng (email đã tồn tại, ngày quá khứ), hệ thống tự thay data mới và ghi lại lên spreadsheet cột H (Test Data).
- **Screenshots**: Tự động upload lên Google Drive và hiển thị qua `=IMAGE()` trong cột Notes.
- **Merge cells**: Cột Status (K) và Notes (L) được auto-merge cho mỗi test case.
- **Customer account**: `duc19092005d@gmail.com` / `anhduc9a5`
- **Flow E2E timeout**: 5 phút (300 giây) — do flow cần chạy qua nhiều trang và chờ load.
