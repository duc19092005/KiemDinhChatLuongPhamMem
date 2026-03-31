using System.Text;
using Dam_Bao_Chat_Luong.Enums;
using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services;
using Dam_Bao_Chat_Luong.Services.ThirdPersonServices;
using Newtonsoft.Json;

namespace Dam_Bao_Chat_Luong;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       🧪 Kiểm Thử Tự Động - Đảm Bảo Chất Lượng        ║");
        Console.WriteLine("║              Dam_Bao_Chat_Luong v1.0                     ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        // ===== 1. Đọc test cases từ Google Spreadsheet =====
        Console.WriteLine("📋 Đang đọc dữ liệu test case từ Google Spreadsheet...");
        var spreadsheetService = new GoogleSpreadSheetService();

        List<TestCaseModel> testCases;
        try
        {
            testCases = await spreadsheetService.GetTestCases(GoogleSpreadSheetEnum.DangNhap);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Lỗi khi đọc spreadsheet: {ex.Message}");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Đã đọc {testCases.Count} test case(s) từ sheet 'Đăng nhập'");
        Console.ResetColor();

        // Hiển thị thông tin test cases
        Console.WriteLine();
        for (int i = 0; i < testCases.Count; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"  [{i + 1}] {tc.TestCaseId}: {tc.TestObjective}");
            Console.WriteLine($"      Số bước: {tc.Steps.Count} | Số roles: {tc.LoginDataList.Count}");
            Console.WriteLine($"      Expected: {tc.ExpectedResult}");
        }

        // ===== 2. Chọn chế độ =====
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("📌 Chọn chế độ chạy test:");
        Console.ResetColor();
        Console.WriteLine("  1. Chạy đơn luồng đăng nhập (tự động + chụp ảnh + ghi kết quả)");
        Console.WriteLine("  2. Chạy multi luồng (trả driver cho bạn thực hiện luồng kế tiếp)");
        Console.Write("\n  Nhập lựa chọn (1/2): ");

        var choice = Console.ReadLine()?.Trim();

        switch (choice)
        {
            case "1":
                await RunSingleFlowAsync(testCases, spreadsheetService);
                break;
            case "2":
                RunMultiFlow(testCases);
                break;
            default:
                Console.WriteLine("Lựa chọn không hợp lệ. Thoát.");
                break;
        }
    }

    /// <summary>
    /// Mode 1: Chạy đơn luồng — tự động chạy tất cả test cases, chụp ảnh, ghi kết quả
    /// </summary>
    static async Task RunSingleFlowAsync(List<TestCaseModel> testCases, GoogleSpreadSheetService spreadsheetService)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🚀 Bắt đầu chạy test đơn luồng đăng nhập...");
        Console.ResetColor();

        using var seleniumService = new SeleniumTestService();
        var results = seleniumService.RunAllLoginTests(testCases);

        // ===== Hiển thị tổng kết =====
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    📊 TỔNG KẾT KẾT QUẢ                  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var passCount = results.Count(r => r.Status == "PASS");
        var failCount = results.Count(r => r.Status == "FAIL");
        var errorCount = results.Count(r => r.Status == "ERROR");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✅ PASS:  {passCount}/{results.Count}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ❌ FAIL:  {failCount}/{results.Count}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠️  ERROR: {errorCount}/{results.Count}");
        Console.ResetColor();

        Console.WriteLine();
        foreach (var result in results)
        {
            var icon = result.Status switch
            {
                "PASS" => "✅",
                "FAIL" => "❌",
                _ => "⚠️"
            };
            Console.WriteLine($"  {icon} [{result.TestCaseId}] Role: {result.Role,-25} | {result.Status,-5} | {result.ActualResult}");
        }

        // ===== Lưu kết quả local =====
        Console.WriteLine();
        Console.WriteLine("💾 Lưu kết quả...");

        // Lưu file JSON
        var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults",
            $"login_results_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
        var json = JsonConvert.SerializeObject(results, Formatting.Indented);
        await File.WriteAllTextAsync(jsonPath, json, Encoding.UTF8);
        Console.WriteLine($"  📄 JSON: {jsonPath}");

        // Lưu file text report
        var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults",
            $"login_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        var reportLines = new List<string>
        {
            $"=== BÁO CÁO KIỂM THỬ ĐĂNG NHẬP ===",
            $"Thời gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"Tổng: {results.Count} | Pass: {passCount} | Fail: {failCount} | Error: {errorCount}",
            "",
            "--- CHI TIẾT ---"
        };
        foreach (var r in results)
        {
            reportLines.Add($"\n[{r.Status}] {r.TestCaseId} - Role: {r.Role}");
            reportLines.Add($"  Objective: {r.TestObjective}");
            reportLines.Add($"  Expected: {r.ExpectedResult}");
            reportLines.Add($"  Actual: {r.ActualResult}");
            reportLines.Add($"  Screenshot: {r.ScreenshotPath ?? "N/A"}");
        }
        await File.WriteAllTextAsync(reportPath, string.Join("\n", reportLines), Encoding.UTF8);
        Console.WriteLine($"  📄 Report: {reportPath}");

        // ===== Ghi lên Google Spreadsheet (nếu có credentials) =====
        Console.WriteLine();
        Console.WriteLine("☁️  Đang ghi kết quả lên Google Spreadsheet...");
        await spreadsheetService.WriteTestResults(GoogleSpreadSheetEnum.DangNhap, results);

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ Hoàn tất kiểm thử đăng nhập!");
        Console.ResetColor();
    }

    /// <summary>
    /// Mode 2: Multi luồng — trả driver cho user code tiếp
    /// </summary>
    static void RunMultiFlow(List<TestCaseModel> testCases)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("🔄 Chế độ multi luồng - Khởi tạo driver...");
        Console.ResetColor();

        var seleniumService = new SeleniumTestService();
        var driver = seleniumService.GetDriver();

        Console.WriteLine($"  Driver đã sẵn sàng. Trang: {driver.Url}");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  ℹ️  Driver đã được khởi tạo và trả về cho bạn.");
        Console.WriteLine("  ℹ️  Bạn có thể sử dụng driver object để thực hiện các luồng test tiếp theo.");
        Console.WriteLine("  ℹ️  Nhấn Enter để đóng browser và kết thúc...");
        Console.ResetColor();

        Console.ReadLine();
        seleniumService.Dispose();
    }
}
