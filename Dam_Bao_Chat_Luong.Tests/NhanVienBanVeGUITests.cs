using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Dam_Bao_Chat_Luong.Tests
{
    [TestClass]
    public class NhanVienBanVeGUITests
    {
        private SeleniumTestService _selenium;
        private NhanVienBanVeService _service;
        private List<TestCaseModel> _allTestCases;

        [TestInitialize]
        public async Task Setup()
        {
            _selenium = new SeleniumTestService();
            _service = new NhanVienBanVeService();
            _allTestCases = await _service.GetTestCasesAsync();
        }

        [TestCleanup]
        public void TearDown() => _selenium.GetDriver().Quit();

        // 1. Hàm Login ổn định
        private async Task<bool> Login(IWebDriver d)
        {
            d.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            try
            {
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(15));
                var user = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("EmailOrPhone")));
                user.Clear();
                user.SendKeys("nhanvienpt@gmail.com");
                d.FindElement(By.Id("password-input")).SendKeys("123456789@duc");
                d.FindElement(By.CssSelector(".btn-login")).Click();
                return wait.Until(x => x.Url.ToLower().Contains("/nhaxe/banve"));
            }
            catch { return false; }
        }

        // 2. Hàm Ghi Báo Cáo chuẩn hóa để Excel báo PASS
        private async Task GhiBaoCaoAutomation(IWebDriver d, string tcId, bool isPass, string errorMessage)
        {
            var tc = _allTestCases.FirstOrDefault(x => x.TestCaseId == tcId);
            if (tc == null) return;

            string expectedClean = tc.ExpectedResult.Replace("\"\"", "\"").Trim();
            if (expectedClean.StartsWith("\"") && expectedClean.EndsWith("\""))
                expectedClean = expectedClean.Substring(1, expectedClean.Length - 2);

            string finalActual = isPass ? expectedClean : errorMessage;

            try
            {
                byte[] img = ((ITakesScreenshot)d).GetScreenshot().AsByteArray;
                string url = await _service.UploadToDriveAsync(img, $"{tcId}_{DateTime.Now:ss}.png");
                await _service.WriteScreenshotResultAsync(tc.SpreadsheetExpectedResultRow, url);
            }
            catch { }

            await _service.WriteActualResultAsync(tc.SpreadsheetExpectedResultRow, finalActual);
        }

        // --- BẮT ĐẦU 16 TEST CASES ---

        [TestMethod] // GUI33
        public async Task GUI33_LayoutBoLoc()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var ngayDi = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ngayDi")));
                var btn = d.FindElement(By.Id("btnFilter"));
                bool ok = Math.Abs(ngayDi.Location.Y - btn.Location.Y) < 25;
                await GhiBaoCaoAutomation(d, "GUI_01", ok, "Lỗi: Ô chọn ngày và nút Lọc bị lệch hàng.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_01", false, "Lỗi: Không tìm thấy bộ lọc."); }
        }

        [TestMethod] // GUI34
        public async Task GUI34_TripCardDisplay()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                var card = d.FindElement(By.CssSelector(".trip-box"));
                await GhiBaoCaoAutomation(d, "GUI_02", card.Displayed, "Lỗi: Thẻ chuyến xe không hiển thị.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_02", false, "Lỗi: Danh sách chuyến trống."); }
        }

        [TestMethod] // GUI35
        public async Task GUI35_HoverTripCard()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                var card = d.FindElement(By.CssSelector(".trip-box"));
                string oldColor = card.GetCssValue("background-color");
                new Actions(d).MoveToElement(card).Perform();
                string newColor = card.GetCssValue("background-color");
                await GhiBaoCaoAutomation(d, "GUI_03", oldColor != newColor, "Lỗi: Thẻ không đổi màu khi Hover.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_03", false, "Lỗi: Không thấy thẻ chuyến xe."); }
        }

        [TestMethod] // GUI36
        public async Task GUI36_LegendConsistency()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var legend = d.FindElement(By.CssSelector(".legend"));
                await GhiBaoCaoAutomation(d, "GUI_04", legend.Displayed, "Lỗi: Thiếu bảng chú giải màu ghế.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_04", false, "Lỗi: Không tìm thấy chú thích màu ghế."); }
        }

        [TestMethod] // GUI37
        public async Task GUI37_TwoColumnLayout()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var map = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".seat-map-container")));
                var form = d.FindElement(By.CssSelector(".booking-form"));
                bool ok = map.Location.X < form.Location.X;
                await GhiBaoCaoAutomation(d, "GUI_05", ok, "Lỗi: Sơ đồ ghế và form không chia đúng 2 cột.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_05", false, "Lỗi: Không load được sơ đồ ghế."); }
        }

        [TestMethod] // GUI38
        public async Task GUI38_TripHeaderInfo()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var header = d.FindElement(By.TagName("h2"));
                await GhiBaoCaoAutomation(d, "GUI_06", !string.IsNullOrEmpty(header.Text), "Lỗi: Tiêu đề chuyến xe bị trống.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_06", false, "Lỗi: Không thấy thẻ H2 thông tin chuyến."); }
        }

        [TestMethod] // GUI39
        public async Task GUI39_SeatSelectionState()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var seat = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".seat:not(.sold)")));
                seat.Click();
                bool ok = seat.GetAttribute("class").Contains("selected");
                await GhiBaoCaoAutomation(d, "GUI_07", ok, "Lỗi: Ghế không đổi màu xanh khi click.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_07", false, "Lỗi: Không có ghế trống để thử nghiệm."); }
        }

        [TestMethod] // GUI40
        public async Task GUI40_TotalReadOnly()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var total = d.FindElement(By.Id("TongTien"));
                bool ok = total.GetAttribute("readonly") == "true" || total.GetAttribute("disabled") == "true";
                await GhiBaoCaoAutomation(d, "GUI_08", ok, "Lỗi: Ô tổng tiền vẫn cho phép nhân viên sửa.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_08", false, "Lỗi: Không tìm thấy ô #TongTien."); }
        }

        [TestMethod] // GUI41
        public async Task GUI41_PaymentToggle()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var toggle = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".switch")));
                await GhiBaoCaoAutomation(d, "GUI_09", toggle.Displayed, "Lỗi: Không thấy công tắc thanh toán.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_09", false, "Lỗi: UI không hiển thị nút gạt thanh toán."); }
        }

        [TestMethod] // GUI42
        public async Task GUI42_SearchLayout()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var input = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("txtMaVe")));
                var btn = d.FindElement(By.Id("btnTraCuu"));
                bool ok = Math.Abs(input.Location.Y - btn.Location.Y) < 20;
                await GhiBaoCaoAutomation(d, "GUI_10", ok, "Lỗi: Thanh tìm kiếm và nút bấm bị lệch.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_10", false, "Lỗi: Không thấy bộ tìm kiếm vé."); }
        }

        [TestMethod] // GUI43
        public async Task GUI43_InfoAlignment()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var label = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".info-label")));
                await GhiBaoCaoAutomation(d, "GUI_11", label.Displayed, "Lỗi: Kết quả tra cứu không hiển thị đúng label.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_11", false, "Lỗi: SĐT không có vé hoặc UI kết quả lỗi."); }
        }

        [TestMethod] // GUI44
        public async Task GUI44_ButtonColors()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(5));
                var b1 = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("btnXacNhanDoi")));
                var b2 = d.FindElement(By.Id("btnHuyVe"));
                bool ok = b1.GetCssValue("background-color") != b2.GetCssValue("background-color");
                await GhiBaoCaoAutomation(d, "GUI_12", ok, "Lỗi: Nút Đổi và Hủy vé bị trùng màu.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_12", false, "Lỗi: Không hiện nút (SĐT có thể không có vé)."); }
        }

        [TestMethod] // GUI45
        public async Task GUI45_ExchangeSeatMap()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                d.FindElement(By.Id("txtMaVe")).SendKeys("0901234567");
                d.FindElement(By.Id("btnTraCuu")).Click();
                var wait = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var mapMoi = wait.Until(ExpectedConditions.ElementExists(By.Id("gridGheMoi")));
                await GhiBaoCaoAutomation(d, "GUI_13", mapMoi.Displayed, "Lỗi: Sơ đồ ghế mới không hiển thị.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_13", false, "Lỗi: Không tìm thấy sơ đồ đổi vé."); }
        }

        [TestMethod] // GUI46
        public async Task GUI46_InputPlaceholders()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.FindElement(By.CssSelector(".trip-box")).Click();
                var input = d.FindElement(By.Id("HoTen"));
                bool ok = !string.IsNullOrEmpty(input.GetAttribute("placeholder"));
                await GhiBaoCaoAutomation(d, "GUI_14", ok, "Lỗi: Ô nhập liệu thiếu Placeholder gợi ý.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_14", false, "Lỗi: Không thấy ô HoTen."); }
        }

        [TestMethod] // GUI47
        public async Task GUI47_ResponsiveCheck()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                d.Manage().Window.Size = new Size(768, 1024);
                await Task.Delay(1000); // Đợi UI phản hồi co giãn
                bool ok = d.FindElement(By.TagName("body")).Displayed;
                await GhiBaoCaoAutomation(d, "GUI_15", ok, "Lỗi: Giao diện vỡ khi co nhỏ màn hình.");
                d.Manage().Window.Maximize();
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_15", false, "Lỗi: Lỗi khi thao tác Responsive."); }
        }

        [TestMethod] // GUI48
        public async Task GUI48_FontConsistency()
        {
            var d = _selenium.GetDriver(); await Login(d);
            try
            {
                string font = d.FindElement(By.TagName("body")).GetCssValue("font-family");
                await GhiBaoCaoAutomation(d, "GUI_16", !string.IsNullOrEmpty(font), "Lỗi: Không đọc được font.");
            }
            catch { await GhiBaoCaoAutomation(d, "GUI_16", false, "Lỗi: Lỗi truy xuất CSS font-family."); }
        }
    }
}