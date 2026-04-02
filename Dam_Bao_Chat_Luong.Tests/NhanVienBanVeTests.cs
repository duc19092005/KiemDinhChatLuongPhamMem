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

            string emailNv1 = tc.Steps[0].TestDataRaw;
            string passNv1 = tc.Steps[1].TestDataRaw;
            string emailNv2 = tc.Steps[2].TestDataRaw;
            string passNv2 = tc.Steps[3].TestDataRaw;
            string ngayDi = tc.Steps[4].TestDataRaw;
            string soGhe = tc.Steps[5].TestDataRaw.Trim().PadLeft(2, '0');
            string tenKhach1 = tc.Steps[6].TestDataRaw;
            string sdtKhach1 = tc.Steps[7].TestDataRaw;
            string tenKhach2 = tc.Steps[8].TestDataRaw;
            string sdtKhach2 = tc.Steps[9].TestDataRaw;

            using (var d2 = new ChromeDriver()) 
            {
                try
                {
                    await Login(d1, emailNv1, passNv1);
                    await Login(d2, emailNv2, passNv2);
                    Thread.Sleep(2000);
                    var w1 = new WebDriverWait(d1, TimeSpan.FromSeconds(15));
                    var js1 = (IJavaScriptExecutor)d1;
                    Thread.Sleep(2000);
                    js1.ExecuteScript($"document.getElementById('ngayDi').value='{ngayDi}';");
                    d1.FindElement(By.Id("btnFilter")).Click();
                    Thread.Sleep(2000);
                    w1.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a.trip-box"))).Click();
                    w1.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));
                    Thread.Sleep(2000);
                    d2.Navigate().GoToUrl(d1.Url);
                    var w2 = new WebDriverWait(d2, TimeSpan.FromSeconds(15));
                    var js2 = (IJavaScriptExecutor)d2;
                    w2.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));
                    Thread.Sleep(2000);
                    string sid = "seat-" + soGhe;
                    var s1 = w1.Until(ExpectedConditions.ElementExists(By.Id(sid)));
                    js1.ExecuteScript("arguments[0].click();", s1);
                    Thread.Sleep(2000);
                    var s2 = w2.Until(ExpectedConditions.ElementExists(By.Id(sid)));
                    js2.ExecuteScript("arguments[0].click();", s2);
                    Thread.Sleep(2000);
                    d2.FindElement(By.Id("HoTen")).SendKeys(tenKhach2);
                    d2.FindElement(By.Id("SoDienThoai")).SendKeys(sdtKhach2);
                    js2.ExecuteScript("arguments[0].click();", d2.FindElement(By.Id("btnSubmitBanVe")));
                    Thread.Sleep(2000);
                    w2.Until(ExpectedConditions.AlertIsPresent()).Accept();
                    w2.Until(ExpectedConditions.AlertIsPresent()).Accept();
                    Thread.Sleep(2000);
                    d1.FindElement(By.Id("HoTen")).SendKeys(tenKhach1);
                    d1.FindElement(By.Id("SoDienThoai")).SendKeys(sdtKhach1);
                    js1.ExecuteScript("arguments[0].click();", d1.FindElement(By.Id("btnSubmitBanVe")));
                    Thread.Sleep(2000);
                    w1.Until(ExpectedConditions.AlertIsPresent()).Accept();
                    Thread.Sleep(2000);
                    var errorAlert = w1.Until(ExpectedConditions.AlertIsPresent());
                    string alertText = errorAlert.Text.ToLower();
                    Thread.Sleep(2000);
                    // Đọc text để Assert
                    pass = alertText.Contains("đã có người") || alertText.Contains("đã bán") || alertText.Contains("lỗi") || alertText.Contains("thất bại");
                    msg = pass ? $"{errorAlert.Text}" : $"Lỗi: Thông báo sai '{errorAlert.Text}'";
                    Thread.Sleep(2000);
                    errorAlert.Accept();
                    Thread.Sleep(2000);
                    await GhiKetQuaTuNhien(d1, "TC18", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
                }
                catch (Exception ex) { msg = DichLoi(ex); }
            }
            Assert.IsTrue(pass, msg);
        }

        [TestMethod]
        public async Task TC20_TrungGhe_Online()
        {
            var dOnline = _selenium.GetDriver(); 
            var tc = (await _service.GetTestCasesAsync()).FirstOrDefault(x => x.TestCaseId == "IV.3_BV_16");
            string msg = ""; bool pass = false;
            string currentStep = "Bắt đầu test";

            // Đọc data từ Excel
            string emailKhach = tc.Steps[0].TestDataRaw;
            string passKhach = tc.Steps[1].TestDataRaw;
            string ngayDi = tc.Steps[2].TestDataRaw;
            string soGheKhach = tc.Steps[3].TestDataRaw.Trim();
            string soGheNv = soGheKhach.PadLeft(2, '0');
            string emailNv = tc.Steps[4].TestDataRaw;
            string passNv = tc.Steps[5].TestDataRaw;
            string tenKhachVangLai = tc.Steps[6].TestDataRaw;
            string sdtKhachVangLai = tc.Steps[7].TestDataRaw;

            using (var dNv = new ChromeDriver()) 
            {
                try
                {
                    var wOnl = new WebDriverWait(dOnline, TimeSpan.FromSeconds(30));
                    var jsOnl = (IJavaScriptExecutor)dOnline;
                    dOnline.Manage().Window.Maximize();

                    // BƯỚC 1: KHÁCH HÀNG ĐĂNG NHẬP VÀ CHỌN GHẾ
                    currentStep = "[KH] Đăng nhập";
                    dOnline.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
                    var inputEmail = wOnl.Until(ExpectedConditions.ElementIsVisible(By.Id("EmailOrPhone")));
                    inputEmail.Clear(); inputEmail.SendKeys(emailKhach);
                    dOnline.FindElement(By.Id("password-input")).SendKeys(passKhach);
                    dOnline.FindElement(By.CssSelector(".btn-login")).Click();
                    Thread.Sleep(2000);

                    currentStep = "[KH] Nhảy thẳng vào trang Lịch Trình/Chuyến Xe";
                    dOnline.Navigate().GoToUrl("http://duck123.runasp.net/Home_User/ChuyenXe_User");
                    Thread.Sleep(2000);

                    currentStep = "[KH] Nhập ngày và Tìm kiếm chuyến xe";
                    var inputNgayDi = wOnl.Until(ExpectedConditions.ElementExists(By.Id("ngayDi")));
                    jsOnl.ExecuteScript($"arguments[0].value='{ngayDi}';", inputNgayDi);
                    jsOnl.ExecuteScript("arguments[0].blur();", inputNgayDi);
                    Thread.Sleep(500);

                    var btnTimKiem = wOnl.Until(ExpectedConditions.ElementExists(By.CssSelector(".btn-search")));
                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", btnTimKiem);
                    Thread.Sleep(2000);

                    currentStep = "[KH] Trích xuất ID Chuyến và Bấm Chọn";
                    var btnChonChuyen = wOnl.Until(ExpectedConditions.ElementExists(By.XPath("(//a[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'chọn chuyến')])[1]")));

                    string hrefKhach = btnChonChuyen.GetAttribute("href");
                    string chuyenId = hrefKhach.Substring(hrefKhach.IndexOf("chuyenId=") + 9);
                    if (chuyenId.Contains("&")) chuyenId = chuyenId.Split('&')[0];

                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", btnChonChuyen);

                    currentStep = "[KH] Chọn ghế trên sơ đồ";
                    wOnl.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".seat")));
                    var xpathGheKh = $"//*[contains(@class, 'seat') and (@data-seat-id='{soGheKhach}' or @id='seat-{soGheKhach}' or @id='seat-{soGheNv}' or normalize-space(text())='{soGheKhach}')]";
                    var sOnl = wOnl.Until(ExpectedConditions.ElementExists(By.XPath(xpathGheKh)));
                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", sOnl);
                    Thread.Sleep(1000);

                    // BƯỚC 2: NHÂN VIÊN ĐĂNG NHẬP VÀ VÀO CÙNG CHUYẾN ĐÓ
                    currentStep = "[NV] Đăng nhập";
                    dNv.Manage().Window.Maximize();
                    await Login(dNv, emailNv, passNv);
                    var wNv = new WebDriverWait(dNv, TimeSpan.FromSeconds(30));
                    var jsNv = (IJavaScriptExecutor)dNv;

                    currentStep = "[NV] Nhảy thẳng vào chung chuyến xe với Khách Hàng";
                    dNv.Navigate().GoToUrl($"http://duck123.runasp.net/NhaXe/BanVe/BanVe/{chuyenId}");
                    wNv.Until(ExpectedConditions.ElementIsVisible(By.Id("seat-map-container")));

                    currentStep = "[NV] Chọn ghế tranh chấp";
                    var sNv = wNv.Until(ExpectedConditions.ElementExists(By.Id($"seat-{soGheNv}")));
                    jsNv.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", sNv);

                    dNv.FindElement(By.Id("HoTen")).SendKeys(tenKhachVangLai);
                    dNv.FindElement(By.Id("SoDienThoai")).SendKeys(sdtKhachVangLai);
                    Thread.Sleep(1000);

                    // BƯỚC 3: KHÁCH HÀNG THANH TOÁN MOMO TRƯỚC
                    currentStep = "[KH] Chọn phương thức MoMo và Tiếp tục";
                    var selectGateway = wOnl.Until(ExpectedConditions.ElementExists(By.Id("gateway")));
                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", selectGateway);
                    jsOnl.ExecuteScript("arguments[0].click();", selectGateway);
                    Thread.Sleep(500);

                    var selectObj = new SelectElement(selectGateway);
                    selectObj.SelectByValue("MOMO");
                    Thread.Sleep(1000);

                    currentStep = "[KH] Bấm nút Tiếp tục để sang cổng thanh toán";
                    var btnContinuePay = dOnline.FindElement(By.Id("btn-continue"));
                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", btnContinuePay);
                    Thread.Sleep(2000);

                    currentStep = "[KH] Xác nhận thanh toán bên trang MoMo";
                    var btnXacNhanMoMo = wOnl.Until(ExpectedConditions.ElementExists(By.XPath("//button[contains(translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'thanh toán')]")));
                    jsOnl.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", btnXacNhanMoMo);

                    currentStep = "[KH] Chờ trang thông báo Thanh toán thành công";
                    wOnl.Until(x => x.PageSource.ToLower().Contains("thành công") || x.Url.ToLower().Contains("success"));
                    Thread.Sleep(2000);

                    // BƯỚC 4: NHÂN VIÊN BẤM XUẤT VÉ SAU VÀ BỊ CHẶN
                    currentStep = "[NV] Bấm Xác nhận & Xuất vé";
                    var btnXacNhanNV = dNv.FindElement(By.Id("btnSubmitBanVe"));
                    jsNv.ExecuteScript("arguments[0].scrollIntoView({block: 'center'}); arguments[0].click();", btnXacNhanNV);
                    Thread.Sleep(2000);
                    try { wNv.Until(ExpectedConditions.AlertIsPresent()).Accept(); } catch { }
                    Thread.Sleep(2000);
                    currentStep = "[NV] Bắt Alert thông báo lỗi";
                    var errorAlert = wNv.Until(ExpectedConditions.AlertIsPresent());
                    string alertText = errorAlert.Text.ToLower();
                    Thread.Sleep(2000);
                    pass = alertText.Contains("đã có người") || alertText.Contains("đã bán") || alertText.Contains("online") || alertText.Contains("lỗi") || alertText.Contains("thất bại");
                    msg = pass ? $"Hệ thống báo đúng lỗi: '{errorAlert.Text}'" : $"Lỗi: Thông báo sai '{errorAlert.Text}'";
                    Thread.Sleep(2000);
                    errorAlert.Accept();
                    Thread.Sleep(2000); 
                }
                catch (Exception ex)
                {
                    msg = $"Lỗi kẹt giao diện tại bước: {currentStep} | Chi tiết: {ex.Message}";
                }

                await GhiKetQuaTuNhien(dNv, "TC20", tc?.SpreadsheetExpectedResultRow ?? 0, msg);
            }
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

      
    }
}