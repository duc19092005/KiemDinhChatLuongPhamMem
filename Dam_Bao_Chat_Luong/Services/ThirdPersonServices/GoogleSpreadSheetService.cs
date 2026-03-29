using System.Text;
using Dam_Bao_Chat_Luong.Enums;
using Dam_Bao_Chat_Luong.Models;
using Newtonsoft.Json;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace Dam_Bao_Chat_Luong.Services.ThirdPersonServices;

public class GoogleSpreadSheetService
{
    private readonly string _spreadsheetId = "1oG1OjLR2BR-RsCnU7DS4wMt7P22xjt16yaln69VsFHc";
    private readonly HttpClient _httpClient = new();
    private readonly IConfiguration? _configuration;

    // GID mapping cho từng sheet
    private static readonly Dictionary<GoogleSpreadSheetEnum, string> SheetGidMap = new()
    {
        { GoogleSpreadSheetEnum.DangNhap, "785755617" }
    };

    // Tên sheet tương ứng
    private static readonly Dictionary<GoogleSpreadSheetEnum, string> SheetNameMap = new()
    {
        { GoogleSpreadSheetEnum.DangNhap, "Đăng nhập" }
    };

    public GoogleSpreadSheetService()
    {
        // Load appsettings.Development.json
        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .Build();
        }
        catch
        {
            _configuration = null;
        }
    }

    /// <summary>
    /// Đọc test cases từ Google Spreadsheet (public, không cần credentials)
    /// </summary>
    public async Task<List<TestCaseModel>> GetTestCases(GoogleSpreadSheetEnum sheetType)
    {
        var gid = SheetGidMap[sheetType];
        var url = $"https://docs.google.com/spreadsheets/d/{_spreadsheetId}/export?format=csv&gid={gid}";

        Console.WriteLine($"  → Đang tải dữ liệu từ sheet: {SheetNameMap[sheetType]}...");
        var csvContent = await _httpClient.GetStringAsync(url);

        var rows = ParseCsv(csvContent);
        var testCases = ParseTestCases(rows);

        return testCases;
    }

    /// <summary>
    /// Ghi kết quả test vào cột Notes trên Google Spreadsheet
    /// Sử dụng OAuth2 User Credentials (ClientId + ClientSecret từ appsettings.Development.json)
    /// Lần đầu sẽ mở browser để user đăng nhập Google, sau đó token được cache lại
    /// </summary>
    public async Task<bool> WriteTestResults(GoogleSpreadSheetEnum sheetType, List<TestResult> results)
    {
        var clientId = _configuration?["Google:ClientId"];
        var clientSecret = _configuration?["Google:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || clientId == "YOUR_GOOGLE_CLIENT_ID_HERE"
            || string.IsNullOrEmpty(clientSecret) || clientSecret == "YOUR_GOOGLE_CLIENT_SECRET_HERE")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠️  Chưa cấu hình Google OAuth2 trong appsettings.Development.json.");
            Console.WriteLine("   Để ghi kết quả lên Google Spreadsheet, bạn cần:");
            Console.WriteLine("   1. Vào Google Cloud Console → APIs & Services → Credentials");
            Console.WriteLine("   2. Tạo OAuth 2.0 Client ID (loại Desktop app)");
            Console.WriteLine("   3. Copy Client ID và Client Secret");
            Console.WriteLine("   4. Điền vào file appsettings.Development.json:");
            Console.WriteLine("      {");
            Console.WriteLine("        \"Google\": {");
            Console.WriteLine("          \"ClientId\": \"YOUR_CLIENT_ID\",");
            Console.WriteLine("          \"ClientSecret\": \"YOUR_CLIENT_SECRET\"");
            Console.WriteLine("        }");
            Console.WriteLine("      }");
            Console.WriteLine("   5. Enable Google Sheets API trong Google Cloud Console");
            Console.WriteLine("\n   Kết quả đã được lưu tại local.");
            Console.ResetColor();
            return false;
        }

        try
        {
            Console.WriteLine("  🔐 Đang xác thực Google OAuth2...");
            Console.WriteLine("     (Lần đầu sẽ mở browser để bạn đăng nhập Google)");

            var clientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };

            // GoogleWebAuthorizationBroker sẽ:
            // - Lần đầu: Mở browser → user đăng nhập → lưu token vào thư mục "token_store"
            // - Lần sau: Dùng cached token (tự refresh nếu hết hạn)
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                new[] { SheetsService.Scope.Spreadsheets },
                "user",
                CancellationToken.None,
                new FileDataStore("token_store", true)
            );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✅ Xác thực thành công!");
            Console.ResetColor();

            var sheetName = SheetNameMap[sheetType];

            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Dam_Bao_Chat_Luong_Test"
            });

            // Gom kết quả theo SpreadsheetRow
            var resultsByRow = results
                .GroupBy(r => r.SpreadsheetRow)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var (row, rowResults) in resultsByRow)
            {
                var notesContent = string.Join("\n\n---\n\n",
                    rowResults.Select(r => r.ToNotesString()));

                // Cột Notes = cột J (column index 10 in 0-based, 'J' in A1 notation)
                var range = $"'{sheetName}'!J{row}";
                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Values = new List<IList<object>> { new List<object> { notesContent } }
                };

                var request = service.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await request.ExecuteAsync();

                Console.WriteLine($"  ✅ Đã ghi kết quả vào {range}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ❌ Lỗi khi ghi spreadsheet: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }


    #region CSV Parsing

    /// <summary>
    /// Parser CSV theo RFC 4180 — xử lý được fields có newline và double-quote
    /// </summary>
    private List<string[]> ParseCsv(string csvContent)
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csvContent.Length; i++)
        {
            char c = csvContent[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csvContent.Length && csvContent[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    currentField.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if (c == '\r')
                {
                    // Skip \r
                }
                else if (c == '\n')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                }
                else
                {
                    currentField.Append(c);
                }
            }
        }

        // Thêm field và row cuối cùng
        if (currentField.Length > 0 || currentRow.Count > 0)
        {
            currentRow.Add(currentField.ToString());
            rows.Add(currentRow.ToArray());
        }

        return rows;
    }

    /// <summary>
    /// Parse CSV rows thành danh sách TestCaseModel
    /// Cấu trúc: Row có No. → bắt đầu test case mới; row không có No. → tiếp tục steps
    /// </summary>
    private List<TestCaseModel> ParseTestCases(List<string[]> rows)
    {
        var testCases = new List<TestCaseModel>();
        TestCaseModel? currentTestCase = null;

        // Columns: 0=No., 1=TestReqId, 2=TestCaseId, 3=TestObjective, 4=PreConditions,
        //          5=Step#, 6=StepAction, 7=TestData, 8=ExpectedResult, 9=Notes

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Length < 9) continue;

            var noValue = row[0].Trim();
            var stepNumStr = row.Length > 5 ? row[5].Trim() : "";

            // Bỏ qua header và section header rows
            if (noValue == "No." || noValue == "Đăng nhập" || noValue == "Admin") continue;
            if (string.IsNullOrEmpty(noValue) && string.IsNullOrEmpty(stepNumStr)) continue;

            // Nếu có No. → bắt đầu test case mới
            if (!string.IsNullOrEmpty(noValue) && int.TryParse(noValue, out int no))
            {
                currentTestCase = new TestCaseModel
                {
                    No = no,
                    TestRequirementId = row[1].Trim(),
                    TestCaseId = row[2].Trim(),
                    TestObjective = row[3].Trim(),
                    PreConditions = row[4].Trim(),
                    SpreadsheetStartRow = i + 1 // 1-indexed (CSV row index + 1 vì spreadsheet row 1-indexed)
                };
                testCases.Add(currentTestCase);
            }

            // Parse step
            if (currentTestCase != null && int.TryParse(stepNumStr, out int stepNum))
            {
                var step = new TestStep
                {
                    StepNumber = stepNum,
                    Action = row.Length > 6 ? row[6].Trim() : "",
                    TestDataRaw = row.Length > 7 ? row[7].Trim() : null,
                    ExpectedResult = row.Length > 8 ? row[8].Trim() : null
                };

                currentTestCase.Steps.Add(step);

                // Parse JSON test data nếu có (step 2 thường chứa JSON login data)
                if (!string.IsNullOrEmpty(step.TestDataRaw) && step.TestDataRaw.TrimStart().StartsWith("["))
                {
                    try
                    {
                        var loginDataList = JsonConvert.DeserializeObject<List<LoginTestData>>(step.TestDataRaw);
                        if (loginDataList != null)
                        {
                            currentTestCase.LoginDataList = loginDataList;
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"  ⚠️ Không thể parse JSON test data: {ex.Message}");
                    }
                }

                // Lấy Expected Result từ step cuối cùng (step 3 thường có expected result)
                if (!string.IsNullOrEmpty(step.ExpectedResult))
                {
                    currentTestCase.ExpectedResult = step.ExpectedResult;
                }
            }
        }

        return testCases;
    }

    #endregion
}