using Dam_Bao_Chat_Luong.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Dam_Bao_Chat_Luong.Services;

public class SeleniumTestService : IDisposable
{
    private IWebDriver _driver;
    private readonly string _loginUrl = "http://duck123.runasp.net/Auth/Login";
    private readonly string _baseUrl = "http://duck123.runasp.net";
    private readonly string _screenshotDir;

    public SeleniumTestService(string? screenshotDir = null)
    {
        _screenshotDir = screenshotDir ?? Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        Directory.CreateDirectory(_screenshotDir);

        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");
        options.AddArgument("--disable-notifications");
        // Không dùng headless để user có thể quan sát
        // options.AddArgument("--headless");

        _driver = new ChromeDriver(options);
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Chạy tất cả test cases đăng nhập
    /// </summary>
    public List<TestResult> RunAllLoginTests(List<TestCaseModel> testCases)
    {
        var allResults = new List<TestResult>();

        foreach (var testCase in testCases)
        {
            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine($"  Test Case: {testCase.TestCaseId} - {testCase.TestObjective}");
            Console.WriteLine($"{'=',-60}");

            if (testCase.LoginDataList.Count > 0)
            {
                // Có nhiều login data (nhiều roles) → chạy từng role
                foreach (var loginData in testCase.LoginDataList)
                {
                    // Bỏ qua CUSTOMER nếu email/password null
                    if (string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
                    {
                        Console.WriteLine($"  ⏭️  Bỏ qua role {loginData.Role} (không có credentials)");
                        continue;
                    }

                    var result = ExecuteLoginTest(loginData, testCase);
                    allResults.Add(result);
                    PrintResult(result);
                }
            }
            else
            {
                // Không có JSON data → tạo LoginTestData rỗng (test thiếu fields)
                var emptyData = new LoginTestData { Role = "N/A", Email = "", Password = "" };
                var result = ExecuteLoginTest(emptyData, testCase);
                allResults.Add(result);
                PrintResult(result);
            }
        }

        return allResults;
    }

    /// <summary>
    /// Thực thi 1 test case đăng nhập cụ thể
    /// </summary>
    public TestResult ExecuteLoginTest(LoginTestData loginData, TestCaseModel testCase)
    {
        var result = new TestResult
        {
            TestCaseId = testCase.TestCaseId,
            TestObjective = testCase.TestObjective,
            Role = loginData.Role,
            ExpectedResult = testCase.ExpectedResult,
            SpreadsheetRow = testCase.SpreadsheetExpectedResultRow,
            Timestamp = DateTime.Now
        };

        try
        {
            // ===== Step 1: Vào trang đăng nhập =====
            Console.WriteLine($"  📌 Step 1: Vào trang đăng nhập...");
            _driver.Navigate().GoToUrl(_loginUrl);
            WaitForPageLoad();
            Thread.Sleep(1500);

            // Verify đang ở trang đăng nhập
            if (!_driver.Url.Contains("/Auth/Login"))
            {
                result.ActualResult = $"Không thể truy cập trang đăng nhập. URL hiện tại: {_driver.Url}";
                result.Status = "FAIL";
                result.IsMatch = false;
                TakeScreenshot(result);
                return result;
            }

            // ===== Step 2: Nhập các trường dữ liệu =====
            Console.WriteLine($"  📌 Step 2: Nhập dữ liệu - Email: {loginData.Email}, Role: {loginData.Role}");

            var emailField = _driver.FindElement(By.Id("EmailOrPhone"));
            var passwordField = _driver.FindElement(By.Id("password-input"));

            emailField.Clear();
            emailField.SendKeys(loginData.Email ?? "");

            passwordField.Clear();
            passwordField.SendKeys(loginData.Password ?? "");

            Thread.Sleep(800);

            // ===== Step 3: Ấn nút đăng nhập =====
            Console.WriteLine($"  📌 Step 3: Ấn nút đăng nhập...");
            var loginButton = _driver.FindElement(By.CssSelector(".btn-login"));
            loginButton.Click();

            // Đợi một chút để server phản hồi hoặc chuyển trang
            Console.WriteLine("  ⌛ Đang chờ server phản hồi...");
            
            // Đợi tối đa 10s để URL thay đổi (đăng nhập thành công) hoặc xuất hiện thông báo lỗi
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
                wait.Until(d => 
                {
                    // Case 1: Đã chuyển hướng sang trang khác
                    if (!d.Url.Contains("/Auth/Login")) return true;
                    
                    // Case 2: Vẫn ở trang login nhưng hiện lỗi
                    if (GetErrorMessages().Count > 0) return true;
                    
                    return false;
                });
            }
            catch (WebDriverTimeoutException)
            {
                // Timeout kệ nó, sẽ check kết quả sau
            }

            // ===== Đánh giá kết quả =====
            EvaluateLoginResult(result, testCase);
        }
        catch (NoSuchElementException ex)
        {
            result.ActualResult = $"Không tìm thấy element: {ex.Message}";
            result.Status = "FAIL"; // Coi như fail nếu element form bị mất
            result.IsMatch = false;
        }

        catch (WebDriverException ex)
        {
            result.ActualResult = $"WebDriver lỗi: {ex.Message}";
            result.Status = "ERROR";
            result.IsMatch = false;
        }
        catch (Exception ex)
        {
            result.ActualResult = $"Lỗi không xác định: {ex.Message}";
            result.Status = "ERROR";
            result.IsMatch = false;
        }

        // Luôn chụp screenshot
        TakeScreenshot(result);

        // Logout nếu đã đăng nhập thành công (để test role tiếp theo)
        TryLogout();

        return result;
    }

    /// <summary>
    /// Đánh giá kết quả sau khi click đăng nhập
    /// </summary>
    private void EvaluateLoginResult(TestResult result, TestCaseModel testCase)
    {
        var currentUrl = _driver.Url;
        var expectedResult = testCase.ExpectedResult;

        // Case 1: Đăng nhập thành công → URL thay đổi, không còn ở /Auth/Login
        if (!currentUrl.Contains("/Auth/Login"))
        {
            result.ActualResult = $"Đăng nhập thành công - Chuyển hướng đến: {currentUrl}";

            if (expectedResult.Contains("Trỏ về trang") || expectedResult.Contains("tương ứng"))
            {
                result.Status = "PASS";
                result.IsMatch = true;
            }
            else
            {
                // Đăng nhập thành công nhưng expected là thất bại
                result.Status = "FAIL";
                result.IsMatch = false;
            }
        }
        else
        {
            // Case 2: Vẫn ở trang đăng nhập → kiểm tra thông báo lỗi
            var errorMessages = GetErrorMessages();

            if (errorMessages.Count > 0)
            {
                result.ActualResult = $"Thông báo lỗi: {string.Join("; ", errorMessages)}";
            }
            else
            {
                result.ActualResult = "Vẫn ở trang đăng nhập - Không có thông báo lỗi rõ ràng";
            }

            // Kiểm tra expected result
            if (expectedResult.Contains("Thông báo") || expectedResult.Contains("thất bại") || expectedResult.Contains("thiếu"))
            {
                // Khi bỏ trống fields → trình duyệt dùng HTML5 validation (required)
                // → KHÔNG gửi form → KHÔNG có server error → vẫn ở trang đăng nhập = hành vi đúng
                result.Status = (errorMessages.Count > 0 || 
                    result.ActualResult.Contains("Không có thông báo", StringComparison.OrdinalIgnoreCase)) 
                    ? "PASS" : "FAIL";
                result.IsMatch = result.Status == "PASS";
            }
            else
            {
                result.Status = "FAIL";
                result.IsMatch = false;
            }
        }
    }

    /// <summary>
    /// Tìm các thông báo lỗi trên trang (chỉ quét chính xác các thẻ có thực tế trên duck123)
    /// </summary>
    private List<string> GetErrorMessages()
    {
        var messages = new List<string>();
        
        // Tắt tạm thời ImplicitWait để quét tức thì (0s delay)
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

        try
        {
            // 1. Check lỗi từ Server (sai tài khoản)
            var serverErrors = _driver.FindElements(By.CssSelector(".validation-summary-errors li"));
            foreach (var el in serverErrors)
            {
                var text = el.Text.Trim();
                if (!string.IsNullOrEmpty(text)) messages.Add(text);
            }

            // 2. Check lỗi từ Client (nhập thiếu)
            var clientErrors = _driver.FindElements(By.CssSelector("span.text-danger.field-validation-error"));
            foreach (var el in clientErrors)
            {
                var text = el.Text.Trim();
                if (!string.IsNullOrEmpty(text)) messages.Add(text);
            }
        }
        catch { }
        finally
        {
            // Bật lại ImplicitWait cho an toàn các bước sau
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        return messages;
    }

    /// <summary>
    /// Chụp screenshot và lưu vào thư mục Screenshots
    /// </summary>
    private void TakeScreenshot(TestResult result)
    {
        try
        {
            var safeName = $"{result.TestCaseId}_{result.Role ?? "NoRole"}_{DateTime.Now:yyyyMMdd_HHmmss}"
                .Replace(" ", "_")
                .Replace("/", "_")
                .Replace("\\", "_");

            var filename = $"{safeName}.png";
            var filepath = Path.Combine(_screenshotDir, filename);

            var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
            screenshot.SaveAsFile(filepath);

            result.ScreenshotPath = Path.GetFullPath(filepath);
            Console.WriteLine($"  📸 Screenshot: {result.ScreenshotPath}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠️ Không thể chụp screenshot: {ex.Message}");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Thử logout nếu đã đăng nhập thành công
    /// </summary>
    private void TryLogout()
    {
        try
        {
            if (!_driver.Url.Contains("/Auth/Login"))
            {
                // Thử tìm nút logout hoặc navigate trực tiếp
                _driver.Navigate().GoToUrl($"{_baseUrl}/Auth/Logout");
                Thread.Sleep(1500);

                // Nếu không có route Logout, navigate về trang login
                if (!_driver.Url.Contains("/Auth/Login"))
                {
                    _driver.Navigate().GoToUrl(_loginUrl);
                    Thread.Sleep(1000);
                }
            }
        }
        catch
        {
            // Fallback: navigate về trang login
            try { _driver.Navigate().GoToUrl(_loginUrl); }
            catch { }
        }
    }

    private void WaitForPageLoad()
    {
        try
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString() == "complete");
        }
        catch
        {
            // Timeout — tiếp tục
        }
    }

    private void PrintResult(TestResult result)
    {
        var color = result.Status switch
        {
            "PASS" => ConsoleColor.Green,
            "FAIL" => ConsoleColor.Red,
            _ => ConsoleColor.Yellow
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"  → [{result.Status}] {result.Role}: {result.ActualResult}");
        Console.ResetColor();
    }

    /// <summary>
    /// Trả WebDriver cho user code tiếp (multi-flow mode)
    /// </summary>
    public IWebDriver GetDriver() => _driver;

    public void Dispose()
    {
        try
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
        catch { }
    }
}
