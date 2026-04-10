using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test điều hướng (Navigation): II.6_NAV_01 → II.6_NAV_04
/// </summary>
[TestClass]
public class NavigationTests
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
    [TestCategory("GUI")]
    [TestCategory("Navigation")]
    [Description("II.6_NAV_01: Kiểm tra Navbar hiển thị đúng sau đăng nhập")]
    public async Task Test_II6_NAV01_Navbar_HienThi()
    {
        var tc = await _reader.GetTestCaseById("II.6_NAV_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.6_NAV_01");

        var result = _selenium.Test_NAV01(tc);
        await _writer.WriteTestResult(result);

        Console.WriteLine($"  Status: {result.Status}");
        foreach (var sr in result.StepResults)
            Console.WriteLine($"  Step {sr.StepNumber}: {sr.ActualResult}");

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("GUI")]
    [TestCategory("Navigation")]
    [Description("II.6_NAV_02: Điều hướng đến tất cả trang từ navbar")]
    public async Task Test_II6_NAV02_DieuHuong_TatCaTrang()
    {
        var tc = await _reader.GetTestCaseById("II.6_NAV_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.6_NAV_02");

        var result = _selenium.Test_NAV02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("GUI")]
    [TestCategory("Navigation")]
    [Description("II.6_NAV_03: Trang Về chúng tôi hiển thị đúng")]
    public async Task Test_II6_NAV03_VeChungToi()
    {
        var tc = await _reader.GetTestCaseById("II.6_NAV_03");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.6_NAV_03");

        var result = _selenium.Test_NAV03(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("GUI")]
    [TestCategory("Navigation")]
    [Description("II.6_NAV_04: Trang Lịch trình hiển thị đúng")]
    public async Task Test_II6_NAV04_LichTrinh()
    {
        var tc = await _reader.GetTestCaseById("II.6_NAV_04");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.6_NAV_04");

        var result = _selenium.Test_NAV04(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
