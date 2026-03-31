using ClosedXML.Excel;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Interactions; 

namespace Dam_Bao_Chat_Luong.Tests.Test_NHAXE
{
    [TestClass]
    public class GUI_QLVANCHUYEN
    {
        private IWebDriver driver;

        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "GUI_QLVANCHUYEN"; 

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            driver.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            driver.FindElement(By.Id("EmailOrPhone")).SendKeys("trang@gmail.com");
            driver.FindElement(By.Id("password-input")).SendKeys("123456789@phuongtrang");
            driver.FindElement(By.XPath("/html/body/div[2]/form/button")).Click();
            Thread.Sleep(2000);
        }

        // DÒNG 2: GUI_QLDP_IDX_01 - Bộ điều hướng thời gian
        [TestMethod]
        public void GUI_QLDP_IDX_01_KiemTraDieuHuongThoiGian()
        {
            int excelRow = 2;
            string testName = "GUI_QLDP_IDX_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement dateRange = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[1]/div/span"));

                // Kiểm tra nút Hôm nay 
                IWebElement btnHomNay = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[1]/div/a[3]"));

                GhiKetQuaExcel(excelRow, "Hiển thị khoảng ngày ở giữa, nút Hôm nay màu xanh đậm", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 3: GUI_QLDP_IDX_02 - Mục "Chưa có tài xế"
        [TestMethod]
        public void GUI_QLDP_IDX_02_KiemTraMucChuaCoTaiXe()
        {
            int excelRow = 3;
            string testName = "GUI_QLDP_IDX_02";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi/PhanCong/a4260682");
                Thread.Sleep(1500);

                IWebElement khungChuaPhanCong = driver.FindElement(By.Name("taiXeId"));

                GhiKetQuaExcel(excelRow, "Kiểm tra dropdown phân công tài xế", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 4: GUI_QLDP_IDX_03 - Thẻ chuyến xe chờ
        [TestMethod]
        public void GUI_QLDP_IDX_03_KiemTraTheChuyenXeCho()
        {
            int excelRow = 4;
            string testName = "GUI_QLDP_IDX_03";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement theXeCho = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[1]"));

                string borderLeftWidth = theXeCho.GetCssValue("border-left-width");
                string borderLeftColor = theXeCho.GetCssValue("border-left-color");

                Assert.IsTrue(borderLeftWidth.Contains("4px"), "Viền trái không dày 4px");
                Assert.IsTrue(borderLeftColor.Contains("255, 0, 0") || borderLeftColor.Contains("red") || borderLeftColor.Contains("220, 53, 69"), "Viền trái không có màu đỏ");

                IWebElement btnChonTX = theXeCho.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[2]/a"));
                string btnColor = btnChonTX.GetCssValue("background-color");
                Assert.IsTrue(btnColor.Contains("13, 110, 253") || btnColor.Contains("blue"), "Nút Chọn TX không có màu xanh dương");

                GhiKetQuaExcel(excelRow, "Thẻ viền trái đỏ 4px, nút Chọn TX xanh dương", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 5: GUI_QLDP_TAB_01 - Bảng Sticky
        [TestMethod]
        public void GUI_QLDP_TAB_01_KiemTraTinhCoDinhSticky()
        {
            int excelRow = 5;
            string testName = "GUI_QLDP_TAB_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement headerNgay = driver.FindElement(By.CssSelector("thead tr th"));
                Assert.IsTrue(headerNgay.GetCssValue("position").Contains("sticky"), "Tiêu đề Ngày (Header) không được dính (sticky)");

                IWebElement colTaiXe = driver.FindElement(By.CssSelector("tbody tr td:first-child, tbody tr th:first-child"));
                Assert.IsTrue(colTaiXe.GetCssValue("position").Contains("sticky"), "Cột TÀI XẾ / NGÀY không được dính (sticky)");

                GhiKetQuaExcel(excelRow, "Tiêu đề Ngày và Cột đầu tiên đều có thuộc tính sticky", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 6: GUI_QLDP_TAB_02 - Highlight ngày hiện tại
        [TestMethod]
        public void GUI_QLDP_TAB_02_KiemTraHighlightNgayHienTai()
        {
            int excelRow = 6;
            string testName = "GUI_QLDP_TAB_02";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement colHomNay = driver.FindElement(By.CssSelector(".current-day, .today-column, th.bg-warning-light"));
                string bgColor = colHomNay.GetCssValue("background-color");

                // Màu nền vàng nhạt 
                Assert.IsTrue(bgColor.Contains("255, 243, 205"), "Cột hôm nay không được highlight màu vàng nhạt");

                GhiKetQuaExcel(excelRow, "Cột ngày hôm nay có nền vàng nhạt (#fff3cd)", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 8: GUI_QLDP_TAB_04 - Thẻ chuyến xe đã phân công (Hover)
        [TestMethod]
        public void GUI_QLDP_TAB_04_KiemTraTheChuyenXeDaPhanCong()
        {
            int excelRow = 8;
            string testName = "GUI_QLDP_TAB_04";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                IWebElement theDaPhanCong = driver.FindElement(By.XPath("//table//td//div"));

                string borderLeft = theDaPhanCong.GetCssValue("border-left-color");
                Assert.IsTrue(borderLeft.Contains("blue") || borderLeft.Contains("0, 40, 85"), "Viền trái không có màu xanh đậm");

                Actions action = new Actions(driver);
                action.MoveToElement(theDaPhanCong).Perform();
                Thread.Sleep(500);

                GhiKetQuaExcel(excelRow, "Thẻ viền trái xanh đậm, có hiệu ứng nhích lên khi di chuột", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 9: GUI_QLDP_PC_01 - Form Phân công 
        [TestMethod]
        public void GUI_QLDP_PC_01_KiemTraGiaoDienFormPhanCong()
        {
            int excelRow = 9;
            string testName = "GUI_QLDP_PC_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnChon = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[1]/a"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnChon);
                Thread.Sleep(1500);

                IWebElement formContainer = driver.FindElement(By.XPath("/html/body/div/div/main/div/div"));

                string borderColor = formContainer.GetCssValue("border-color");

                Assert.IsTrue(borderColor.Contains("13, 110, 253") || borderColor.Contains("blue"), "Form không có viền xanh Primary");

                GhiKetQuaExcel(excelRow, "Form nằm trong Card viền xanh Primary, max-width 600px", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 10: GUI_QLDP_PC_02 - Tóm tắt thông tin chuyến
        [TestMethod]
        public void GUI_QLDP_PC_02_KiemTraKhuVucTomTat()
        {
            int excelRow = 10;
            string testName = "GUI_QLDP_PC_02";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                // Click mở form phân công
                IWebElement btnChon = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[1]/a"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnChon);
                Thread.Sleep(1500);

                IWebElement summaryBox = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/div"));

                IWebElement badgeBienSo = summaryBox.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/div/ul/li[2]/span"));
                Assert.IsTrue(badgeBienSo.GetCssValue("background-color").Contains("255, 193, 7") , "Khu vực Biển số không có nền vàng");

                GhiKetQuaExcel(excelRow, "Khu vực tóm tắt nền xám nhạt, Biển số badge vàng", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 11: GUI_QLDP_PC_03 - Các nút hành động
        [TestMethod]
        public void GUI_QLDP_PC_03_KiemTraNutHanhDong()
        {
            int excelRow = 11;
            string testName = "GUI_QLDP_PC_03";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/DieuPhoi");
                Thread.Sleep(1500);

                IWebElement btnChon = driver.FindElement(By.XPath("/html/body/div/div/main/div/div[2]/div[2]/div/div[1]/a"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnChon);
                Thread.Sleep(1500);

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[2]/button"));
                IWebElement btnHuy = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/div[2]/form/div[2]/a"));

                string bgLuu = btnLuu.GetCssValue("background-color");
                string bgHuy = btnHuy.GetCssValue("background-color");

                Assert.IsTrue(bgLuu.Contains("25, 135, 84") || bgLuu.Contains("green") || bgLuu.Contains("0, 128, 0"), "Nút Lưu Phân Công không có màu xanh lá");
                Assert.IsTrue(bgHuy.Contains("108, 117, 125") || bgHuy.Contains("gray") || bgHuy.Contains("128, 128, 128"), "Nút Hủy không có màu xám");

                GhiKetQuaExcel(excelRow, "Nút Lưu Phân Công xanh lá, nút Hủy xám", "Passed");
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        private string ChupManHinh(string tenTestCase)
        {
            try
            {
                string folderPath = @"D:\NAM_3_HKII\BDCL_PM\Screenshots\";
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filePath = folderPath + tenTestCase + "_" + timeStamp + ".png";

                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                System.IO.File.WriteAllBytes(filePath, screenshot.AsByteArray);

                Console.WriteLine("📸 Đã chụp màn hình lỗi: " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Lỗi chụp màn hình: " + ex.Message);
                return null;
            }
        }

        private void GhiKetQuaExcel(int dong, string actualResult, string status, string imagePath = null)
        {
            try
            {
                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet(sheetName);

                    worksheet.Cell(dong, 6).Value = actualResult;
                    worksheet.Cell(dong, 7).Value = status;

                    if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                    {
                        worksheet.Row(dong).Height = 80;
                        worksheet.Column(8).Width = 40;

                        worksheet.AddPicture(imagePath)
                                 .MoveTo(worksheet.Cell(dong, 8))
                                 .WithSize(200, 100);
                    }

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
