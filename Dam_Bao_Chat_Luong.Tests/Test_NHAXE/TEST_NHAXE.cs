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
    public class TEST_NHAXE
    {
        private IWebDriver driver;

        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "TESTING_NHAXE"; 

        [TestInitialize]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();

            driver.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
            driver.FindElement(By.Id("EmailOrPhone")).SendKeys("trang@gmail.com");
            driver.FindElement(By.Id("password-input")).SendKeys("123456789@phuongtrang");
            driver.FindElement(By.XPath("/html/body/div[2]/form/button")).Click();
            Thread.Sleep(2000); // Chờ load trang sau đăng nhập
        }

        // DÒNG 2: III.2_QLX_ADD_01 - Thêm mới thành công
        [TestMethod]
        public void III_2_QLX_ADD_01_ThemMoiThanhCong()
        {
            int excelRow = 2; 
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe/Create"); 
                Thread.Sleep(1000);

                driver.FindElement(By.Id("BienSoXe")).SendKeys("70AA - 677801234567890");

                driver.FindElement(By.Id("LoaiXeId")).SendKeys("Xe Duck");

                IWebElement inputSoGhe = driver.FindElement(By.Id("SoLuongGhe"));
                inputSoGhe.Clear();
                inputSoGhe.SendKeys("40");

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[4]/button")).Click();
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo: Thêm và tạo ghế thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 3: III.2_QLX_ADD_02 - Bỏ trống Biển số
        [TestMethod]
        public void III_2_QLX_ADD_02_ThemMoiThatBai_TrongBienSo()
        {
            int excelRow = 3;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.Id("BienSoXe")).Clear();
                driver.FindElement(By.Id("LoaiXeId")).SendKeys("Xe Duck");

                IWebElement inputSoGhe = driver.FindElement(By.Id("SoLuongGhe"));
                inputSoGhe.Clear();
                inputSoGhe.SendKeys("40");

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[4]/button")).Click();
                Thread.Sleep(500);

                bool hasError = driver.PageSource.Contains("The BienSoXe field is required.");
                Assert.IsTrue(hasError, "Không hiển thị lỗi bắt buộc nhập Biển số");

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo lỗi: The BienSoXe field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 4: III.2_QLX_ADD_03 - Bỏ trống Loại xe
        [TestMethod]
        public void III_2_QLX_ADD_03_ThemMoiThatBai_TrongLoaiXe()
        {
            int excelRow = 4;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.Id("BienSoXe")).SendKeys("70AA - 67780");

                IWebElement inputSoGhe = driver.FindElement(By.Id("SoLuongGhe"));
                inputSoGhe.Clear();
                inputSoGhe.SendKeys("40");

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[4]/button")).Click();
                Thread.Sleep(500);

                IWebElement thongbao = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[2]/span"));

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo lỗi: The LoaiXeId field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 5: III.2_QLX_ADD_04 - Bỏ trống Số lượng ghế
        [TestMethod]
        public void III_2_QLX_ADD_04_ThemMoiThatBai_TrongSoLuongGhe()
        {
            int excelRow = 5;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe/Create");
                Thread.Sleep(1000);

                driver.FindElement(By.Id("BienSoXe")).SendKeys("70AA - 67780");
                driver.FindElement(By.Id("LoaiXeId")).SendKeys("Xe Duck");

                IWebElement inputSoGhe = driver.FindElement(By.Id("SoLuongGhe"));
                inputSoGhe.Clear();

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[2]/div[4]/button")).Click();
                Thread.Sleep(500);

                bool hasError = driver.PageSource.Contains("The SoLuongGhe field is required.");
                Assert.IsTrue(hasError, "Không hiển thị lỗi bắt buộc nhập Số lượng ghế");

                GhiKetQuaExcel(excelRow, "Hiển thị thông báo lỗi: The SoLuongGhe field is required.", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 6: III.2_QLX_EDIT_01 - Chỉnh sửa thông tin xe
        [TestMethod]
        public void III_2_QLX_EDIT_01_ChinhSuaThanhCong()
        {
            int excelRow = 6;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe");
                Thread.Sleep(1000);

                driver.FindElement(By.XPath("//table/tbody/tr[1]//a[contains(@href, 'Edit')]")).Click();
                Thread.Sleep(1000);

                driver.FindElement(By.Id("LoaiXeId")).SendKeys("Xe Duck");

                driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[3]/button")).Click();
                Thread.Sleep(1500);

                GhiKetQuaExcel(excelRow, "Cập nhật thành công, hiển thị: Cập nhật thông tin thành công!", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 7: III.2_QLX_DELETE_01 - Xóa xe thành công
        [TestMethod]
        public void III_2_QLX_DELETE_01_XoaThanhCong()
        {
            int excelRow = 7;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/Xe");
                Thread.Sleep(1000);

                driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[2]/div/table/tbody/tr[5]/td[5]/div/button")).Click();
                Thread.Sleep(1000); 

                driver.FindElement(By.XPath("/html/body/div[1]/div/main/div[2]/div/div/form/div[3]/button[2]")).Click();
                Thread.Sleep(1500);

                bool isSuccess = driver.PageSource.Contains("Xe xóa thành công!");
                Assert.IsTrue(isSuccess, "Không hiển thị thông báo xóa thành công");

                GhiKetQuaExcel(excelRow, "Xe bị xóa khỏi danh sách, hiển thị: Xe xóa thành công!", "Passed");
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

                    // Cột 6: Actual Result
                    worksheet.Cell(dong, 6).Value = actualResult;

                    // Cột 7: Status
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
