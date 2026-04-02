using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using System;
using System.Threading;
using ClosedXML.Excel;

namespace Dam_Bao_Chat_Luong.Test_NHAXE
{
    [TestClass]
    public class GUI_QL_NHA_XE
    {
        private IWebDriver driver;

        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "GUI_TESTING_NHAXE";

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

        // DÒNG 2: GUI_QLX_IDX_01 - Bố cục trang
        [TestMethod]
        public void GUI_QLX_IDX_01_KiemTraBoCucDanhSachXe()
        {
            int excelRow = 2;
            string testCaseName = "GUI_QLX_IDX_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe");
                Thread.Sleep(1000);

                IWebElement title = driver.FindElement(By.TagName("h1"));
                Assert.IsTrue(title.Text.Contains("Quản Lý Xe"), "Thiếu chữ Quản Lý Xe");

                IWebElement btnThemXe = driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[1]/a"));
                string bgColor = btnThemXe.GetCssValue("background-color");
                Assert.IsTrue(bgColor.Contains("255, 140, 0"), "Nút không có màu cam");

                // Đã thêm: Chụp ảnh khi Pass và đưa vào Excel
                string anhThanhCong = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, "Đúng thiết kế", "Passed", anhThanhCong);
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 3: GUI_QLX_IDX_02 - Bảng dữ liệu
        [TestMethod]
        public void GUI_QLX_IDX_02_KiemTraBangDuLieu()
        {
            int excelRow = 3;
            string testCaseName = "GUI_QLX_IDX_02";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe");
                Thread.Sleep(1000);

                IWebElement table = driver.FindElement(By.TagName("table"));

                IWebElement dongChan = table.FindElement(By.CssSelector("tbody tr:nth-child(2)"));
                string rowColor = dongChan.GetCssValue("background-color");

                Assert.IsTrue(rowColor.Contains("253, 248, 243"), "Dòng chẵn sai màu nền");

                string anhThanhCong = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, "Hiển thị đủ cột, dòng chẵn đúng màu", "Passed", anhThanhCong);
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // DÒNG 5: GUI_QLX_CRT_02 - Validate trống
        [TestMethod]
        public void GUI_QLX_CRT_02_KiemTraValidateTrong()
        {
            int excelRow = 4;
            string testCaseName = "GUI_QLX_CRT_02";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[4]/button")).Click();
                Thread.Sleep(500);

                IWebElement errorText = driver.FindElement(By.ClassName("text-danger"));
                string errorColor = errorText.GetCssValue("color");

                Assert.IsTrue(errorColor.Contains("220, 53, 69") || errorColor.Contains("255, 0, 0"), "Lỗi không hiển thị màu đỏ");

                string anhThanhCong = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, "Thông báo lỗi hiển thị đúng màu đỏ", "Passed", anhThanhCong);
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }
        // DÒNG 7: GUI_QLX_DEL_01 - Modal Xóa
        [TestMethod]
        public void GUI_QLX_DEL_01_KiemTraModalXoa()
        {
            int excelRow = 5;
            string testCaseName = "GUI_QLX_DEL_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe");
                Thread.Sleep(1000);

                driver.FindElement(By.CssSelector(".btn-delete")).Click();
                Thread.Sleep(500);

                IWebElement modalHeader = driver.FindElement(By.CssSelector(".modal-header.bg-danger"));
                Assert.IsNotNull(modalHeader, "Không tìm thấy modal báo xóa nền đỏ");

                string anhThanhCong = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, "Hiển thị đúng Modal Xóa", "Passed", anhThanhCong);
            }
            catch (Exception ex)
            {
                string anhLoi = ChupManHinh(testCaseName);
                GhiKetQuaExcel(excelRow, ex.Message, "Failed", anhLoi);
                throw;
            }
        }

        // HÀM HỖ TRỢ 1: CHỤP MÀN HÌNH (DÙNG MẢNG BYTE)
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

                Console.WriteLine("📸 Đã chụp màn hình: " + filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Lỗi chụp màn hình: " + ex.Message);
                return null;
            }
        }

        // HÀM HỖ TRỢ 2: GHI KẾT QUẢ VÀ CHÈN ẢNH VÀO EXCEL
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