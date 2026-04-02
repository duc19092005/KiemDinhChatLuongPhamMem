using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace Dam_Bao_Chat_Luong.Tests
{
    [TestClass]
    public class NhanVienBanVeFullFlowTests
    {
        private SeleniumTestService _selenium;
        private NhanVienBanVeService _service;

        [TestInitialize]
        public void Setup() { _selenium = new SeleniumTestService(); _service = new NhanVienBanVeService(); }

        [TestCleanup]
        public void TearDown() => _selenium.GetDriver().Quit();

        private string ChuanHoaNgay(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";
            string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd", "d/M/yyyy" };
            if (DateTime.TryParseExact(input.Trim().Split(' ')[0], formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                return dt.ToString("yyyy-MM-dd");
            return input;
        }

        private void ClickJS(IWebDriver d, IWebElement el) => ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", el);

        [TestMethod]
        public async Task IV7_E2E_01_Dat_Doi_Huy_Bat_Bug_Huy_Ve()
        {
            var d = _selenium.GetDriver();
            var wait = new WebDriverWait(d, TimeSpan.FromSeconds(20));
            var js = (IJavaScriptExecutor)d;

            var allTCs = await _service.GetTestCasesAsync();
            var tc = allTCs.FirstOrDefault(x => x.TestCaseId == "IV.7_E2E_01");
            var tcLogin = allTCs.FirstOrDefault(x => x.TestCaseId == "IV.1_DN_01");

            if (tc == null || tcLogin == null) return;

            try
            {
                // 1. ĐĂNG NHẬP
                d.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("EmailOrPhone"))).SendKeys(tcLogin.Steps[1].TestDataRaw);
                d.FindElement(By.Id("password-input")).SendKeys(tcLogin.Steps[2].TestDataRaw);
                d.FindElement(By.CssSelector(".btn-login")).Click();
                await Task.Delay(2000);

                // 2. ĐẶT VÉ
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe");
                var inputNgay = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("ngayDi")));
                js.ExecuteScript("arguments[0].value = arguments[1];", inputNgay, ChuanHoaNgay(tc.Steps[0].TestDataRaw));
                d.FindElement(By.Id("btnFilter")).Click();
                await Task.Delay(2000);

                string gioDi = tc.Steps[2].TestDataRaw.Substring(0, 5);
                var trip = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath($"//a[contains(@class,'trip-box') and .//span[contains(text(),'{gioDi}')]]")));
                ClickJS(d, trip);
                await Task.Delay(3000);

                try
                {
                    var reNgay = d.FindElement(By.Id("ngayDi"));
                    js.ExecuteScript("arguments[0].value = arguments[1];", reNgay, ChuanHoaNgay(tc.Steps[3].TestDataRaw));
                    js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", reNgay);
                }
                catch { d.Navigate().Refresh(); }
                await Task.Delay(3000);

                string soGhe = tc.Steps[5].TestDataRaw;
                var seat = wait.Until(ExpectedConditions.ElementExists(By.Id($"seat-{soGhe.PadLeft(2, '0')}")));
                ClickJS(d, seat);
                d.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[6].TestDataRaw);
                d.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[7].TestDataRaw);

                ClickJS(d, d.FindElement(By.Id("btnSubmitBanVe")));
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(1500);
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(2000);

                // 3. ĐỔI VÉ
                d.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/BanVe/TraCuu");
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("txtMaVe"))).SendKeys(tc.Steps[9].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id("lblHoTen")));
                await Task.Delay(1000);

                var inputNgayMoi = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("txtNgayDoi")));
                js.ExecuteScript("arguments[0].value = arguments[1];", inputNgayMoi, ChuanHoaNgay(tc.Steps[10].TestDataRaw));
                js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", inputNgayMoi);

                wait.Until(driver => new SelectElement(driver.FindElement(By.Id("slChuyenMoi"))).Options.Count > 1);
                await Task.Delay(1000);

                var slChuyen = new SelectElement(d.FindElement(By.Id("slChuyenMoi")));
                string gioMoi = tc.Steps[11].TestDataRaw.Substring(0, 5);
                var targetOption = slChuyen.Options.FirstOrDefault(o => o.Text.Contains(gioMoi));
                if (targetOption != null) slChuyen.SelectByText(targetOption.Text);

                js.ExecuteScript("document.getElementById('slChuyenMoi').dispatchEvent(new Event('change'));");
                await Task.Delay(5000);

                string gheMoi = tc.Steps[13].TestDataRaw.PadLeft(2, '0');
                var seatMoi = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector($"#gridGheMoi .seat-new-{gheMoi}, #gridGheMoi #seat-new-{gheMoi}")));
                ClickJS(d, seatMoi);

                d.FindElement(By.Id("btnXacNhanDoi")).Click();
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(1500);
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(2000);

                // 4. KIỂM TRA LỖI HỦY VÉ
                // Hủy lần 1
                d.FindElement(By.Id("txtMaVe")).Clear();
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[15].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                await Task.Delay(2000);

                var btnHuy = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("btnHuyVe")));
                ClickJS(d, btnHuy);
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(1500);
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(3000);

                // Tra cứu lại để xem vé đã mất chưa
                d.FindElement(By.Id("txtMaVe")).Clear();
                d.FindElement(By.Id("txtMaVe")).SendKeys(tc.Steps[15].TestDataRaw);
                d.FindElement(By.Id("btnTraCuu")).Click();
                await Task.Delay(2000);

                byte[] bugScreenshot = ((ITakesScreenshot)d).GetScreenshot().AsByteArray;

                var btnHuyKiemChung = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("btnHuyVe")));
                ClickJS(d, btnHuyKiemChung);
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();
                await Task.Delay(1500);

                string checkMsg = wait.Until(ExpectedConditions.AlertIsPresent()).Text;
                wait.Until(ExpectedConditions.AlertIsPresent()).Accept();

                bool isPass = false;
                string actualResultMsg = "";

                if (checkMsg.Contains("thành công"))
                {
                    actualResultMsg = "Lỗi hệ thống: Báo hủy vé thành công nhưng vé vẫn tồn tại khi tra cứu lại.";
                }
                else
                {
                    isPass = true;
                    actualResultMsg = tc.Steps[16].ExpectedResult.Replace("\"", "").Trim();
                }

                await GhiBaoCaoVaChupAnh(tc, actualResultMsg, bugScreenshot);
                Assert.IsTrue(isPass, actualResultMsg);
            }
            catch (Exception ex)
            {
                byte[] errImg = null;
                try { errImg = ((ITakesScreenshot)d).GetScreenshot().AsByteArray; } catch { }
                await GhiBaoCaoVaChupAnh(tc, "Gãy luồng tại lỗi: " + ex.Message, errImg);
                throw;
            }
        }

        private async Task GhiBaoCaoVaChupAnh(TestCaseModel tc, string message, byte[] imageBytes)
        {
            if (tc == null) return;
            try
            {
                if (imageBytes != null)
                {
                    string url = await _service.UploadToDriveAsync(imageBytes, $"Bug_HuyVe_{DateTime.Now:HHmmss}.png");
                    await _service.WriteScreenshotResultAsync(tc.SpreadsheetExpectedResultRow, url);
                }
            }
            catch { }

            await _service.WriteActualResultAsync(tc.SpreadsheetExpectedResultRow, message);
        }
    }
}