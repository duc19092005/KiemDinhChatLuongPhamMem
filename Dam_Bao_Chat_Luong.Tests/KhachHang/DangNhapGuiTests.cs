using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test GUI trang đăng nhập: II.1_LG_01 → II.1_LG_04
/// </summary>
[TestClass]
public class DangNhapGuiTests
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
    [TestCategory("DangNhap")]
    [Description("II.1_LG_01: Kiểm tra trang đăng nhập hiển thị đúng")]
    public async Task Test_II1_LG01_LoginPage_HienThi()
    {
        var tc = await _reader.GetTestCaseById("II.1_LG_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_LG_01");

        var result = _selenium.Test_LG01(tc);
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
    [TestCategory("DangNhap")]
    [Description("II.1_LG_02: Đăng nhập để trống Email")]
    public async Task Test_II1_LG02_LoginPage_TrongEmail()
    {
        var tc = await _reader.GetTestCaseById("II.1_LG_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_LG_02");

        var result = _selenium.Test_LG02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("GUI")]
    [TestCategory("DangNhap")]
    [Description("II.1_LG_03: Đăng nhập để trống Password")]
    public async Task Test_II1_LG03_LoginPage_TrongPassword()
    {
        var tc = await _reader.GetTestCaseById("II.1_LG_03");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_LG_03");

        var result = _selenium.Test_LG03(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("GUI")]
    [TestCategory("DangNhap")]
    [Description("II.1_LG_04: Đăng nhập sai mật khẩu")]
    public async Task Test_II1_LG04_LoginPage_SaiMatKhau()
    {
        var tc = await _reader.GetTestCaseById("II.1_LG_04");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.1_LG_04");

        var result = _selenium.Test_LG04(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
