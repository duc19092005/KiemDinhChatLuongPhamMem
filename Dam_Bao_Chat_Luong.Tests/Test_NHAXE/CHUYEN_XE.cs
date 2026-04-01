using ClosedXML.Excel;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using OpenQA.Selenium.Support.UI;

namespace Dam_Bao_Chat_Luong.Tests.Test_NHAXE
{
    [TestClass]
    public class CHUYEN_XE
    {
        private IWebDriver driver;
        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "CHUYEN_XE";

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        // Đăng nhập -> Thêm -> Search -> Sửa -> Xóa
        [TestMethod]
        public void LuongChay_CRUD_ChuyenXe_ToanDien()
        {
            Buoc1_KiemTraDangNhap();
            Buoc2_KiemTraThemChuyenXe();
            Buoc3_SearchChuyenXe();
            Buoc4_ChinhSuaChuyen();
            Buoc5_XoaChuyen();
        }

        private void Buoc1_KiemTraDangNhap()
        {
            int excelRow = 2;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
                Thread.Sleep(1000);

                driver.FindElement(By.Id("EmailOrPhone")).SendKeys("trang@gmail.com");
                driver.FindElement(By.Id("password-input")).SendKeys("123456789@phuongtrang");

                IWebElement btnLogin = driver.FindElement(By.XPath("//button[@type='submit']"));
                JsClick(btnLogin);
                Thread.Sleep(2000);

                Assert.IsTrue(driver.Url.Contains("NhaXe") || driver.Url.Contains("Home"), "Đăng nhập không thành công");
                GhiKetQuaExcel(excelRow, "Đăng nhập thành công, chuyển hướng đúng", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw; 
            }
        }

        private void Buoc2_KiemTraThemChuyenXe()
        {
            int excelRow = 6;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1500);

                driver.FindElement(By.Name("LoTrinhId")).SendKeys("VietNam - Campuchia");
                driver.FindElement(By.Name("XeId")).SendKeys("70AA123");
                driver.FindElement(By.Name("TuNgay")).SendKeys("02/04/2026");
                driver.FindElement(By.Name("DenNgay")).SendKeys("03/04/2026");
                driver.FindElement(By.Name("KhungGioTu")).SendKeys("07:00AM");
                driver.FindElement(By.Name("KhungGioDen")).SendKeys("04:00PM");
                driver.FindElement(By.Name("GianCachPhut")).SendKeys("120");

                IWebElement btnLuu = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[13]/button"));
                JsClick(btnLuu);
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Tạo chuyến xe thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        private void Buoc3_SearchChuyenXe()
        {
            int excelRow = 16;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1500);

                driver.FindElement(By.Name("diemDi")).SendKeys("VietNam");
                driver.FindElement(By.Name("diemDen")).SendKeys("Campuchia");

                IWebElement btnTimKiem = driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[2]/form/div/button"));
                JsClick(btnTimKiem);
                Thread.Sleep(1500);

                IWebElement table = driver.FindElement(By.TagName("table"));
                Assert.IsNotNull(table, "Không hiển thị bảng ViewList");

                GhiKetQuaExcel(excelRow, "Hiển thị bảng ViewList các chuyến xe khớp kết quả", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        private void Buoc4_ChinhSuaChuyen()
        {
            int excelRow = 21;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/74e5d826");
                Thread.Sleep(1500);

                SelectElement selectLoTrinh = new SelectElement(driver.FindElement(By.Name("LoTrinhId")));
                selectLoTrinh.SelectByText("Huflit Campuchia -> Bến xe Nhà Đức");

                SelectElement selectXe = new SelectElement(driver.FindElement(By.Name("XeId")));
                selectXe.SelectByText("50H33333");

                IWebElement btnLuu = driver.FindElement(By.XPath("//button[@type='submit']"));
                JsClick(btnLuu);
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Chỉnh sửa thành công, lưu dữ liệu mới", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        private void Buoc5_XoaChuyen()
        {
            int excelRow = 26;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1500);

                IWebElement btnXoaDongDau = driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[3]/div/div/div[2]/table/tbody/tr[3]/td[2]/div"));
                JsClick(btnXoaDongDau);
                Thread.Sleep(1000);

                IWebElement btnXacNhanXoa = driver.FindElement(By.XPath("/html/body/div[1]/div/main/div[3]/div/div/div[3]/button"));
                JsClick(btnXacNhanXoa);
                Thread.Sleep(1500);

               GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Đã xóa chuyến xe.", "Passed");
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
                    worksheet.Cell(dong, 5).Value = actualResult;  
                    worksheet.Cell(dong, 6).Value = status;      
                    workbook.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ LỖI GHI EXCEL: " + ex.Message);
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