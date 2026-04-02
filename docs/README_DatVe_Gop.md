Tên : Trần Anh Đức 

Test case + Test Data  Intergration : Xử lý đặt vé bao gồm chọn ghế và thanh toán 

Test Script Gop_Test_FlowDatVe.cs 

STT 1  

Hàm trong script : Test_CG01 

Chọn ghế trên sơ đồ và tính tiền tự động 

Test Requirement ID : II.4_DatVe  

Test Case ID : II.4_CG_01 

STT 2 

Hàm trong script : Test_CG02 

Không cho phép chọn ghế đã bán 

Test Requirement ID : II.4_DatVe  

Test Case ID : II.4_CG_02 

STT 3 

Hàm trong script : Test_TT01 

Thông tin hành khách được tự động điền 

Test Requirement ID : II.4_DatVe  

Test Case ID : II.4_TT_01 

STT 4 

Hàm trong script : Test_TT02 

Đặt vé và Thanh toán Momo thành công 

Test Requirement ID : II.4_DatVe  

Test Case ID : II.4_TT_02 

STT 5 

Hàm trong script : Test_Flow_DatVe_E2E 

Flow đặt vé End-to-End: Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán 

Test Requirement ID : II.6_FlowDatVe  

Test Case ID : II.6_FLOW_01 

-----------------------------------------------------------

### Lệnh Terminal để chỉ chạy chính xác hàm Đặt Vé có trong file

Theo kiến trúc của dự án, các luồng Chọn ghế/Thanh toán được gắn Category là `DatVe`, còn luồng số (5) dài 8 bước sẽ chạy ở Category `FlowDatVe`. Để chạy chính xác những kịch bản liên quan đến Đặt Vé này, hãy mở Terminal tại thư mục codebase (chỗ chưa file `.sln` hoặc `.Tests`) rồi gõ:

```bash
# Lệnh 1: Chỉ chạy test các luồng Chọn Ghế, Check Trùng và Thanh Toán cục bộ (STT 1 - STT 4)
dotnet test --filter "TestCategory=DatVe" --logger "console;verbosity=detailed"

# Lệnh 2: Chỉ chạy duy nhất luồng E2E 8 bước (thực tế luồng này là dài nhất - STT 5)
dotnet test --filter "TestCategory=FlowDatVe" --logger "console;verbosity=detailed"

# GỘP 2 LỆNH TRÊN: Bạn cũng có thể dùng dấu gạch đứng ( | ) để chạy CÙNG LÚC cả hai cụm luồng trên
dotnet test --filter "TestCategory=DatVe|TestCategory=FlowDatVe" --logger "console;verbosity=detailed"
```
