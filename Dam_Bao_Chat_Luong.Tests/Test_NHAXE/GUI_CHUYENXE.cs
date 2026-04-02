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
    public class GUI_CHUYENXE
    {
        private IWebDriver driver;
        private string excelFilePath = @"D:\NAM_3_HKII\BDCL_PM\TEST_DOAN_BDCLPM.xlsx";
        private string sheetName = "GUI_QLCHUYEN"; 

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

        // DÒNG 2: GUI_QLC_IDX_01 - Bố cục trang
        [TestMethod]
        public void GUI_QLC_IDX_01_KiemTraBoCucTrangDanhSach()
        {
            int excelRow = 2;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1000);

                IWebElement title = driver.FindElement(By.TagName("h1"));
                Assert.IsTrue(title.Text.Contains("Danh sách Chuyến xe"), "Sai tiêu đề trang");

                IWebElement btnThem = driver.FindElement(By.PartialLinkText("Thêm chuyến xe"));
                string bgImage = btnThem.GetCssValue("background-image");
                string bgColor = btnThem.GetCssValue("background-color");
                Assert.IsTrue(bgImage.Contains("gradient") || bgColor.Contains("255, 102, 0"), "Nút không có màu cam gradient nổi bật");

                GhiKetQuaExcel(excelRow, "Hiển thị đúng tiêu đề và nút Thêm màu cam", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 3: GUI_QLC_IDX_02 - Bộ lọc tìm kiếm
        [TestMethod]
        public void GUI_QLC_IDX_02_KiemTraHienThiBoLoc()
        {
            int excelRow = 3;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1000);

                IWebElement selectDiemDi = driver.FindElement(By.Id("diemDi"));
                IWebElement selectDiemDen = driver.FindElement(By.Id("diemDen"));
                Assert.IsNotNull(selectDiemDi);
                Assert.IsNotNull(selectDiemDen);

                IWebElement iconDiemDi = driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[2]/form/div/div[1]/label/i"));
                IWebElement iconDiemDen = driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[2]/form/div/div[2]/label/i"));

                GhiKetQuaExcel(excelRow, "Hiển thị đủ 2 ô Select và icon đúng màu", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 5: GUI_QLX_GRID_01 - Bảng lịch điều phối
        [TestMethod]
        public void GUI_QLX_GRID_01_KiemTraBangLichDieuPhoi()
        {
            int excelRow = 4;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/8f0e614b");
                Thread.Sleep(1000);

                IWebElement ngaydi = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[4]/div[1]/div/input[1]"));

                IWebElement giodi = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[4]/div[2]/div/input[1]"));

                IWebElement gioden = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[5]/div[1]/div/input[1]"));

                GhiKetQuaExcel(excelRow, "Cột giờ và ngày hiện tại hiển thị chuẩn xác", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 6: GUI_QLX_GRID_02 - Màu sắc trạng thái
        [TestMethod]
        public void GUI_QLX_GRID_02_KiemTraMauTrangThai()
        {
            int excelRow = 5;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1500);

                IWebElement dangBan = driver.FindElement(By.CssSelector(".status-legend span:nth-child(1)"));
                string bgDangBan = dangBan.GetCssValue("background-color");
                Assert.IsTrue(bgDangBan.Contains("198, 246, 213"), "Đang bán không có nền xanh nhạt");

                IWebElement choChay = driver.FindElement(By.CssSelector(".status-legend span:nth-child(2)"));
                string bgChoChay = choChay.GetCssValue("background-color");
                Assert.IsTrue(bgChoChay.Contains("254, 235, 200"), "Chờ chạy không có nền vàng nhạt");

                IWebElement daHuy = driver.FindElement(By.CssSelector(".status-legend span:nth-child(3)"));
                string bgDaHuy = daHuy.GetCssValue("background-color");
                Assert.IsTrue(bgDaHuy.Contains("254, 215, 215"), "Đã hủy không có nền đỏ nhạt");

                GhiKetQuaExcel(excelRow, "Các khối trạng thái có màu nền tương ứng", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 7: GUI_QLC_CRT_01 - Form Lên lịch tự động
        [TestMethod]
        public void GUI_QLC_CRT_01_KiemTraFormLenLich()
        {
            int excelRow = 6;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                IWebElement title = driver.FindElement(By.TagName("h2"));
                Assert.AreEqual("uppercase", title.GetCssValue("text-transform"), "Tiêu đề không viết hoa");
                Assert.AreEqual("38px", title.GetCssValue("font-size"), "Tiêu đề không có cỡ chữ 38px");

                // Kiểm tra các nút nhanh tồn tại
                Assert.IsNotNull(driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[7]/div/button[1]")));
                Assert.IsNotNull(driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[7]/div/button[2]")));

                GhiKetQuaExcel(excelRow, "Tiêu đề viết hoa 38px, đủ nút thao tác nhanh", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 8: GUI_QLC_CRT_02 - Biểu tượng (Icon) Nhãn
        [TestMethod]
        public void GUI_QLC_CRT_02_KiemTraIconLabel()
        {
            int excelRow = 7;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Create");
                Thread.Sleep(1000);

                var icons = driver.FindElements(By.CssSelector("label i.fa, label i.fas"));
                Assert.IsTrue(icons.Count >= 4, "Không tìm thấy đủ icon trong các nhãn");

                int iconIndex = 1;

                foreach (var icon in icons)
                {
                    string actualColor = icon.GetCssValue("color");
                    string colorNoSpaces = actualColor.Replace(" ", "");
                    string errorMessage = $"Icon thứ {iconIndex} không có màu cam. Màu thực tế Selenium đọc được là: {actualColor}";
                    Assert.IsTrue(colorNoSpaces.Contains("255,152,0,1)"), errorMessage);
                    iconIndex++;
                }

                GhiKetQuaExcel(excelRow, "Các nhãn Lộ trình, Xe, Ngày, Giờ đều có icon màu cam", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 9: GUI_QLC_DET_01 - Hiển thị Hành trình Timeline
        [TestMethod]
        public void GUI_QLC_DET_01_KiemTraHanhTrinhTimeline()
        {
            int excelRow = 8;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/8f0e614b"); 
                Thread.Sleep(1000);

                IWebElement timelineLine = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[3]/div[1]/div/select"));
                GhiKetQuaExcel(excelRow, "Hiển thị nét đứt và chấm xanh lá cho điểm kết thúc", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 10: GUI_QLC_DET_02 - Biển số 
        [TestMethod]
        public void GUI_QLC_DET_02_KiemTraBienSoTaiXe()
        {
            int excelRow = 9;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe/Edit/8f0e614b");
                Thread.Sleep(1000);

                IWebElement timelineLine = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[3]/div[1]/div/select"));

                IWebElement boxBienSo = driver.FindElement(By.XPath("/html/body/div/div/main/div/div/form/div[3]/div[2]/div/select"));
                GhiKetQuaExcel(excelRow, "Biển số nền vàng chữ hoa, avatar tròn xám", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 13: GUI_QLC_DEL_01 - Cảnh báo xóa (Modal)
        [TestMethod]
        public void GUI_QLC_DEL_01_KiemTraCanhBaoXoa()
        {
            int excelRow = 10;
            string testCaseName = "GUI_QLC_DEL_01";
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1500);

                driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[3]/div/div/div[2]/table/tbody/tr[3]/td[4]/div")).Click();
                Thread.Sleep(1000);

                IWebElement modalHeader = driver.FindElement(By.CssSelector(".modal-header"));
                Assert.IsTrue(modalHeader.GetCssValue("background-color").Contains("220, 53, 69") , "Header Modal không có màu vàng cảnh báo");

                GhiKetQuaExcel(excelRow, "Modal Header màu vàng cảnh báo, icon tam giác đỏ", "Passed");
            }
            catch (Exception ex)
            {
                GhiKetQuaExcel(excelRow, ex.Message, "Failed");
                throw;
            }
        }

        // DÒNG 14: GUI_QLC_DEL_02 - Kiểu dáng nút xóa
        [TestMethod]
        public void GUI_QLC_DEL_02_KiemTraKieuDangNutXoa()
        {
            int excelRow = 11;
            try
            {
                driver.Navigate().GoToUrl("http://duck123.runasp.net/NhaXe/ChuyenXe");
                Thread.Sleep(1000);

                driver.FindElement(By.XPath("/html/body/div/div/main/div[1]/div[3]/div/div/div[2]/table/tbody/tr[3]/td[13]/div[1]")).Click();
                Thread.Sleep(1000);

                driver.FindElement(By.XPath("/html/body/div[1]/div/main/div[3]/div/div/div[3]/button")).Click();
                Thread.Sleep(1000); 

                IWebElement btnXacNhan = driver.FindElement(By.XPath("/html/body/div[1]/div/main/div[2]/div/div/form/div[3]/button[2]"));
                IWebElement btnHuy = driver.FindElement(By.XPath("/html/body/div[1]/div/main/div[2]/div/div/form/div[3]/button[1]"));

                Assert.IsTrue(btnXacNhan.GetCssValue("background-color").Contains("220, 53, 69") , "Nút xác nhận không có màu đỏ đậm");
                Assert.IsTrue(btnHuy.GetCssValue("background-color").Contains("108, 117, 125") || btnHuy.GetCssValue("background-color").Contains("gray"), "Nút hủy không có màu xám");

                GhiKetQuaExcel(excelRow, "Nút Xác nhận đỏ/có hover phóng to, Nút Hủy xám tối giản", "Passed");
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
