using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Chrome;

namespace Dam_Bao_Chat_Luong.Tests
{
    [TestClass]
    public class TraCuuDoiHuyVeTests
    {
        private SeleniumTestService _selenium;
        private NhanVienBanVeService _service;

        [TestInitialize]
        public void Setup()
        {
            _selenium = new SeleniumTestService();
            _service = new NhanVienBanVeService();
        }

        [TestCleanup]
        public void TearDown() => _selenium.GetDriver().Quit();

        private async Task<bool> Login(IWebDriver d, string u, string p)
        {
            d.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            try
            {
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                wait.Until(x => x.FindElement(By.Id("EmailOrPhone"))).SendKeys(u ?? "");
                d.FindElement(By.Id("password-input")).SendKeys(p ?? "");
                d.FindElement(By.CssSelector(".btn-login")).Click();
                return wait.Until(x => x.Url.ToLower().Contains("/nhaxe/banve"));
            }
            catch { return false; }
        }
        private async Task Luu(IWebDriver d, int row, string msg)
        {
            try
            {
                byte[] img = ((ITakesScreenshot)d).GetScreenshot().AsByteArray;
                string url = await _service.UploadToDriveAsync(img, $"Test_{DateTime.Now:ss}.png");
                await _service.WriteScreenshotResultAsync(row, url);
            }
            catch { }
            await _service.WriteActualResultAsync(row, msg);
        }

        private async Task LuuKetQua(IWebDriver d, string name, int row, string msg)
        {
            try
            {
                byte[] img = ((ITakesScreenshot)d).GetScreenshot().AsByteArray;
                string url = await _service.UploadToDriveAsync(img, $"{name}_{DateTime.Now:mm}.png");
                await _service.WriteScreenshotResultAsync(row, url);
            }
            catch { }
            await _service.WriteActualResultAsync(row, msg);
        }

        private async Task<(IWebDriver d, WebDriverWait w, IJavaScriptExecutor js, TestCaseModel tc)> Init(string id)
        {
            var d = _selenium.GetDriver();
            var w = new WebDriverWait(d, TimeSpan.FromSeconds(15));
            var js = (IJavaScriptExecutor)d;
            var tcs = await _service.GetTestCasesAsync();
            var tc = tcs.FirstOrDefault(x => x.TestCaseId == id);
            var log = tcs.FirstOrDefault(x => x.TestCaseId == "IV.1_DN_01");
            await Login(d, log.Steps[1].TestDataRaw, log.Steps[2].TestDataRaw);
            w.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(., 'Tra Cứu')]"))).Click();
            w.Until(ExpectedConditions.ElementIsVisible(By.Id("btnTraCuu")));
            return (d, w, js, tc);
        }

        private string GetAlert(WebDriverWait w)
        {
            try
            {
                var a = w.Until(ExpectedConditions.AlertIsPresent());
                string t = a.Text;
                a.Accept();
                return t;
            }
            catch { return ""; }
        }
        private string HandleAlert(WebDriverWait w)
        {
            try { var a = w.Until(ExpectedConditions.AlertIsPresent()); string t = a.Text; a.Accept(); return t; }
            catch { return ""; }
        }

