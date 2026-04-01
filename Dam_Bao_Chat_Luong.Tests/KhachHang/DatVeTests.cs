using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test đặt vé: II.4_CG_01, II.4_CG_02, II.4_TT_01, II.4_TT_02
/// </summary>
[TestClass]
public class DatVeTests
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
    [TestCategory("DatVe")]
    [Description("II.4_CG_01: Chọn ghế trên sơ đồ và tính tiền tự động")]
    public async Task Test_II4_CG01_ChonGhe_TinhTien()
    {
        var tc = await _reader.GetTestCaseById("II.4_CG_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.4_CG_01");

        var result = _selenium.Test_CG01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("DatVe")]
    [Description("II.4_CG_02: Không cho phép chọn ghế đã bán")]
    public async Task Test_II4_CG02_ChonGhe_GheDaBan()
    {
        var tc = await _reader.GetTestCaseById("II.4_CG_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.4_CG_02");

        var result = _selenium.Test_CG02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("DatVe")]
    [Description("II.4_TT_01: Thông tin hành khách được tự động điền")]
    public async Task Test_II4_TT01_AutoFill()
    {
        var tc = await _reader.GetTestCaseById("II.4_TT_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.4_TT_01");

        var result = _selenium.Test_TT01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("DatVe")]
    [Description("II.4_TT_02: Đặt vé và Thanh toán Momo thành công")]
    public async Task Test_II4_TT02_ThanhToanMomo()
    {
        var tc = await _reader.GetTestCaseById("II.4_TT_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.4_TT_02");

        var result = _selenium.Test_TT02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
