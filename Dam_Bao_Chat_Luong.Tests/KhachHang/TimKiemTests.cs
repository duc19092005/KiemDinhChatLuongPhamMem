using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test tìm kiếm chuyến xe: II.3_TK_01, II.3_TK_02, II.3_TK_03
/// </summary>
[TestClass]
public class TimKiemTests
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
    [TestCategory("TimKiem")]
    [Description("II.3_TK_01: Tìm kiếm chuyến xe hợp lệ")]
    public async Task Test_II3_TK01_TimKiem_HopLe()
    {
        var tc = await _reader.GetTestCaseById("II.3_TK_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.3_TK_01");

        var result = _selenium.Test_TK01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("TimKiem")]
    [Description("II.3_TK_02: Báo lỗi khi tìm kiếm với ngày đi trong quá khứ")]
    public async Task Test_II3_TK02_TimKiem_NgayQuaKhu()
    {
        var tc = await _reader.GetTestCaseById("II.3_TK_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.3_TK_02");

        var result = _selenium.Test_TK02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("TimKiem")]
    [Description("II.3_TK_03: Xử lý khi không có chuyến xe nào")]
    public async Task Test_II3_TK03_TimKiem_KhongCoChuyen()
    {
        var tc = await _reader.GetTestCaseById("II.3_TK_03");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.3_TK_03");

        var result = _selenium.Test_TK03(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