        [TestMethod]
        public async Task TC21_TraCuuSDT()
        {
            var (d, w, js, tc) = await Init("IV.4_TC_01");
            string msg = ""; bool pass = false;
            try
            {
                string sdt = tc.Steps[0].TestDataRaw;
                if (!sdt.StartsWith("0")) sdt = "0" + sdt;
                d.FindElement(By.Id("txtMaVe")).SendKeys(sdt);
                d.FindElement(By.Id("btnTraCuu")).Click();
                pass = w.Until(x => !string.IsNullOrEmpty(x.FindElement(By.Id("lblHoTen")).Text));
                msg = pass ? tc.ExpectedResult : "Dữ liệu khách hàng không hiển thị.";
            }
            catch { msg = "Hệ thống phản hồi chậm hoặc không tìm thấy dữ liệu."; }
            await LuuKetQua(d, "TC21", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC22_TraCuuMaVe()
        {
            var (d, w, js, tc) = await Init("IV.4_TC_02");
            string msg = ""; bool pass = false;
            try
            {
                string ma = tc.Steps[0].TestDataRaw;
                d.FindElement(By.Id("txtMaVe")).SendKeys(ma);
                d.FindElement(By.Id("btnTraCuu")).Click();
                pass = w.Until(x => x.FindElement(By.Id("lblMaVe")).Text.Trim() == ma.Trim());
                msg = pass ? tc.ExpectedResult : "Mã vé hiển thị không chính xác.";
            }
            catch { msg = "Không tìm thấy thông tin vé yêu cầu."; }
            await LuuKetQua(d, "TC22", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC23_TraCuuSai()
        {
            var (d, w, js, tc) = await Init("IV.4_TC_03");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                string t = GetAlert(w);
                pass = t.Contains("không");
                msg = pass ? tc.ExpectedResult : "Hệ thống không báo lỗi khi nhập sai.";
            }
            catch { msg = "Hệ thống không hiển thị thông báo lỗi."; }
            await LuuKetQua(d, "TC23", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC24_DoiVe()
        {
            var (d, w, js, tc) = await Init("IV.5_DV_01");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                w.Until(x => !string.IsNullOrEmpty(x.FindElement(By.Id("lblHoTen")).Text));
                string date = DateTime.ParseExact(tc.Steps[1].TestDataRaw, "d/M/yyyy", null).ToString("yyyy-MM-dd");
                js.ExecuteScript($"document.getElementById('txtNgayDoi').value='{date}'; document.getElementById('txtNgayDoi').dispatchEvent(new Event('change'));");
                var s = new SelectElement(w.Until(ExpectedConditions.ElementIsVisible(By.Id("slChuyenMoi"))));
                w.Until(x => s.Options.Count > 1); s.SelectByIndex(1);
                string seat = "seat-new-" + tc.Steps[2].TestDataRaw.Trim().PadLeft(2, '0');
                w.Until(ExpectedConditions.ElementToBeClickable(By.Id(seat))).Click();
                d.FindElement(By.Id("btnXacNhanDoi")).Click();
                GetAlert(w);
                pass = GetAlert(w).Contains("thành công");
                msg = pass ? tc.ExpectedResult : "Quá trình đổi vé thất bại.";
            }
            catch { msg = "Lỗi trong quá trình thao tác đổi vé."; }
            await LuuKetQua(d, "TC24", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC25_DoiNhieuGhe()
        {
            var (d, w, js, tc) = await Init("IV.5_DV_02");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                new SelectElement(w.Until(x => x.FindElement(By.Id("slChuyenMoi")))).SelectByIndex(1);
                foreach (var g in tc.Steps[1].TestDataRaw.Split(','))
                    w.Until(x => x.FindElement(By.Id("seat-new-" + g.Trim().PadLeft(2, '0')))).Click();
                d.FindElement(By.Id("btnXacNhanDoi")).Click();
                GetAlert(w);
                pass = GetAlert(w).Contains("thành công");
                msg = pass ? tc.ExpectedResult : "Không thể đổi nhiều ghế cùng lúc.";
            }
            catch { msg = "Lỗi khi chọn nhiều ghế để đổi."; }
            await LuuKetQua(d, "TC25", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC26_NgayKhongXe()
        {
            var (d, w, js, tc) = await Init("IV.5_DV_03");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                string date = DateTime.ParseExact(tc.Steps[0].TestDataRaw, "d/M/yyyy", null).ToString("yyyy-MM-dd");
                js.ExecuteScript($"document.getElementById('txtNgayDoi').value='{date}'; document.getElementById('txtNgayDoi').dispatchEvent(new Event('change'));");
                Thread.Sleep(1500);
                pass = d.FindElement(By.Id("slChuyenMoi")).Text.Contains("không");
                msg = pass ? tc.ExpectedResult : "Hệ thống vẫn hiện chuyến dù ngày không có xe.";
            }
            catch { msg = "Lỗi khi kiểm tra lịch trình ngày xa."; }
            await LuuKetQua(d, "TC26", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC28_GheDaBan()
        {
            var (d, w, js, tc) = await Init("IV.5_DV_05");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                new SelectElement(w.Until(x => x.FindElement(By.Id("slChuyenMoi")))).SelectByIndex(1);
                var g = w.Until(x => x.FindElement(By.CssSelector("#gridGheMoi .seat.sold")));
                g.Click();
                pass = !g.GetAttribute("class").Contains("selected");
                msg = pass ? tc.ExpectedResult : "Vẫn cho phép chọn ghế đã có người mua.";
            }
            catch { msg = "Không tìm thấy ghế đã bán để kiểm tra."; }
            await LuuKetQua(d, "TC28", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC29_GheDangNgoi()
        {
            var (d, w, js, tc) = await Init("IV.5_DV_06");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                var g = w.Until(x => x.FindElement(By.CssSelector("#gridGheMoi .seat.current")));
                g.Click();
                pass = !g.GetAttribute("class").Contains("selected");
                msg = pass ? tc.ExpectedResult : "Hệ thống cho phép chọn lại chính ghế đang ngồi.";
            }
            catch { msg = "Lỗi khi kiểm tra ghế hiện tại."; }
            await LuuKetQua(d, "TC29", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC30_HuyVe()
        {
            var (d, w, js, tc) = await Init("IV.6_HV_01");
            string msg = ""; bool pass = false;
            try
            {
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                w.Until(x => x.FindElement(By.Id("btnHuyVe"))).Click();
                GetAlert(w);
                pass = GetAlert(w).Contains("thành công");
                msg = pass ? tc.ExpectedResult : "Không thể thực hiện hủy vé.";
            }
            catch { msg = "Lỗi thao tác hủy vé."; }
            await LuuKetQua(d, "TC30", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC31_HuyHaiLan()
        {
            var (d1, w1, js1, tc) = await Init("IV.6_HV_02");
            string sdt = tc.Steps[0].TestDataRaw;
            d1.FindElement(By.Id("txtMaVe")).SendKeys(sdt);
            d1.FindElement(By.Id("btnTraCuu")).Click();
            using (var d2 = new ChromeDriver())
            {
                await Login(d2, "nhanvienpt@gmail.com", "123456789@duc");
                d2.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                d2.FindElement(By.Id("txtMaVe")).SendKeys(sdt);
                d2.FindElement(By.Id("btnTraCuu")).Click();
                d1.FindElement(By.Id("btnHuyVe")).Click();
                w1.Until(ExpectedConditions.AlertIsPresent()).Accept();
                w1.Until(ExpectedConditions.AlertIsPresent()).Accept();
                d2.FindElement(By.Id("btnHuyVe")).Click();
                d2.SwitchTo().Alert().Accept();
                string t = new WebDriverWait(d2, TimeSpan.FromSeconds(5)).Until(ExpectedConditions.AlertIsPresent()).Text;
                bool pass = t.Contains("hủy") || t.Contains("tồn tại");
                string msg = pass ? tc.ExpectedResult : t;
                await LuuKetQua(d1, "TC31", tc.SpreadsheetExpectedResultRow, msg);
                Assert.IsTrue(pass, msg);
            }
        }

        [TestMethod]
        public async Task TC32_HuySatGio()
        {
            var (d, w, js, tc) = await Init("IV.6_HV_03");
            string msg = ""; bool pass = false;
            try
            {
                js.ExecuteScript($"document.getElementById('txtMaVe').value='{tc.Steps[0].TestDataRaw}';");
                d.FindElement(By.Id("btnTraCuu")).Click();
                w.Until(x => x.FindElement(By.Id("btnHuyVe"))).Click();
                GetAlert(w);
                string t = GetAlert(w);
                pass = t.Contains("trước") || t.Contains("không");
                msg = pass ? tc.ExpectedResult : t;
            }
            catch { msg = "Hệ thống vẫn cho phép hủy dù sát giờ chạy."; }
            await LuuKetQua(d, "TC32", tc.SpreadsheetExpectedResultRow, msg);
            Assert.IsTrue(pass, msg);
        }
    }
}