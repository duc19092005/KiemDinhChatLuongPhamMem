using System.Text;
using Dam_Bao_Chat_Luong.Models;
using Dam_Bao_Chat_Luong.Services.ThirdPersonServices;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace Dam_Bao_Chat_Luong.Services;

public class NhanVienBanVeService
{
    private readonly GoogleSpreadSheetService _spreadsheetEngine;
    private readonly IConfiguration _gDriveConfig;
    private const string SHEET_NAME = "NHÂN VIÊN BÁN VÉ";
    private const string GID = "1870381428";
    public NhanVienBanVeService()
    {
        _spreadsheetEngine = new GoogleSpreadSheetService();

        _gDriveConfig = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("gdrive_credentials.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public async Task<List<TestCaseModel>> GetTestCasesAsync()
    {
        var csvContent = await _spreadsheetEngine.GetCsvContentAsync(GID);
        var rows = ParseCsv(csvContent);
        return ParseTestCasesInternal(rows);
    }

    public async Task WriteActualResultAsync(int row, string message)
    {
        await _spreadsheetEngine.UpdateCellAsync(SHEET_NAME, "J", row, message);
    }

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
                    if (i + 1 < csvContent.Length && csvContent[i + 1] == '"') { currentField.Append('"'); i++; }
                    else inQuotes = false;
                }
                else currentField.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { currentRow.Add(currentField.ToString()); currentField.Clear(); }
                else if (c == '\n')
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    rows.Add(currentRow.ToArray());
                    currentRow = new List<string>();
                }
                else if (c != '\r') currentField.Append(c);
            }
        }
        if (currentField.Length > 0 || currentRow.Count > 0) { currentRow.Add(currentField.ToString()); rows.Add(currentRow.ToArray()); }
        return rows;
    }

    private List<TestCaseModel> ParseTestCasesInternal(List<string[]> rows)
    {
        var testCases = new List<TestCaseModel>();
        TestCaseModel? currentTC = null;

        Console.WriteLine("\n=== BẢN ĐỒ DÒNG TEST CASE (Kiểm tra tại đây) ===");

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Length < 3) continue;

            string noValue = row[0].Trim();
            string tcId = row[2].Trim();
            string stepNumStr = row.Length > 5 ? row[5].Trim() : "";

            // Kiểm tra an toàn: Chỉ tạo TC khi No là số và TC_ID không trống
            if (!string.IsNullOrEmpty(noValue) && int.TryParse(noValue, out int no) && !string.IsNullOrEmpty(tcId))
            {
                currentTC = new TestCaseModel
                {
                    No = no,
                    TestCaseId = tcId,
                    Steps = new(),
                    SpreadsheetStartRow = i + 1
                };
                testCases.Add(currentTC);
            }

            // Thêm Step vào TC hiện tại (xử lý cả khi ô gộp bị trống)
            if (currentTC != null && !string.IsNullOrEmpty(stepNumStr) && int.TryParse(stepNumStr, out int stepNum))
            {
                currentTC.Steps.Add(new TestStep
                {
                    StepNumber = stepNum,
                    TestDataRaw = row.Length > 7 ? row[7].Trim() : ""
                });
                // Cập nhật dòng kết quả theo step cuối cùng tìm thấy
                currentTC.SpreadsheetExpectedResultRow = i + 1;
            }
        }

        foreach (var tc in testCases)
        {
            Console.WriteLine($"TC: {tc.TestCaseId} | Ghi kết quả vào dòng: {tc.SpreadsheetExpectedResultRow}");
        }
        return testCases;
    }
    public async Task<string> UploadToDriveAsync(byte[] imageBytes, string fileName)
    {
        var clientId = _gDriveConfig["GoogleDrive:ClientId"];
        var clientSecret = _gDriveConfig["GoogleDrive:ClientSecret"];

        if (string.IsNullOrEmpty(clientId)) throw new Exception("❌ Chưa cấu hình ClientId trong gdrive_credentials.json!");

        // Xác thực quyền Drive
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
            new[] { DriveService.Scope.DriveFile },
            "user", CancellationToken.None, new FileDataStore("token_store_nhanvien", true)
        );

        var driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "NhanVien_SQA_Test"
        });

        var fileMetadata = new Google.Apis.Drive.v3.Data.File { Name = fileName, MimeType = "image/png" };

        using (var stream = new MemoryStream(imageBytes))
        {
            var request = driveService.Files.Create(fileMetadata, stream, "image/png");
            request.Fields = "id";
            var progress = await request.UploadAsync();

            if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
                throw new Exception($" Lỗi Drive: {progress.Exception.Message}");

            var file = request.ResponseBody;

            // Mở quyền để link ảnh hiển thị được trong Google Sheets
            await driveService.Permissions.Create(new Google.Apis.Drive.v3.Data.Permission
            {
                Type = "anyone",
                Role = "reader"
            }, file.Id).ExecuteAsync();

            return $"https://drive.google.com/uc?export=view&id={file.Id}";
        }
    }

    public async Task WriteScreenshotResultAsync(int row, string driveUrl)
    {
        // Ghi công thức IMAGE vào cột L (Notes)
        await _spreadsheetEngine.UpdateCellAsync(SHEET_NAME, "L", row, $"{driveUrl}");
    }
}