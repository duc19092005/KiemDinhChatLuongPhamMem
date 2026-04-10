using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test helper: Append test cases mới lên Google Spreadsheet.
/// Chạy 1 lần duy nhất trước khi chạy các test cases mới.
/// Lệnh: dotnet test --filter "FullyQualifiedName~AppendNewTestCases"
/// </summary>
[TestClass]
public class SpreadsheetSetupTests
{
    [TestMethod]
    [TestCategory("Setup")]
    [Description("Append tất cả test cases mới lên Google Spreadsheet (chạy 1 lần)")]
    public async Task AppendNewTestCases_ToSpreadsheet()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var appender = new KhachHangTestCaseAppender();
        var success = await appender.AppendNewTestCases();
        Assert.IsTrue(success, "Không thể append test cases lên spreadsheet");
        Console.WriteLine("  ✅ Hoàn tất append test cases mới lên Google Spreadsheet");
    }
}
