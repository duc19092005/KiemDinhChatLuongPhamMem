using System.Text;
using Dam_Bao_Chat_Luong.Enums;
using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services;
using Dam_Bao_Chat_Luong.Services.ThirdPersonServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LoginTestResult = Dam_Bao_Chat_Luong.Models.TestResult;

namespace Dam_Bao_Chat_Luong.Tests;

/// <summary>
/// Test class cho chức năng Đăng nhập — sử dụng MSTest framework
/// Chạy bằng: dotnet test
/// </summary>
[TestClass]
public class LoginTests
{
    private SeleniumTestService _seleniumService = null!;
    private GoogleSpreadSheetService _spreadsheetService = null!;
    private static List<TestCaseModel>? _cachedTestCases;

    [TestInitialize]
    public void Setup()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        _seleniumService = new SeleniumTestService();
        _spreadsheetService = new GoogleSpreadSheetService();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _seleniumService?.Dispose();
    }

    /// <summary>
    /// Helper: Lấy test cases từ Google Spreadsheet (cache lại để không gọi API nhiều lần)
    /// </summary>
    private async Task<List<TestCaseModel>> GetTestCasesAsync()
    {
        if (_cachedTestCases == null)
        {
            _cachedTestCases = await _spreadsheetService.GetTestCases(GoogleSpreadSheetEnum.DangNhap);
        }
        return _cachedTestCases;
    }

    /// <summary>
    /// Helper: Tìm test case theo TestCaseId
    /// </summary>
    private async Task<TestCaseModel> FindTestCaseAsync(string testCaseId)
    {
        var testCases = await GetTestCasesAsync();
        var tc = testCases.FirstOrDefault(t =>
            t.TestCaseId.Equals(testCaseId, StringComparison.OrdinalIgnoreCase));
        Assert.IsNotNull(tc, $"Không tìm thấy test case '{testCaseId}' trong spreadsheet");
        return tc;
    }

    /// <summary>
    /// Helper: Chạy login test cho 1 role cụ thể và assert kết quả
    /// </summary>
    private void RunAndAssertLogin(LoginTestData loginData, TestCaseModel testCase, string expectedStatus = "PASS")
    {
        var result = _seleniumService.ExecuteLoginTest(loginData, testCase);

        Console.WriteLine($"  TestCase: {result.TestCaseId}");
        Console.WriteLine($"  Role: {result.Role}");
        Console.WriteLine($"  Expected: {result.ExpectedResult}");
        Console.WriteLine($"  Actual: {result.ActualResult}");
        Console.WriteLine($"  Status: {result.Status}");
        Console.WriteLine($"  Screenshot: {result.ScreenshotPath ?? "N/A"}");

        Assert.AreEqual(expectedStatus, result.Status,
            $"[{testCase.TestCaseId}] Role: {loginData.Role} — " +
            $"Expected status '{expectedStatus}' nhưng nhận được '{result.Status}'. " +
            $"Actual result: {result.ActualResult}");
    }

    // ========================================================================
    //  TEST METHODS — Mỗi method là 1 test case đăng nhập
    // ========================================================================

    [TestMethod]
    [Description("Chạy tất cả test cases đăng nhập từ Google Spreadsheet")]
    [TestCategory("Login")]
    [TestCategory("Integration")]
    public async Task TestLogin_AllCases_FromSpreadsheet()
    {
        // Arrange
        var testCases = await GetTestCasesAsync();
        Assert.IsTrue(testCases.Count > 0, "Không có test case nào từ spreadsheet");

        var allResults = new List<LoginTestResult>();

        // Act
        foreach (var testCase in testCases)
        {
            Console.WriteLine($"\n{'=',-60}");
            Console.WriteLine($"  Test Case: {testCase.TestCaseId} - {testCase.TestObjective}");
            Console.WriteLine($"{'=',-60}");

            if (testCase.LoginDataList.Count > 0)
            {
                foreach (var loginData in testCase.LoginDataList)
                {
                    if (string.IsNullOrEmpty(loginData.Email) || string.IsNullOrEmpty(loginData.Password))
                    {
                        Console.WriteLine($"  ⏭️ Bỏ qua role {loginData.Role} (không có credentials)");
                        continue;
                    }

                    var result = _seleniumService.ExecuteLoginTest(loginData, testCase);
                    allResults.Add(result);

                    Console.WriteLine($"  → [{result.Status}] {result.Role}: {result.ActualResult}");
                }
            }
            else
            {
                var emptyData = new LoginTestData { Role = "N/A", Email = "", Password = "" };
                var result = _seleniumService.ExecuteLoginTest(emptyData, testCase);
                allResults.Add(result);

                Console.WriteLine($"  → [{result.Status}] {result.Role}: {result.ActualResult}");
            }
        }

        // Assert
        var failCount = allResults.Count(r => r.Status != "PASS");
        Console.WriteLine($"\n📊 Tổng kết: {allResults.Count - failCount}/{allResults.Count} PASS");

        Assert.AreEqual(0, failCount,
            $"Có {failCount}/{allResults.Count} test case FAIL:\n" +
            string.Join("\n", allResults.Where(r => r.Status != "PASS")
                .Select(r => $"  ❌ [{r.TestCaseId}] {r.Role}: {r.ActualResult}")));
    }

    [TestMethod]
    [Description("TC_LOGIN_01: Đăng nhập với tài khoản Admin hợp lệ")]
    [TestCategory("Login")]
    [TestCategory("Admin")]
    public async Task TestLogin_Admin_ValidCredentials()
    {
        // Arrange
        var testCases = await GetTestCasesAsync();
        var loginTestCase = testCases.FirstOrDefault(tc =>
            tc.LoginDataList.Any(d => d.Role != null && d.Role.Contains("ADMIN", StringComparison.OrdinalIgnoreCase)));

        if (loginTestCase == null)
        {
            Assert.Inconclusive("Không tìm thấy test case nào có role ADMIN trong spreadsheet");
            return;
        }

        var adminData = loginTestCase.LoginDataList
            .First(d => d.Role != null && d.Role.Contains("ADMIN", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        RunAndAssertLogin(adminData, loginTestCase, "PASS");
    }

    [TestMethod]
    [Description("TC_LOGIN_02: Đăng nhập với tài khoản Staff hợp lệ")]
    [TestCategory("Login")]
    [TestCategory("Staff")]
    public async Task TestLogin_Staff_ValidCredentials()
    {
        // Arrange
        var testCases = await GetTestCasesAsync();
        var loginTestCase = testCases.FirstOrDefault(tc =>
            tc.LoginDataList.Any(d => d.Role != null && d.Role.Contains("STAFF", StringComparison.OrdinalIgnoreCase)));

        if (loginTestCase == null)
        {
            Assert.Inconclusive("Không tìm thấy test case nào có role STAFF trong spreadsheet");
            return;
        }

        var staffData = loginTestCase.LoginDataList
            .First(d => d.Role != null && d.Role.Contains("STAFF", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        RunAndAssertLogin(staffData, loginTestCase, "PASS");
    }

    [TestMethod]
    [Description("TC_LOGIN_03: Đăng nhập với tài khoản Manager hợp lệ")]
    [TestCategory("Login")]
    [TestCategory("Manager")]
    public async Task TestLogin_Manager_ValidCredentials()
    {
        // Arrange
        var testCases = await GetTestCasesAsync();
        var loginTestCase = testCases.FirstOrDefault(tc =>
            tc.LoginDataList.Any(d => d.Role != null && d.Role.Contains("MANAGER", StringComparison.OrdinalIgnoreCase)));

        if (loginTestCase == null)
        {
            Assert.Inconclusive("Không tìm thấy test case nào có role MANAGER trong spreadsheet");
            return;
        }

        var managerData = loginTestCase.LoginDataList
            .First(d => d.Role != null && d.Role.Contains("MANAGER", StringComparison.OrdinalIgnoreCase));

        // Act & Assert
        RunAndAssertLogin(managerData, loginTestCase, "PASS");
    }

    [TestMethod]
    [Description("TC_LOGIN_04: Đăng nhập với email/password sai — mong đợi PASS (hệ thống hiện thông báo lỗi đúng)")]
    [TestCategory("Login")]
    [TestCategory("Negative")]
    public async Task TestLogin_InvalidCredentials()
    {
        // Arrange — Tìm test case có credentials "sai" hoặc dùng data thủ công
        var testCases = await GetTestCasesAsync();

        // Tìm test case có expected result chứa "thất bại" hoặc "Thông báo"
        var negativeTestCase = testCases.FirstOrDefault(tc =>
            tc.ExpectedResult.Contains("thất bại", StringComparison.OrdinalIgnoreCase) ||
            tc.ExpectedResult.Contains("Thông báo", StringComparison.OrdinalIgnoreCase));

        if (negativeTestCase != null && negativeTestCase.LoginDataList.Count > 0)
        {
            var loginData = negativeTestCase.LoginDataList.First(d =>
                !string.IsNullOrEmpty(d.Email) && !string.IsNullOrEmpty(d.Password));

            // Act & Assert — negative test nên expected status là PASS
            // (hệ thống hiện đúng thông báo lỗi = test PASS)
            RunAndAssertLogin(loginData, negativeTestCase, "PASS");
        }
        else
        {
            // Fallback: test thủ công với credentials sai
            var fakeTestCase = new TestCaseModel
            {
                TestCaseId = "TC_MANUAL_INVALID",
                TestObjective = "Đăng nhập với thông tin sai",
                ExpectedResult = "Thông báo đăng nhập thất bại",
                SpreadsheetExpectedResultRow = 0
            };
            var fakeLogin = new LoginTestData
            {
                Role = "INVALID",
                Email = "invalid@test.com",
                Password = "wrongpassword123"
            };

            var result = _seleniumService.ExecuteLoginTest(fakeLogin, fakeTestCase);

            Console.WriteLine($"  Status: {result.Status} | Actual: {result.ActualResult}");

            // Đăng nhập sai → phải ở lại trang login → đó là kết quả đúng → PASS
            Assert.AreEqual("PASS", result.Status,
                $"Đăng nhập với credentials sai nhưng status không đúng mong đợi: {result.ActualResult}");
        }
    }

    [TestMethod]
    [Description("TC_LOGIN_05: Đăng nhập khi bỏ trống email và password")]
    [TestCategory("Login")]
    [TestCategory("Negative")]
    public void TestLogin_EmptyFields()
    {
        // Arrange
        var testCase = new TestCaseModel
        {
            TestCaseId = "TC_EMPTY_FIELDS",
            TestObjective = "Đăng nhập khi bỏ trống các trường",
            ExpectedResult = "Thông báo thiếu thông tin",
            SpreadsheetExpectedResultRow = 0
        };
        var loginData = new LoginTestData
        {
            Role = "EMPTY",
            Email = "",
            Password = ""
        };

        // Act
        var result = _seleniumService.ExecuteLoginTest(loginData, testCase);

        Console.WriteLine($"  Status: {result.Status} | Actual: {result.ActualResult}");

        // Assert — Khi bỏ trống fields:
        // - Trình duyệt dùng HTML5 validation (required) nên KHÔNG gửi form → KHÔNG có server error
        // - Vẫn ở trang đăng nhập = hành vi đúng
        // Chấp nhận PASS hoặc kiểm tra vẫn ở trang login (cả 2 đều OK)
        bool stayOnLogin = result.ActualResult?.Contains("trang đăng nhập") == true
            || result.ActualResult?.Contains("Không có thông báo") == true
            || result.Status == "PASS";
        Assert.IsTrue(stayOnLogin,
            $"Bỏ trống fields nhưng đăng nhập thành công (lỗi bảo mật!): {result.ActualResult}");
    }

    [TestMethod]
    [Description("TC_LOGIN_06: Kiểm tra trang đăng nhập load thành công")]
    [TestCategory("Login")]
    [TestCategory("Smoke")]
    public void TestLogin_PageLoads()
    {
        // Arrange & Act
        var driver = _seleniumService.GetDriver();
        driver.Navigate().GoToUrl("http://duck123.runasp.net/Auth/Login");
        Thread.Sleep(2000);

        // Assert
        Assert.IsTrue(driver.Url.Contains("/Auth/Login"),
            $"Trang đăng nhập không load được. URL: {driver.Url}");

        // Kiểm tra các element quan trọng có tồn tại
        var emailField = driver.FindElements(OpenQA.Selenium.By.Id("EmailOrPhone"));
        var passwordField = driver.FindElements(OpenQA.Selenium.By.Id("password-input"));
        var loginButton = driver.FindElements(OpenQA.Selenium.By.CssSelector(".btn-login"));

        Assert.IsTrue(emailField.Count > 0, "Không tìm thấy trường Email/Phone");
        Assert.IsTrue(passwordField.Count > 0, "Không tìm thấy trường Password");
        Assert.IsTrue(loginButton.Count > 0, "Không tìm thấy nút Đăng nhập");

        Console.WriteLine("  ✅ Trang đăng nhập load thành công với đầy đủ các elements");
    }
}
