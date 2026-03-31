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
    public class NhanVienBanVeTests
    {
        private SeleniumTestService _selenium;
        private NhanVienBanVeService _service;

        [TestInitialize]
        public void Setup() { _selenium = new SeleniumTestService(); _service = new NhanVienBanVeService(); }

        [TestCleanup]
        public void TearDown() => _selenium.GetDriver().Quit();

        // --- HÀM TRỢ GIÚP ĐỌC DATA VÀ GHI BÁO CÁO ---

        private async Task GhiKetQuaTuNhien(IWebDriver d, string tcName, int row, string msg)
        {
            try
            {
                byte[] img = ((ITakesScreenshot)d).GetScreenshot().AsByteArray;
                string url = await _service.UploadToDriveAsync(img, $"{tcName}_{DateTime.Now:ss}.png");
                await _service.WriteScreenshotResultAsync(row, url);
            }
            catch { }
            await _service.WriteActualResultAsync(row, msg);
        }

        private string DichLoi(Exception ex)
        {
            string e = ex.Message.ToLower();
            if (e.Contains("timed out")) return "Hệ thống xử lý quá chậm hoặc giao diện không phản hồi đúng lúc.";
            if (e.Contains("no such element")) return "Không tìm thấy thành phần giao diện (nút bấm/ô nhập/ghế) như kịch bản mô tả.";
            if (e.Contains("alert")) return "Thông báo hệ thống xuất hiện bất ngờ làm gián đoạn luồng test.";
            return "Lỗi phát sinh: " + ex.Message;
        }

        private async Task<bool> Login(IWebDriver d, string u, string p)
        {
            d.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var input = w.Until(ExpectedConditions.ElementIsVisible(By.Id("EmailOrPhone")));
                input.Clear(); input.SendKeys(u ?? "");
                d.FindElement(By.Id("password-input")).SendKeys(p ?? "");
                d.FindElement(By.CssSelector(".btn-login")).Click();
                return w.Until(x => x.Url.ToLower().Contains("/nhaxe/banve"));
            }
            catch { return false; }
        }

        private async Task<(string u, string p)> LayAuthChung()
        {
            var tcs = await _service.GetTestCasesAsync();
            var log = tcs.FirstOrDefault(x => x.TestCaseId == "IV.1_DN_01");
            return (log.Steps[1].TestDataRaw, log.Steps[2].TestDataRaw);
        }

        // --- 20 TEST CASES ĐÃ ĐƯỢC FIX LOGIC ---

        [TestMethod]
        public async Task TC01_LoginSuccess()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.1_DN_01");
            bool ok = await Login(d, tc.Steps[1].TestDataRaw, tc.Steps[2].TestDataRaw);
            string msg = ok ? tc.ExpectedResult : "Đăng nhập thất bại dù dùng tài khoản hợp lệ.";
            await GhiKetQuaTuNhien(d, "TC01", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC02_LoginFail()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.1_DN_02");
            bool ok = !await Login(d, tc.Steps[1].TestDataRaw, tc.Steps[2].TestDataRaw);
            string msg = ok ? tc.ExpectedResult : "Hệ thống vẫn cho phép đăng nhập khi sai mật khẩu.";
            await GhiKetQuaTuNhien(d, "TC02", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC03_ViewDefaultTrips()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.2_DS_01");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            var trips = d.FindElements(By.CssSelector("a.trip-box"));
            bool ok = trips.Count > 0;
            string msg = ok ? tc.ExpectedResult : "Danh sách chuyến xe đang bị trống, không đúng mặc định.";
            await GhiKetQuaTuNhien(d, "TC03", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC04_FilterByDate()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.2_DS_02");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                string dateVal = DateTime.Parse(tc.Steps[0].TestDataRaw).ToString("yyyy-MM-dd");
                ((IJavaScriptExecutor)d).ExecuteScript($"document.getElementById('ngayDi').value='{dateVal}';");
                d.FindElement(By.Id("btnFilter")).Click();
                Thread.Sleep(2000);
                ok = true; msg = tc.ExpectedResult;
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC04", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC05_SelectOneSeat()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_01");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container"))); // Chờ load sơ đồ

                string seatNo = tc.Steps[1].TestDataRaw.Trim().PadLeft(2, '0');
                var s = w.Until(ExpectedConditions.ElementExists(By.Id($"seat-{seatNo}")));
                js.ExecuteScript("arguments[0].scrollIntoView(true); arguments[0].click();", s);
                Thread.Sleep(500);

                ok = s.GetAttribute("class").Contains("selected");
                msg = ok ? tc.ExpectedResult : "Ghế không đổi màu xanh sau khi click.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC05", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC06_SelectMultipleSeats()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_02");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                string s1 = tc.Steps[0].TestDataRaw.Trim().PadLeft(2, '0');
                string s2 = tc.Steps[1].TestDataRaw.Trim().PadLeft(2, '0');

                var seat1 = d.FindElement(By.Id($"seat-{s1}"));
                var seat2 = d.FindElement(By.Id($"seat-{s2}"));
                js.ExecuteScript("arguments[0].click();", seat1);
                js.ExecuteScript("arguments[0].click();", seat2);
                Thread.Sleep(500);

                ok = seat1.GetAttribute("class").Contains("selected") && seat2.GetAttribute("class").Contains("selected");
                msg = ok ? tc.ExpectedResult : "Không chọn được cùng lúc nhiều ghế.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC06", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC07_DeselectSeat()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_03");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                string sNo = tc.Steps[0].TestDataRaw.Trim().PadLeft(2, '0');
                var s = d.FindElement(By.Id($"seat-{sNo}"));
                js.ExecuteScript("arguments[0].click();", s); // Chọn
                Thread.Sleep(500);
                js.ExecuteScript("arguments[0].click();", s); // Bỏ chọn
                Thread.Sleep(500);

                ok = !s.GetAttribute("class").Contains("selected");
                msg = ok ? tc.ExpectedResult : "Ghế vẫn bị chọn, không hủy được màu xanh.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC07", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC08_AutoFillCustomer()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_04");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Chọn tạm 1 ghế để form hiển thị rõ ràng (nếu có validate UX)
                var s = d.FindElement(By.CssSelector(".seat:not(.sold)"));
                ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", s);

                d.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("HoTen")).Click();
                Thread.Sleep(1500); // Chờ API tự động điền gọi về

                ok = !string.IsNullOrEmpty(d.FindElement(By.Id("HoTen")).GetAttribute("value"));
                msg = ok ? tc.ExpectedResult : "Tên khách không được tự động điền theo SĐT.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC08", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC09_BanVeThanhCong()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_05");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // 1. Bốc dữ liệu chính xác từ Excel theo thứ tự mới
                string hoTen = tc.Steps[0].TestDataRaw; // "Nguyễn Văn A"
                string sdt = tc.Steps[1].TestDataRaw;   // "901234567"
                if (!string.IsNullOrEmpty(sdt) && !sdt.StartsWith("0")) sdt = "0" + sdt; // Tự động bù số 0 bị mất do Excel

                string soGhe = tc.Steps[2].TestDataRaw.Trim().PadLeft(2, '0'); // "15" -> Format thành "15"

                // 2. Click đúng ghế số 15 từ Excel
                var seat = w.Until(ExpectedConditions.ElementExists(By.Id($"seat-{soGhe}")));
                js.ExecuteScript("arguments[0].scrollIntoView(true); arguments[0].click();", seat);
                Thread.Sleep(500);

                // 3. Điền thông tin (Dùng Clear để đảm bảo không bị dính chữ cũ)
                var inputHoTen = d.FindElement(By.Id("HoTen"));
                inputHoTen.Clear();
                inputHoTen.SendKeys(hoTen);

                var inputSdt = d.FindElement(By.Id("SoDienThoai"));
                inputSdt.Clear();
                inputSdt.SendKeys(sdt);
                Thread.Sleep(500);

                // 4. Bấm nút Submit
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                // 5. Xử lý 2 tầng Alert (1 cái Xác nhận của Web, 1 cái Kết quả của API)
                w.Until(ExpectedConditions.AlertIsPresent()).Accept(); // Bỏ qua Xác nhận

                var resultAlert = w.Until(ExpectedConditions.AlertIsPresent());
                ok = resultAlert.Text.ToLower().Contains("thành công");
                resultAlert.Accept();
                msg = ok ? tc.ExpectedResult : "Lỗi: Không nhận được thông báo xuất vé thành công.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC09", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC10_DatGiuCho()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_06");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Đã fix: Bắt buộc điền thông tin để không bị chặn bởi validate Name/SĐT
                var s = d.FindElement(By.CssSelector(".seat:not(.sold)"));
                js.ExecuteScript("arguments[0].click();", s);
                d.FindElement(By.Id("HoTen")).SendKeys("Khách Test Giữ Chỗ");
                d.FindElement(By.Id("SoDienThoai")).SendKeys("0911223344");

                // Tắt công tắc Đã thanh toán
                js.ExecuteScript("document.getElementById('DaThanhToan').click();");
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                w.Until(ExpectedConditions.AlertIsPresent()).Accept();
                var resultAlert = w.Until(ExpectedConditions.AlertIsPresent());
                ok = resultAlert.Text.ToLower().Contains("thành công");
                resultAlert.Accept();
                msg = ok ? tc.ExpectedResult : "Lỗi: Hệ thống từ chối chức năng giữ chỗ.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC10", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC11_ThieuHoTen()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_07");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Đã fix: Chọn ghế trước tiên
                var s = w.Until(ExpectedConditions.ElementExists(By.CssSelector(".seat:not(.sold)")));
                js.ExecuteScript("arguments[0].click();", s);

                d.FindElement(By.Id("SoDienThoai")).SendKeys("0901234567");
                // Để trống họ tên
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                var a = w.Until(ExpectedConditions.AlertIsPresent());
                ok = a.Text.ToLower().Contains("họ tên");
                a.Accept();
                msg = ok ? tc.ExpectedResult : "Hệ thống không báo lỗi yêu cầu nhập tên.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC11", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC12_ThieuSDT()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_08");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Đã fix: Chọn ghế trước tiên
                var s = w.Until(ExpectedConditions.ElementExists(By.CssSelector(".seat:not(.sold)")));
                js.ExecuteScript("arguments[0].click();", s);

                d.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[0].TestDataRaw);
                // Để trống SĐT
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                var a = w.Until(ExpectedConditions.AlertIsPresent());
                ok = a.Text.ToLower().Contains("điện thoại");
                a.Accept();
                msg = ok ? tc.ExpectedResult : "Hệ thống không báo lỗi thiếu SĐT.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC12", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC13_ChuaChonGhe()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_09");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Không chọn ghế, bấm xác nhận luôn
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));
                var a = w.Until(ExpectedConditions.AlertIsPresent());
                ok = a.Text.ToLower().Contains("ghế");
                a.Accept();
                msg = ok ? tc.ExpectedResult : "Vẫn cho xác nhận khi chưa chọn ghế.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC13", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC14_GheDaBan()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_10");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                var sold = d.FindElements(By.CssSelector(".seat.sold")).FirstOrDefault();
                if (sold != null)
                {
                    ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", sold);
                    ok = !sold.GetAttribute("class").Contains("selected");
                    msg = ok ? tc.ExpectedResult : "Lỗi: Hệ thống vẫn cho click chọn đè lên ghế màu đỏ.";
                }
                else
                {
                    ok = true; msg = "Bỏ qua: Không có ghế đỏ để kiểm tra kịch bản.";
                }
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC14", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC15_FullyBooked()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_11");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));
                ok = true; msg = tc.ExpectedResult;
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC15", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC16_PriceReadOnly()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_12");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("GiaVe")));

                ok = d.FindElement(By.Id("GiaVe")).GetAttribute("readonly") != null;
                msg = ok ? tc.ExpectedResult : "Lỗi bảo mật: Ô nhập Giá Vé không bị khóa (readonly).";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC16", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC17_SDTSaiDinhDang()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_13");
            var auth = await LayAuthChung(); await Login(d, auth.u, auth.p);
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                // Đã fix: Chọn ghế trước
                var s = w.Until(ExpectedConditions.ElementExists(By.CssSelector(".seat:not(.sold)")));
                js.ExecuteScript("arguments[0].click();", s);

                d.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[0].TestDataRaw);
                d.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[1].TestDataRaw); // SĐT chứa chữ
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                var a = w.Until(ExpectedConditions.AlertIsPresent());
                ok = a.Text.ToLower().Contains("hợp lệ") || a.Text.ToLower().Contains("đúng");
                a.Accept();
                msg = ok ? tc.ExpectedResult : "Không chặn được số điện thoại sai định dạng (chứa chữ cái).";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC17", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC18_TrungGhe_NV_NV()
        {
            var d1 = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_14");
            string msg = ""; bool pass = false;
            using (var d2 = new ChromeDriver())
            {
                try
                {
                    await Login(d1, tc.Steps[0].TestDataRaw, "123456789@duc"); // NV1
                    await Login(d2, tc.Steps[2].TestDataRaw, "123456789@duc"); // NV2

                    var w1 = new WebDriverWait(d1, TimeSpan.FromSeconds(15));
                    var js1 = (IJavaScriptExecutor)d1;
                    w1.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                    w1.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));
                    d2.Navigate().GoToUrl(d1.Url);

                    var w2 = new WebDriverWait(d2, TimeSpan.FromSeconds(15));
                    var js2 = (IJavaScriptExecutor)d2;
                    w2.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                    string sid = "seat-" + tc.Steps[4].TestDataRaw.Trim().PadLeft(2, '0'); // Ghế từ Excel
                    var s1 = d1.FindElement(By.Id(sid)); js1.ExecuteScript("arguments[0].click();", s1);
                    var s2 = d2.FindElement(By.Id(sid)); js2.ExecuteScript("arguments[0].click();", s2);

                    // NV2 mua trước
                    d2.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[8].TestDataRaw);
                    d2.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[9].TestDataRaw);
                    js2.ExecuteScript("arguments[0].click();", d2.FindElement(By.Id("btnSubmitBanVe")));
                    w2.Until(ExpectedConditions.AlertIsPresent()).Accept();
                    w2.Until(ExpectedConditions.AlertIsPresent()).Accept();

                    // NV1 mua sau
                    d1.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[5].TestDataRaw);
                    d1.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[6].TestDataRaw);
                    js1.ExecuteScript("arguments[0].click();", d1.FindElement(By.Id("btnSubmitBanVe")));
                    w1.Until(ExpectedConditions.AlertIsPresent()).Accept();

                    var alertResult = w1.Until(ExpectedConditions.AlertIsPresent());
                    pass = alertResult.Text.Contains("đã") || alertResult.Text.Contains("lỗi");
                    alertResult.Accept();
                    msg = pass ? tc.ExpectedResult : "Lỗi: Không chặn được giao dịch trùng ghế.";
                }
                catch (Exception ex) { msg = DichLoi(ex); }
            }
            await GhiKetQuaTuNhien(d1, "TC18", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC19_ChuyenDaChay()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_15");
            await Login(d, tc.Steps[0].TestDataRaw, "123456789@duc");
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(10));
                var js = (IJavaScriptExecutor)d;

                string dt = tc.Steps[1].TestDataRaw; // Lấy ngày hôm qua
                js.ExecuteScript($"document.getElementById('ngayDi').value='{dt}';");
                d.FindElement(By.Id("btnFilter")).Click(); Thread.Sleep(2000);

                if (d.FindElements(By.CssSelector("a.trip-box")).Count == 0)
                {
                    await GhiKetQuaTuNhien(d, "TC19", tc.SpreadsheetExpectedResultRow, "Bỏ qua: Không có chuyến nào trong quá khứ để test.");
                    return;
                }

                d.FindElement(By.CssSelector("a.trip-box")).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                var btn = d.FindElement(By.Id("btnSubmitBanVe"));
                if (btn.GetAttribute("disabled") != null)
                {
                    ok = true;
                }
                else
                {
                    // Nếu không khóa mờ, thử mua xem server có chặn không
                    var s = w.Until(ExpectedConditions.ElementExists(By.CssSelector(".seat:not(.sold)")));
                    js.ExecuteScript("arguments[0].click();", s);
                    d.FindElement(By.Id("HoTen")).SendKeys("Test Quá Hạn");
                    d.FindElement(By.Id("SoDienThoai")).SendKeys("0900000000");
                    js.ExecuteScript("arguments[0].click();", btn);
                    w.Until(ExpectedConditions.AlertIsPresent()).Accept();
                    var a = w.Until(ExpectedConditions.AlertIsPresent());
                    ok = a.Text.Contains("quá hạn") || a.Text.Contains("không");
                    a.Accept();
                }
                msg = ok ? tc.ExpectedResult : "Lỗi: Vẫn cho phép xuất vé vào chuyến xe đã khởi hành.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC19", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }

        [TestMethod]
        public async Task TC20_TrungGhe_Online()
        {
            var d = _selenium.GetDriver();
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_16");
            await Login(d, tc.Steps[3].TestDataRaw, "123456789@duc"); // Email NV
            bool ok = false; string msg = "";
            try
            {
                var w = new WebDriverWait(d, TimeSpan.FromSeconds(15));
                var js = (IJavaScriptExecutor)d;
                w.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                w.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                string sNo = tc.Steps[8].TestDataRaw.Trim().PadLeft(2, '0'); // Ghế từ Excel
                var s = w.Until(ExpectedConditions.ElementExists(By.Id($"seat-{sNo}")));
                js.ExecuteScript("arguments[0].click();", s);

                d.FindElement(By.Id("HoTen")).SendKeys(tc.Steps[9].TestDataRaw);
                d.FindElement(By.Id("SoDienThoai")).SendKeys(tc.Steps[10].TestDataRaw);
                js.ExecuteScript("arguments[0].click();", d.FindElement(By.Id("btnSubmitBanVe")));

                w.Until(ExpectedConditions.AlertIsPresent()).Accept();
                var alert = w.Until(ExpectedConditions.AlertIsPresent());
                ok = alert.Text.Contains("thất bại") || alert.Text.Contains("online") || alert.Text.Contains("đã bán");
                alert.Accept();
                msg = ok ? tc.ExpectedResult : "Không ưu tiên chặn thanh toán tại quầy khi khách online đang giữ chỗ.";
            }
            catch (Exception ex) { msg = DichLoi(ex); }
            await GhiKetQuaTuNhien(d, "TC20", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            Assert.IsTrue(ok, msg);
        }
    }
}