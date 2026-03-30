using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test đăng ký tài khoản: II.1_DK_01, II.1_DK_02
/// </summary>
[TestClass]
public class DangKyTests
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
    [TestCategory("DangKy")]
    [Description("II.1_DK_01: Đăng ký tài khoản khách hàng thành công")]
    public async Task Test_II1_DK01_DangKy_ThanhCong()
    {
        var tc = await _reader.GetTestCaseById("II.1_DK_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_DK_01");

        var result = _selenium.Test_DK01(tc);
        await _writer.WriteTestResult(result);

        Console.WriteLine($"  Status: {result.Status}");
        foreach (var sr in result.StepResults)
            Console.WriteLine($"  Step {sr.StepNumber}: {sr.ActualResult}");

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("DangKy")]
    [Description("II.1_DK_02: Đăng ký thất bại do trùng Email/SĐT")]
    public async Task Test_II1_DK02_DangKy_TrungEmail()
    {
        var tc = await _reader.GetTestCaseById("II.1_DK_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_DK_02");

        var result = _selenium.Test_DK02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
