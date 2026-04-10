using System.Text;
using Dam_Bao_Chat_Luong.Models.KhachHang;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test Flow đặt vé End-to-End (A-Z):
///   Đăng nhập → Chọn chuyến xe → Chọn ghế → Chọn thanh toán → Thanh toán MoMo → Đăng xuất
/// 
/// Test case sử dụng:
///   - II.6_FLOW_01: Flow hoàn chỉnh đặt vé thành công (8 steps)
///   - II.6_FLOW_02: Flow đặt vé từ spreadsheet nếu có
///
/// Cách chạy:
///   dotnet test --filter "TestCategory=FlowDatVe" --logger "console;verbosity=detailed"
///   dotnet test --filter "FullyQualifiedName~Test_Flow_DatVe_E2E"
/// </summary>
[TestClass]
public class FlowDatVeTests
{
    private KhachHangSeleniumService _selenium = null!;
    private KhachHangExcelReaderService _reader = null!;
    private KhachHangExcelWriterService _writer = null!;

    [TestInitialize]
    public void Setup()
    {
        Console.OutputEncoding = Encoding.UTF8;
        _selenium = new KhachHangSeleniumService();
        _reader = new KhachHangExcelReaderService();
        _writer = new KhachHangExcelWriterService();
    }

    [TestCleanup]
    public void Cleanup() => _selenium?.Dispose();

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("FlowDatVe")]
    [TestCategory("E2E")]
    [Description("II.6_FLOW_01: Flow đặt vé E2E — Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán MoMo → Đăng xuất")]
    [Timeout(300000)] // 5 phút timeout cho flow E2E
    public async Task Test_Flow_DatVe_E2E_FullFlow()
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  🚀 FLOW ĐẶT VÉ END-TO-END (A → Z)                        ║");
        Console.WriteLine("║  Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán → Logout  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        // Thử lấy test case từ spreadsheet trước
        KhachHangTestCaseModel? tc = null;
        try
        {
            tc = await _reader.GetTestCaseById("II.6_FLOW_01");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️ Không thể đọc từ spreadsheet: {ex.Message}");
            Console.WriteLine("  → Sử dụng test case local (hardcoded steps)");
        }

        // Nếu không có trên spreadsheet → tạo test case local với 8 steps
        tc ??= CreateLocalFlowTestCase();

        Console.WriteLine($"\n  📋 Test Case: {tc.TestCaseId}");
        Console.WriteLine($"  📝 Mô tả: {tc.TestObjective}");
        Console.WriteLine($"  📊 Số steps: {tc.Steps.Count}");
        Console.WriteLine();

        // Chạy flow
        var result = _selenium.Test_Flow_DatVe_E2E(tc);

        // Log kết quả từng step
        Console.WriteLine("\n┌──────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│  📊 KẾT QUẢ TỪNG BƯỚC                                       │");
        Console.WriteLine("├──────────────────────────────────────────────────────────────┤");
        foreach (var sr in result.StepResults)
        {
            var icon = sr.ActualResult?.Contains("thành công", StringComparison.OrdinalIgnoreCase) == true
                    || sr.ActualResult?.Contains("Hiển thị", StringComparison.OrdinalIgnoreCase) == true
                    || sr.ActualResult?.StartsWith("-") == true
                ? "✅" : "❌";
            Console.WriteLine($"│  {icon} Step {sr.StepNumber}: {sr.ActualResult?.Split('\n').FirstOrDefault()}");
        }
        Console.WriteLine("├──────────────────────────────────────────────────────────────┤");
        Console.WriteLine($"│  📸 Screenshots: {result.ScreenshotPaths.Count} ảnh");
        Console.WriteLine($"│  🏆 Kết quả tổng: {result.Status}");
        Console.WriteLine("└──────────────────────────────────────────────────────────────┘");

        // Ghi kết quả lên spreadsheet
        if (tc.SpreadsheetStartRow > 0)
        {
            try
            {
                await _writer.WriteTestResult(result);
                Console.WriteLine("  ✅ Đã ghi kết quả lên Google Spreadsheet");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️ Không ghi được lên spreadsheet: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("  ℹ️ Bỏ qua ghi Google Sheet vì Local Test (SpreadsheetStartRow = 0)");
        }

        // Assert
        Assert.AreEqual("PASS", result.Status,
            $"Flow đặt vé E2E FAIL:\n{string.Join("\n", result.StepResults.Select(s => $"  Step {s.StepNumber}: {s.ActualResult}"))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("FlowDatVe")]
    [TestCategory("E2E")]
    [Description("II.6_FLOW_01: Flow đặt vé từ spreadsheet")]
    [Timeout(300000)]
    public async Task Test_Flow_DatVe_E2E_FromSpreadsheet()
    {
        KhachHangTestCaseModel? tc = null;
        try
        {
            tc = await _reader.GetTestCaseById("II.6_FLOW_01");
        }
        catch { }

        if (tc == null)
        {
            // Fallback: dùng local test case nếu chưa có trên spreadsheet
            Console.WriteLine("  ⚠️ II.6_FLOW_01 chưa có trên spreadsheet — dùng local test case");
            tc = CreateLocalFlowTestCase();
        }

        Console.WriteLine($"  📋 Chạy Flow: {tc.TestCaseId} — {tc.TestObjective}");

        var result = _selenium.Test_Flow_DatVe_E2E(tc);

        // Ghi kết quả lên spreadsheet nếu có row mapping
        if (tc.SpreadsheetStartRow > 0)
        {
            try { await _writer.WriteTestResult(result); }
            catch (Exception ex) { Console.WriteLine($"  ⚠️ Không ghi được lên spreadsheet: {ex.Message}"); }
        }
        else
        {
            Console.WriteLine("  ℹ️ Bỏ qua ghi Google Sheet vì Local Test (SpreadsheetStartRow = 0)");
        }

        foreach (var sr in result.StepResults)
            Console.WriteLine($"  Step {sr.StepNumber}: {sr.ActualResult}");

        Assert.AreEqual("PASS", result.Status,
            $"Flow FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    /// <summary>
    /// Tạo test case local với 8 steps cho Flow đặt vé E2E
    /// Dùng khi không có test case trên Google Spreadsheet
    /// </summary>
    private static KhachHangTestCaseModel CreateLocalFlowTestCase()
    {
        return new KhachHangTestCaseModel
        {
            No = "II.6",
            TestRequirementId = "II.6_FlowDatVe",
            TestCaseId = "II.6_FLOW_01",
            TestObjective = "Flow đặt vé End-to-End: Đăng nhập → Chọn chuyến → Chọn ghế → Thanh toán MoMo → Kiểm tra lịch sử → Đăng xuất",
            PreConditions = "Có tài khoản khách hàng hợp lệ, có chuyến xe khả dụng trên trang chủ",
            SpreadsheetStartRow = 0,
            SpreadsheetEndRow = 0,
            Steps = new List<KhachHangTestStep>
            {
                new()
                {
                    StepNumber = 1,
                    Action = "Truy cập trang chủ hệ thống",
                    ExpectedResult = "Trang chủ hiển thị với danh sách chuyến xe và nút Đặt vé",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 2,
                    Action = "Đăng nhập với tài khoản khách hàng (duc19092005d@gmail.com / anhduc9a5)",
                    TestDataRaw = "{\"email\":\"duc19092005d@gmail.com\",\"password\":\"anhduc9a5\"}",
                    ExpectedResult = "Đăng nhập thành công, chuyển hướng về trang chủ, hiển thị tên người dùng trên navbar",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 3,
                    Action = "Chọn chuyến xe từ trang chủ → vào trang sơ đồ ghế",
                    ExpectedResult = "Chuyển đến trang sơ đồ ghế, hiển thị ghế trống (xanh) và ghế đã bán (đỏ/xám)",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 4,
                    Action = "Chọn 1 ghế trống trên sơ đồ",
                    ExpectedResult = "Ghế đổi màu (Đang chọn), tổng tiền tự động cập nhật",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 5,
                    Action = "Chọn phương thức thanh toán MoMo từ dropdown",
                    ExpectedResult = "Đã chọn phương thức thanh toán MoMo",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 6,
                    Action = "Bấm nút 'Tiếp tục' để chuyển đến trang thanh toán",
                    ExpectedResult = "Hệ thống tạo đơn hàng, chuyển hướng sang trang thanh toán MoMo (Giả lập)",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 7,
                    Action = "Bấm 'Thanh toán thành công' trên trang MoMo Checkout",
                    ExpectedResult = "Hiển thị màn hình Booking Success, đơn hàng cập nhật trạng thái Đã thanh toán",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 8,
                    Action = "Vào trang Lịch sử mua vé và kiểm tra vé vừa đặt",
                    ExpectedResult = "Vé vừa đặt tồn tại trong Lịch sử mua vé với trạng thái Đã thanh toán",
                    SpreadsheetRow = 0
                },
                new()
                {
                    StepNumber = 9,
                    Action = "Đăng xuất khỏi hệ thống",
                    ExpectedResult = "Đăng xuất thành công, quay về trang Đăng nhập",
                    SpreadsheetRow = 0
                }
            }
        };
    }
}
