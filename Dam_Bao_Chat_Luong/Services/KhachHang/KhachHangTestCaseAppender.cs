using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace Dam_Bao_Chat_Luong.Services.KhachHang;

/// <summary>
/// Service append test cases mới vào cuối sheet "Khách hàng" trên Google Spreadsheet.
/// Sử dụng API Append để KHÔNG ghi đè dữ liệu cũ.
/// </summary>
public class KhachHangTestCaseAppender
{
    private readonly string _spreadsheetId = "1oG1OjLR2BR-RsCnU7DS4wMt7P22xjt16yaln69VsFHc";
    private readonly string _sheetName = "Khách hàng";
    private readonly IConfiguration? _configuration;

    public KhachHangTestCaseAppender()
    {
        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
        }
        catch { _configuration = null; }
    }

    /// <summary>
    /// Append tất cả test cases mới vào cuối sheet.
    /// Mỗi test case có thể có nhiều rows (nhiều steps).
    /// </summary>
    public async Task<bool> AppendNewTestCases()
    {
        var clientId = _configuration?["Google:ClientId"];
        var clientSecret = _configuration?["Google:ClientSecret"];
        if (string.IsNullOrEmpty(clientId) || clientId.Contains("YOUR_") ||
            string.IsNullOrEmpty(clientSecret) || clientSecret.Contains("YOUR_"))
        {
            Console.WriteLine("⚠️ Chưa cấu hình Google OAuth2. Bỏ qua ghi spreadsheet.");
            return false;
        }

        try
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                new[] { SheetsService.Scope.Spreadsheets },
                "user", CancellationToken.None, new FileDataStore("token_store", true));

            var sheetsService = new SheetsService(new BaseClientService.Initializer
                { HttpClientInitializer = credential, ApplicationName = "KhachHang_Test" });

            // Đọc dữ liệu hiện tại để kiểm tra test case nào đã tồn tại
            var existingData = await sheetsService.Spreadsheets.Values
                .Get(_spreadsheetId, $"'{_sheetName}'!A:L")
                .ExecuteAsync();

            var existingIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (existingData.Values != null)
            {
                foreach (var row in existingData.Values)
                {
                    if (row.Count > 2)
                    {
                        var testCaseId = row[2]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(testCaseId))
                            existingIds.Add(testCaseId);
                    }
                }
            }

            Console.WriteLine($"  📋 Tìm thấy {existingIds.Count} test case IDs đã tồn tại trên sheet");

            // Định nghĩa test cases mới
            var newTestCases = GetNewTestCaseDefinitions();

            // Lọc test cases chưa tồn tại
            var toAppend = newTestCases.Where(tc => !existingIds.Contains(tc.TestCaseId)).ToList();

            if (toAppend.Count == 0)
            {
                Console.WriteLine("  ✅ Tất cả test cases đã tồn tại trên sheet. Không cần thêm.");
                return true;
            }

            Console.WriteLine($"  📝 Sẽ thêm {toAppend.Count} test cases mới...");

            // Build rows để append
            var rows = new List<IList<object>>();
            int noCounter = 16; // Bắt đầu từ II.16

            // Thêm section headers nếu cần
            foreach (var group in toAppend.GroupBy(tc => tc.SectionHeader))
            {
                if (!string.IsNullOrEmpty(group.Key) && !existingIds.Contains(group.Key))
                {
                    rows.Add(new List<object> { group.Key, "", "", "", "", "", "", "", "", "", "", "" });
                }

                foreach (var tc in group)
                {
                    // Row đầu tiên của test case (có No., Test Req ID, Test Case ID, ...)
                    for (int i = 0; i < tc.Steps.Count; i++)
                    {
                        var step = tc.Steps[i];
                        var row = new List<object>();

                        if (i == 0)
                        {
                            row.Add($"II.{noCounter}");        // No.
                            row.Add(tc.TestRequirementId);      // Test Requirement ID
                            row.Add(tc.TestCaseId);             // Test Case ID
                            row.Add(tc.TestObjective);          // Test Objective
                            row.Add(tc.PreConditions);          // Pre-conditions
                        }
                        else
                        {
                            row.Add(""); row.Add(""); row.Add(""); row.Add(""); row.Add("");
                        }

                        row.Add(step.StepNumber.ToString());   // Step #
                        row.Add(step.Action);                  // Step Action
                        row.Add(step.TestData ?? "");          // Test Data
                        row.Add(step.ExpectedResult ?? "");    // Expected Result
                        row.Add("");                           // Actual Result (blank)
                        row.Add("");                           // Status (blank)
                        row.Add("");                           // Notes (blank)

                        rows.Add(row);
                    }
                    noCounter++;
                }
            }

            // Append rows
            var appendBody = new ValueRange { Values = rows };
            var appendReq = sheetsService.Spreadsheets.Values.Append(
                appendBody, _spreadsheetId, $"'{_sheetName}'!A:L");
            appendReq.ValueInputOption = SpreadsheetsResource.ValuesResource
                .AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendReq.InsertDataOption = SpreadsheetsResource.ValuesResource
                .AppendRequest.InsertDataOptionEnum.INSERTROWS;

            var result = await appendReq.ExecuteAsync();
            Console.WriteLine($"  ✅ Đã append {rows.Count} rows vào sheet '{_sheetName}'");
            Console.WriteLine($"  📍 Range: {result.Updates?.UpdatedRange}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Lỗi append test cases: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Định nghĩa tất cả test cases mới cần thêm
    /// </summary>
    private List<NewTestCaseDefinition> GetNewTestCaseDefinitions()
    {
        return new List<NewTestCaseDefinition>
        {
            // ═══ ĐĂNG NHẬP GUI ═══
            new()
            {
                SectionHeader = "ĐĂNG NHẬP",
                TestRequirementId = "II.1_DangNhap",
                TestCaseId = "II.1_LG_01",
                TestObjective = "Kiểm tra trang đăng nhập hiển thị đúng (form, fields, button)",
                PreConditions = "Người dùng chưa đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Truy cập trang Đăng nhập", ExpectedResult = "- Trang đăng nhập hiển thị đầy đủ:\n  + Ô nhập Email/SĐT\n  + Ô nhập Mật khẩu\n  + Nút ĐĂNG NHẬP\n  + Link Đăng ký" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.1_DangNhap",
                TestCaseId = "II.1_LG_02",
                TestObjective = "Đăng nhập để trống Email",
                PreConditions = "Đang ở trang Đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Để trống Email, nhập Password rồi bấm Đăng nhập", TestData = "{\"email\":\"\",\"password\":\"somepassword\"}", ExpectedResult = "- Vẫn ở trang đăng nhập\n- Hiển thị thông báo validation yêu cầu nhập Email" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.1_DangNhap",
                TestCaseId = "II.1_LG_03",
                TestObjective = "Đăng nhập để trống Password",
                PreConditions = "Đang ở trang Đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Nhập Email, để trống Password rồi bấm Đăng nhập", TestData = "{\"email\":\"test@gmail.com\",\"password\":\"\"}", ExpectedResult = "- Vẫn ở trang đăng nhập\n- Hiển thị thông báo validation yêu cầu nhập Mật khẩu" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.1_DangNhap",
                TestCaseId = "II.1_LG_04",
                TestObjective = "Đăng nhập sai mật khẩu",
                PreConditions = "Đang ở trang Đăng nhập, có tài khoản hợp lệ",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Nhập đúng Email nhưng sai mật khẩu rồi bấm Đăng nhập", TestData = "{\"email\":\"duc19092005d@gmail.com\",\"password\":\"wrongpassword123\"}", ExpectedResult = "- Vẫn ở trang đăng nhập\n- Hiển thị thông báo đăng nhập thất bại" }
                }
            },

            // ═══ ĐĂNG KÝ MỞ RỘNG ═══
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.1_DangKy",
                TestCaseId = "II.1_DK_03",
                TestObjective = "Đăng ký thất bại do mật khẩu không khớp",
                PreConditions = "Đang ở trang Đăng ký",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Nhập đầy đủ thông tin nhưng 2 ô mật khẩu không giống nhau rồi bấm Đăng ký", ExpectedResult = "- Giữ nguyên trang đăng ký\n- Hiển thị thông báo lỗi: Mật khẩu không khớp" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.1_DangKy",
                TestCaseId = "II.1_DK_04",
                TestObjective = "Đăng ký thất bại khi để trống tất cả fields",
                PreConditions = "Đang ở trang Đăng ký",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Không nhập gì, bấm Đăng ký", ExpectedResult = "- Giữ nguyên trang đăng ký\n- Hiển thị validation yêu cầu nhập thông tin bắt buộc" }
                }
            },

            // ═══ NAVIGATION ═══
            new()
            {
                SectionHeader = "NAVIGATION",
                TestRequirementId = "II.6_Navigation",
                TestCaseId = "II.6_NAV_01",
                TestObjective = "Kiểm tra Navbar hiển thị đúng sau đăng nhập",
                PreConditions = "Đã đăng nhập với tài khoản khách hàng",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Kiểm tra thanh điều hướng (Navbar) trên trang chủ", ExpectedResult = "- Navbar hiển thị đầy đủ:\n  + Trang chủ\n  + Lịch sử\n  + Lịch trình\n  + Về chúng tôi\n  + Liên hệ\n  + Tên người dùng" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.6_Navigation",
                TestCaseId = "II.6_NAV_02",
                TestObjective = "Điều hướng đến tất cả các trang từ navbar",
                PreConditions = "Đã đăng nhập, đang ở trang chủ",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Click lần lượt vào từng link trên navbar: Lịch sử, Lịch trình, Về chúng tôi", ExpectedResult = "- Tất cả các trang đều load thành công\n- URL đúng với từng trang" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.6_Navigation",
                TestCaseId = "II.6_NAV_03",
                TestObjective = "Kiểm tra trang Về chúng tôi hiển thị đúng",
                PreConditions = "Không cần đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Truy cập trang /Home_User/About", ExpectedResult = "- Trang Về chúng tôi hiển thị đúng với nội dung giới thiệu công ty" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.6_Navigation",
                TestCaseId = "II.6_NAV_04",
                TestObjective = "Kiểm tra trang Lịch trình hiển thị đúng",
                PreConditions = "Không cần đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Truy cập trang /Home_User/ChuyenXe_User", ExpectedResult = "- Trang Lịch trình hiển thị danh sách chuyến xe / tuyến đường" }
                }
            },

            // ═══ ĐỔI MẬT KHẨU & ĐĂNG XUẤT ═══
            new()
            {
                SectionHeader = "QUẢN LÝ TÀI KHOẢN",
                TestRequirementId = "II.5_QuanLy",
                TestCaseId = "II.5_MK_01",
                TestObjective = "Kiểm tra form đổi mật khẩu hiển thị đúng",
                PreConditions = "Đã đăng nhập",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Truy cập trang Đặt lại mật khẩu (/Auth/ResetPassword)", ExpectedResult = "- Form đổi mật khẩu hiển thị đầy đủ:\n  + Ô Mật khẩu cũ\n  + Ô Mật khẩu mới\n  + Ô Xác nhận mật khẩu\n  + Nút Xác nhận" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.5_QuanLy",
                TestCaseId = "II.5_MK_02",
                TestObjective = "Đổi mật khẩu nhập sai mật khẩu cũ",
                PreConditions = "Đã đăng nhập, đang ở trang Đặt lại mật khẩu",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Nhập sai Mật khẩu cũ, nhập Mật khẩu mới hợp lệ rồi bấm Xác nhận", TestData = "{\"old_password\":\"wrongOldPassword123\",\"new_password\":\"NewPassword@123\"}", ExpectedResult = "- Hiển thị thông báo lỗi: Mật khẩu cũ không đúng\n- Không đổi mật khẩu" }
                }
            },
            new()
            {
                SectionHeader = "",
                TestRequirementId = "II.5_QuanLy",
                TestCaseId = "II.5_DX_01",
                TestObjective = "Đăng xuất thành công",
                PreConditions = "Đã đăng nhập với tài khoản khách hàng",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Đăng nhập với tài khoản khách hàng", ExpectedResult = "- Đăng nhập thành công" },
                    new() { StepNumber = 2, Action = "Bấm Đăng xuất", ExpectedResult = "- Đăng xuất thành công\n- Chuyển hướng về trang Đăng nhập" }
                }
            },

            // ═══ FLOW ĐẶT VÉ E2E ═══
            new()
            {
                SectionHeader = "FLOW ĐẶT VÉ E2E",
                TestRequirementId = "II.6_FlowDatVe",
                TestCaseId = "II.6_FLOW_01",
                TestObjective = "Flow đặt vé End-to-End: Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán MoMo → Kiểm tra lịch sử → Đăng xuất",
                PreConditions = "Có tài khoản khách hàng hợp lệ, có chuyến xe khả dụng trên trang chủ",
                Steps = new List<NewTestStep>
                {
                    new() { StepNumber = 1, Action = "Truy cập trang chủ hệ thống", ExpectedResult = "Trang chủ hiển thị với danh sách chuyến xe và nút Đặt vé" },
                    new() { StepNumber = 2, Action = "Đăng nhập với tài khoản khách hàng", TestData = "{\"email\":\"duc19092005d@gmail.com\",\"password\":\"anhduc9a5\"}", ExpectedResult = "Đăng nhập thành công, chuyển hướng về trang chủ" },
                    new() { StepNumber = 3, Action = "Chọn chuyến xe từ trang chủ → vào trang sơ đồ ghế", ExpectedResult = "Chuyển đến trang sơ đồ ghế, hiển thị ghế trống" },
                    new() { StepNumber = 4, Action = "Chọn 1 ghế trống trên sơ đồ", ExpectedResult = "Ghế đổi màu (Đang chọn), tổng tiền tự động cập nhật" },
                    new() { StepNumber = 5, Action = "Chọn phương thức thanh toán MoMo từ dropdown", ExpectedResult = "Đã chọn phương thức thanh toán MoMo" },
                    new() { StepNumber = 6, Action = "Bấm nút Tiếp tục để chuyển đến trang thanh toán", ExpectedResult = "Hệ thống tạo đơn hàng, chuyển hướng sang trang thanh toán MoMo" },
                    new() { StepNumber = 7, Action = "Thanh toán thành công trên trang MoMo Checkout", ExpectedResult = "Hiển thị màn hình Booking Success" },
                    new() { StepNumber = 8, Action = "Vào trang Lịch sử mua vé và kiểm tra vé vừa đặt", ExpectedResult = "Vé vừa đặt tồn tại trong Lịch sử mua vé" },
                    new() { StepNumber = 9, Action = "Đăng xuất khỏi hệ thống", ExpectedResult = "Đăng xuất thành công, quay về trang Đăng nhập" }
                }
            },
        };
    }

    private class NewTestCaseDefinition
    {
        public string SectionHeader { get; set; } = "";
        public string TestRequirementId { get; set; } = "";
        public string TestCaseId { get; set; } = "";
        public string TestObjective { get; set; } = "";
        public string PreConditions { get; set; } = "";
        public List<NewTestStep> Steps { get; set; } = new();
    }

    private class NewTestStep
    {
        public int StepNumber { get; set; }
        public string Action { get; set; } = "";
        public string? TestData { get; set; }
        public string? ExpectedResult { get; set; }
    }
}
