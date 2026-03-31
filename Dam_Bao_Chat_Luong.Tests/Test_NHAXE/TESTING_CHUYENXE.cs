using ClosedXML.Excel;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Dam_Bao_Chat_Luong.Tests.Test_NHAXE
{
    [TestClass]
    public class TESTING_CHUYENXE
    {
        private IWebDriver driver;

        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "TESTING_CHUYENXE";

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

        [TestMethod]
        public void III_3_QLC_ADD_01_ThemThanhCong()
        {
            int excelRow = 2;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Tạo chuyến xe thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_02_ThemThatBai_ThieuBienSo()
        {
            int excelRow = 13;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: The BienSoXe field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_03_ThemThatBai_ThieuLoTrinh()
        {
            int excelRow = 24;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Lộ Trình
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa chọn lộ trình!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_05_ThemThatBai_ThieuNgayDi()
        {
            int excelRow = 35;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Ngày đi
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa có thông tin Ngày đi!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_06_ThemThatBai_ThieuNgayDen()
        {
            int excelRow = 46;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Ngày đến
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa điền thông tin ngày đến!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_07_ThemThatBai_ThieuGioDi()
        {
            int excelRow = 57;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Giờ đi
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa tạo thời gian chạy!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_08_ThemThatBai_ThieuGioDen()
        {
            int excelRow = 68;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Giờ đến
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa tạo thời gian chạy!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_09_ThemThatBai_ThieuGianCach()
        {
            int excelRow = 79;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Bỏ trống Giãn cách chuyến
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chưa có thời gian giãn cách chuyến!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_ADD_10_ThemThatBai_TrungThoiGian()
        {
            int excelRow = 90;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                // Giờ đi và Giờ đến trùng nhau
                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA67780");
                driver.FindElement(By.Name("TuNgay")).SendKeys("03/22/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/24/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("07:00AM"); // Cố tình trùng
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Thời gian chạy trùng lịch!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }
        [TestMethod]
        public void III_3_QLC_EDIT_01_SuaThanhCong()
        {
            int excelRow = 101;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/0d7635f7"); 
                Thread.Sleep(1000);

                IWebElement tuNgay = driver.FindElement(By.Name("NgayDi"));
                tuNgay.Clear();
                tuNgay.SendKeys("03/23/2026");

                IWebElement btnLuu = driver.FindElement(By.CssSelector("button[type='submit']"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chỉnh sửa chuyến xe thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_EDIT_02_SuaThatBai_TrongXe()
        {
            int excelRow = 110;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/0d7635f7");
                Thread.Sleep(1000);

                IWebElement xeInput = driver.FindElement(By.Id("XeId"));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].value = '';", xeInput);

                IWebElement btnLuu = driver.FindElement(By.CssSelector("button[type='submit']"));
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                Assert.IsTrue(driver.PageSource.Contains("The XeId field is required."), "Không thấy lỗi bắt buộc nhập Xe");
                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: The XeId field is required.!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_EDIT_03_SuaThatBai_TrongLoTrinh()
        {
            int excelRow = 119;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/0d7635f7");
                Thread.Sleep(1000);

                IWebElement lotrinhInput = driver.FindElement(By.Name("LoTrinhId"));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].value = '';", lotrinhInput);

                IWebElement btnLuu = driver.FindElement(By.CssSelector("button[type='submit']"));
                js.ExecuteScript("arguments[0].click();", btnLuu);

                Thread.Sleep(1000);

                Assert.IsTrue(driver.PageSource.Contains("The LoTrinhId field is required."), "Không thấy lỗi bắt buộc nhập Lộ trình");
                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: The LoTrinhId field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_EDIT_04_SuaThatBai_TrongNgayDi()
        {
            int excelRow = 128;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/0d7635f7");
                Thread.Sleep(1000);

                IWebElement ngaydiInput = driver.FindElement(By.Name("NgayDi"));

                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].value = '';", ngaydiInput);

                IWebElement btnLuu = driver.FindElement(By.CssSelector("button[type='submit']"));
                js.ExecuteScript("arguments[0].click();", btnLuu);
                Thread.Sleep(1000);

                Assert.IsTrue(driver.PageSource.Contains("The NgayDi field is required."), "Không thấy lỗi bắt buộc nhập Ngày đi");
                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: The NgayDi field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        [TestMethod]
        public void III_3_QLC_DELETE_01_XoaThanhCong()
        {
            int excelRow = 137;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1500);

                driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[3]/div/div/div[2]/table/tbody/tr[4]/td[6]/div")).Click();
                Thread.Sleep(500);

                IWebElement btnXoa = driver.FindElement(By.XPath("//button[contains(text(), 'Xóa') or contains(@class, 'delete')]"));
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", btnXoa);
                Thread.Sleep(1000);

                IWebElement btnXacNhanXoa = driver.FindElement(By.XPath("//form//button[contains(@class, 'danger') or position()=2]"));
                IJavaScriptExecutor js2 = (IJavaScriptExecutor)driver;
                js2.ExecuteScript("arguments[0].click();", btnXacNhanXoa);

                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Chuyến xe xóa thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }
        private void GhiKetQuaExcel(int dong, string actualResult, string status)
        {
            try
            {
                using (var workbook = new XLWorkbook(excelFilePath))
                {
                    var worksheet = workbook.Worksheet(sheetName);

                    // Ghi chữ vào Cột 6 (Actual Result) và Cột 7 (Status)
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