using System.Text;
using Dam_Bao_Chat_Luong.Enums;
using Dam_Bao_Chat_Luong.Models;
using Newtonsoft.Json;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using TestResult = Dam_Bao_Chat_Luong.Models.TestResult;

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

            // Yêu cầu thêm quyền DriveFile để upload ảnh
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                new[] { SheetsService.Scope.Spreadsheets, Google.Apis.Drive.v3.DriveService.Scope.DriveFile },
                "user",
                CancellationToken.None,
                new FileDataStore("token_store", true)
            );

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✅ Xác thực thành công!");
            Console.ResetColor();

            var sheetName = SheetNameMap[sheetType];

            var sheetsService = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Dam_Bao_Chat_Luong_Test"
            });

            var driveService = new Google.Apis.Drive.v3.DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Dam_Bao_Chat_Luong_Test"
            });

            // Gom kết quả theo SpreadsheetRow
            var resultsByRow = results
                .GroupBy(r => r.SpreadsheetRow)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Tạo hoặc lấy ID thư mục lưu ảnh trên Drive để khỏi bừa bộn
            var folderId = await GetOrCreateDriveFolderAsync("Dam_Bao_Chat_Luong_Screenshots", driveService);

            foreach (var (row, rowResults) in resultsByRow)
            {
                // === Cột J: Actual Result ===
                var actualResultContent = string.Join("\n---\n",
                    rowResults.Select(r => $"[{r.Role ?? "N/A"}]: {r.ActualResult}"));

                // === Cột K: Status (nếu có bất kỳ FAIL/ERROR → FAIL, ngược lại PASS) ===
                var hasFailOrError = rowResults.Any(r => r.Status == "FAIL" || r.Status == "ERROR");
                var statusContent = hasFailOrError ? "FAIL" : "PASS";

                // === Cột L: Notes (ảnh screenshot) ===
                var imageFormulas = new List<string>();
                foreach (var r in rowResults)
                {
                    if (!string.IsNullOrEmpty(r.ScreenshotPath) && System.IO.File.Exists(r.ScreenshotPath))
                    {
                        var fileId = await UploadScreenshotToDriveAsync(r.ScreenshotPath, driveService, folderId);
                        if (!string.IsNullOrEmpty(fileId))
                        {
                            imageFormulas.Add($"https://drive.google.com/uc?export=view&id={fileId}");
                        }
                    }
                }

                // Nội dung cột Notes: nếu có ảnh thì dùng =IMAGE(), nếu không thì ghi thông tin thời gian + role
                string notesContent;
                if (imageFormulas.Count > 0)
                {
                    // Chỉ lấy ảnh đầu tiên cho =IMAGE() (Sheets chỉ hiển thị 1 ảnh per cell)
                    // Các link ảnh còn lại ghi thêm bên dưới
                    notesContent = $"=IMAGE(\"{imageFormulas[0]}\")";
                }
                else
                {
                    notesContent = string.Join("\n", rowResults.Select(r => r.ToNotesString()));
                }

                // Ghi vào 3 cột J, K, L
                var rowValues = new List<object> { actualResultContent, statusContent, notesContent };
                var range = $"'{sheetName}'!J{row}:L{row}";

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange
                {
                    Values = new List<IList<object>> { rowValues }
                };

                var request = sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
                // Bắt buộc dùng USER_ENTERED thì hàm =IMAGE mới hoạt động
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                await request.ExecuteAsync();

                Console.WriteLine($"  ✅ Đã ghi: Row {row} | Actual Result → J | Status → K ({statusContent}) | Notes → L");
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

    /// <summary>
    /// Tìm hoặc tạo thư mục trên Google Drive
    /// </summary>
    private async Task<string?> GetOrCreateDriveFolderAsync(string folderName, Google.Apis.Drive.v3.DriveService driveService)
    {
        try
        {
            var request = driveService.Files.List();
            request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
            request.Spaces = "drive";
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            if (result.Files != null && result.Files.Count > 0)
            {
                return result.Files[0].Id;
            }

            Console.WriteLine($"  📁 Đang tạo thư mục mới trên Drive: {folderName}...");
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            var createRequest = driveService.Files.Create(fileMetadata);
            createRequest.Fields = "id";
            var file = await createRequest.ExecuteAsync();

            return file.Id;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠️ Lỗi tạo folder: {ex.Message} (Sẽ upload ra thư mục gốc)");
            Console.ResetColor();
            return null;
        }
    }

    /// <summary>
    /// Upload file ảnh lên Google Drive và trả về ID của file
    /// </summary>
    private async Task<string?> UploadScreenshotToDriveAsync(string localPath, Google.Apis.Drive.v3.DriveService driveService, string? folderId)
    {
        try
        {
            Console.WriteLine($"  ☁️ Đang upload ảnh lên Drive: {System.IO.Path.GetFileName(localPath)}...");
            
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = System.IO.Path.GetFileName(localPath),
                MimeType = "image/png"
            };

            if (!string.IsNullOrEmpty(folderId))
            {
                fileMetadata.Parents = new List<string> { folderId };
            }

            Google.Apis.Drive.v3.FilesResource.CreateMediaUpload request;
            using (var stream = new System.IO.FileStream(localPath, FileMode.Open))
            {
                request = driveService.Files.Create(fileMetadata, stream, "image/png");
                request.Fields = "id";
                await request.UploadAsync();
            }

            var file = request.ResponseBody;
            if (file == null) return null;

            // Mở quyền truy cập cho tất cả mọi người (ai có link đều xem được) để hàm =IMAGE hoạt động
            var permission = new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            };
            await driveService.Permissions.Create(permission, file.Id).ExecuteAsync();

            Console.WriteLine($"  ✅ Upload ảnh thành công!");
            return file.Id;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ❌ Lỗi upload Drive: {ex.Message}");
            Console.ResetColor();
            return null;
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
                    // Ghi nhận dòng chứa Expected Result (ngang hàng với step này)
                    currentTestCase.SpreadsheetExpectedResultRow = i + 1;
                }
            }
        }

        return testCases;
    }

    #endregion
}