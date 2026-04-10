using System.Text;
using Dam_Bao_Chat_Luong.Services.KhachHang;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dam_Bao_Chat_Luong.Tests.KhachHang;

/// <summary>
/// Test quản lý tài khoản: II.5_LS_01, II.5_CN_01, II.5_ECN_01
/// </summary>
[TestClass]
public class QuanLyTaiKhoanTests
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
    [TestCategory("QuanLy")]
    [Description("II.5_LS_01: Xem lịch sử đặt vé")]
    public async Task Test_II5_LS01_LichSuDatVe()
    {
        var tc = await _reader.GetTestCaseById("II.5_LS_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_LS_01");

        var result = _selenium.Test_LS01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("QuanLy")]
    [Description("II.5_CN_01: Xem thông tin cá nhân")]
    public async Task Test_II5_CN01_XemThongTin()
    {
        var tc = await _reader.GetTestCaseById("II.5_CN_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_CN_01");

        var result = _selenium.Test_CN01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("QuanLy")]
    [Description("II.5_ECN_01: Chỉnh sửa thông tin cá nhân")]
    public async Task Test_II5_ECN01_ChinhSuaThongTin()
    {
        var tc = await _reader.GetTestCaseById("II.5_ECN_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_ECN_01");

        var result = _selenium.Test_ECN01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("QuanLy")]
    [TestCategory("GUI")]
    [Description("II.5_MK_01: Kiểm tra form đổi mật khẩu hiển thị đúng")]
    public async Task Test_II5_MK01_FormDoiMatKhau()
    {
        var tc = await _reader.GetTestCaseById("II.5_MK_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_MK_01");

        var result = _selenium.Test_MK01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("QuanLy")]
    [TestCategory("GUI")]
    [Description("II.5_MK_02: Đổi mật khẩu nhập sai mật khẩu cũ")]
    public async Task Test_II5_MK02_DoiMK_SaiMatKhauCu()
    {
        var tc = await _reader.GetTestCaseById("II.5_MK_02");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_MK_02");

        var result = _selenium.Test_MK02(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }

    [TestMethod]
    [TestCategory("KhachHang")]
    [TestCategory("QuanLy")]
    [TestCategory("GUI")]
    [Description("II.5_DX_01: Đăng xuất thành công")]
    public async Task Test_II5_DX01_DangXuat()
    {
        var tc = await _reader.GetTestCaseById("II.5_DX_01");
        Assert.IsNotNull(tc, "Không tìm thấy test case II.5_DX_01");

        var result = _selenium.Test_DX01(tc);
        await _writer.WriteTestResult(result);

        Assert.AreEqual("PASS", result.Status,
            $"Test FAIL: {string.Join(" | ", result.StepResults.Select(s => s.ActualResult))}");
    }
}
