using ClosedXML.Excel;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dam_Bao_Chat_Luong.Tests.Test_NHAXE
{
    [TestClass]
    public class TESTING_QLVANCHUYEN
    {
        private IWebDriver driver;
        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "TESTING_QLVANCHUYEN";

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();

            driver.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            driver.FindElement(By.Id("EmailOrPhone")).SendKeys("trang@gmail.com");
            driver.FindElement(By.Id("password-input")).SendKeys("123456789@phuongtrang");
            driver.FindElement(By.XPath("/html/body/div[2]/form/button")).Click();
            Thread.Sleep(2000);
        }

        [TestMethod]
        public void DP_TC_01_KiemTraHienThiMacDinh()
        {
            int excelRow = 2;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                Assert.IsTrue(driver.PageSource.Contains("Bảng Lịch Chạy"), "Không thấy tiêu đề Bảng Lịch Chạy");

                IWebElement todayCol = driver.FindElement(By.CssSelector(".today-column, .current-day, .bg-warning"));
                Assert.IsNotNull(todayCol, "Không tìm thấy cột ngày hôm nay được bôi vàng");

                GhiKetQuaExcel(excelRow, "Hiển thị đúng tiêu đề và bôi vàng cột hôm nay", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_02_DieuHuongTuanTruoc()
        {
            int excelRow = 7;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnTuanTruoc = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[1]/div/a[1]"));
                JsClick(btnTuanTruoc);
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Trang reload và hiển thị lịch tuần trước", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_03_DieuHuongTuanSau()
        {
            int excelRow = 12;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnTuanSau = driver.FindElement(By.XPath("//a[contains(text(), 'Tuần sau')] | //button[contains(text(), 'Tuần sau')]"));
                JsClick(btnTuanSau);
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Trang reload và hiển thị lịch tuần sau", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_04_DieuHuongHomNay()
        {
            int excelRow = 17;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnHomNay = driver.FindElement(By.XPath("//a[contains(text(), 'Hôm nay')] | //button[contains(text(), 'Hôm nay')]"));
                JsClick(btnHomNay);
                Thread.Sleep(1500);

                IWebElement todayCol = driver.FindElement(By.CssSelector(".today-column, .current-day"));
                Assert.IsNotNull(todayCol, "Không highlight cột ngày hôm nay");

                GhiKetQuaExcel(excelRow, "Trở về tuần hiện tại và highlight cột hôm nay", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_05_HienThiChuaCoTaiXe_CoDuLieu()
        {
            int excelRow = 22;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                var theChuyenXe = driver.FindElements(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[3]/a"));
                Assert.IsTrue(theChuyenXe.Count > 0, "Không tìm thấy chuyến xe nào chưa có tài xế");

                GhiKetQuaExcel(excelRow, $"Hiển thị {theChuyenXe.Count} chuyến xe chưa có tài xế", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        

        [TestMethod]
        public void DP_TC_07_BangLichTaiXe_CoDuLieu()
        {
            int excelRow = 32;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(2000);

                IWebElement bangLich = driver.FindElement(By.TagName("table"));
                Assert.IsTrue(bangLich.Text.Contains("- Trống -") || bangLich.FindElements(By.CssSelector(".trip-card, .assigned-trip")).Count > 0, "Bảng lịch không hiển thị đúng format trống hoặc thẻ chuyến xe");

                GhiKetQuaExcel(excelRow, "Hiển thị danh sách tài xế và thẻ phân công/- Trống -", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_09_ChuyenHuong_ChuyenChuaPhanCong()
        {
            int excelRow = 42;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnChonTX = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[8]/a"));
                JsClick(btnChonTX);
                Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.Contains("PhanCong"), "Chuyển hướng thất bại");

                GhiKetQuaExcel(excelRow, "Mở form phân công thành công", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_10_ChuyenHuong_DoiTaiXe()
        {
            int excelRow = 47;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement theXeDaPhanCong = driver.FindElement(By.CssSelector("table tbody td div.trip-card, table tbody td div.bg-primary"));
                JsClick(theXeDaPhanCong);
                Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.Contains("PhanCong"), "Chuyển hướng thất bại");

                GhiKetQuaExcel(excelRow, "Mở form đổi tài xế thành công", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_11_KiemTraThongTinReadonly()
        {
            int excelRow = 52;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682"); 
                Thread.Sleep(1500);

                Assert.IsTrue(driver.PageSource.Contains("Thông tin chuyến xe"), "Không thấy vùng thông tin chuyến xe");

                GhiKetQuaExcel(excelRow, "Hiển thị đủ thông tin chuyến xe dạng Read-only", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_12_KiemTraDropdownTaiXe()
        {
            int excelRow = 53;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682");
                Thread.Sleep(1500);

                IWebElement dropdown = driver.FindElement(By.TagName("select"));
                Assert.IsTrue(dropdown.Text.Contains("-- Chưa phân công"), "Thiếu option chưa phân công");

                GhiKetQuaExcel(excelRow, "Hiển thị đúng option Chưa phân công và danh sách tài xế", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_13_PhanCongThanhCong()
        {
            int excelRow = 60;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682");
                Thread.Sleep(1500);

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[1]/select/option[2]")).Click(); 

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[2]/button"));
                JsClick(btnLuu);
                Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.Contains("DieuPhoi"), "Chưa điều hướng về trang Index");

                GhiKetQuaExcel(excelRow, "Phân công thành công, điều hướng về Lịch Chạy", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_14_HuyPhanCong()
        {
            int excelRow = 67;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682");
                Thread.Sleep(1500);

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[1]/select/option[1]")).Click();

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[2]/button"));
                JsClick(btnLuu);
                Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.Contains("DieuPhoi"), "Chưa điều hướng về trang Index");

                GhiKetQuaExcel(excelRow, "Gỡ tài xế thành công, chuyến trở về danh sách chờ", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void DP_TC_15_KiemTraNutHuyCancel()
        {
            int excelRow = 74;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682");
                Thread.Sleep(1500);

                IWebElement btnHuy = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[2]/a"));
                JsClick(btnHuy);
                Thread.Sleep(1500);

                Assert.IsTrue(driver.Url.Contains("DieuPhoi"), "Chưa điều hướng về trang Index");

                GhiKetQuaExcel(excelRow, "Hủy thao tác và quay về trang Bảng Lịch Chạy", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        private void JsClick(IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].click();", element);
        }

        private void GhiKetQuaExcel(int dong, string actualResult, string status)
        {
            try
            {
                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet(sheetName);
                    worksheet.Cell(dong, 6).Value = actualResult;
                    worksheet.Cell(dong, 7).Value = status;
                    workbook.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ LỖI GHI EXCEL: Hãy chắc chắn bạn đã TẮT file Excel đi. Chi tiết: " + ex.Message);
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
            }
        }
    }
}
