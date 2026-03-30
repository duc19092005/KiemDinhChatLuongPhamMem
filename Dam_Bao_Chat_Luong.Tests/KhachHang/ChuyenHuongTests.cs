using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test chuyển hướng: II.2_CH_01
/// </summary>
[TestClass]
public class ChuyenHuongTests
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
    [TestCategory("ChuyenHuong")]
    [Description("II.2_CH_01: Chuyển hướng đúng về trang đặt vé sau khi đăng nhập")]
    public async Task Test_II2_CH01_ChuyenHuong_SauDangNhap()
    {
        var tc = await _reader.GetTestCaseById("II.2_CH_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.2_CH_01");

        var result = _selenium.Test_CH01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
