using Dam_Bao_Chat_Luong.Models.KhachHang;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;

namespace Dam_Bao_Chat_Luong.Services.KhachHang;

/// <summary>
/// Service ghi kết quả test khách hàng lên Google Spreadsheet.
/// Hỗ trợ: ghi Actual Result per-step, merge cells Status/Notes, upload screenshot.
/// </summary>
public class KhachHangExcelWriterService
{
    private readonly string _spreadsheetId = "1oG1OjLR2BR-RsCnU7DS4wMt7P22xjt16yaln69VsFHc";
    private readonly string _sheetName = "Khách hàng";
    private readonly int _sheetId = 364798902; // gid
    private readonly IConfiguration? _configuration;

    public KhachHangExcelWriterService()
    {
        try
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();
        }
        catch { _configuration = null; }
    }

    public async Task<bool> WriteTestResult(KhachHangTestResult result)
    {
        return await WriteAllResults(new List<KhachHangTestResult> { result });
    }

    public async Task<bool> WriteAllResults(List<KhachHangTestResult> results)
    {
        var clientId = _configuration?["Google:ClientId"];
        var clientSecret = _configuration?["Google:ClientSecret"];
        if (string.IsNullOrEmpty(clientId) || clientId.Contains("YOUR_") ||
            string.IsNullOrEmpty(clientSecret) || clientSecret.Contains("YOUR_"))
        {
            Console.WriteLine("⚠️ Chưa cấu hình Google OAuth2. Bỏ qua ghi spreadsheet.");
            return false;
        }

        try
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                new[] { SheetsService.Scope.Spreadsheets, DriveService.Scope.DriveFile },
                "user", CancellationToken.None, new FileDataStore("token_store", true));

            var sheetsService = new SheetsService(new BaseClientService.Initializer
                { HttpClientInitializer = credential, ApplicationName = "KhachHang_Test" });
            var driveService = new DriveService(new BaseClientService.Initializer
                { HttpClientInitializer = credential, ApplicationName = "KhachHang_Test" });

            var folderId = await GetOrCreateDriveFolder("KhachHang_Screenshots", driveService);

            foreach (var result in results)
            {
                await WriteOneResult(result, sheetsService, driveService, folderId);
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Lỗi ghi spreadsheet: {ex.Message}");
            return false;
        }
    }

    private async Task WriteOneResult(KhachHangTestResult result,
        SheetsService sheets, DriveService drive, string? folderId)
    {
        int startRow = result.SpreadsheetStartRow;
        int endRow = result.SpreadsheetEndRow;

        // 1. Ghi Actual Result (cột J) cho từng step có kết quả
        foreach (var sr in result.StepResults)
        {
            if (sr.SpreadsheetRow <= 0 || string.IsNullOrEmpty(sr.ActualResult)) continue;

            var range = $"'{_sheetName}'!J{sr.SpreadsheetRow}";
            await UpdateCell(sheets, range, sr.ActualResult);

            // Nếu test data được thay thế → ghi vào cột H
            if (!string.IsNullOrEmpty(sr.ReplacedTestData))
            {
                var hRange = $"'{_sheetName}'!H{sr.SpreadsheetRow}";
                await UpdateCell(sheets, hRange, sr.ReplacedTestData);
            }
        }

        // 2. Upload screenshots + tạo nội dung Notes
        string notesContent = "";
        if (result.ScreenshotPaths.Count > 0)
        {
            var firstScreenshot = result.ScreenshotPaths.First();
            if (File.Exists(firstScreenshot))
            {
                var fileId = await UploadToDrive(firstScreenshot, drive, folderId);
                if (!string.IsNullOrEmpty(fileId))
                    notesContent = $"=IMAGE(\"https://drive.google.com/uc?export=view&id={fileId}\")";
            }
        }
        if (string.IsNullOrEmpty(notesContent))
            notesContent = $"[{result.Timestamp:yyyy-MM-dd HH:mm:ss}] Auto-test";

        // 3. Ghi Status (K) và Notes (L) vào row đầu tiên
        var klRange = $"'{_sheetName}'!K{startRow}:L{startRow}";
        var klValues = new ValueRange
        {
            Values = new List<IList<object>> { new List<object> { result.Status, notesContent } }
        };
        var klReq = sheets.Spreadsheets.Values.Update(klValues, _spreadsheetId, klRange);
        klReq.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest
            .ValueInputOptionEnum.USERENTERED;
        await klReq.ExecuteAsync();

        // 4. Merge cells Status (K) và Notes (L) nếu TC chiếm nhiều dòng
        if (endRow > startRow)
        {
            var mergeRequests = new List<Request>
            {
                // Unmerge trước (tránh lỗi nếu đã merge)
                new() { UnmergeCells = new UnmergeCellsRequest { Range = GridRange(startRow, endRow, 10, 11) } },
                new() { UnmergeCells = new UnmergeCellsRequest { Range = GridRange(startRow, endRow, 11, 12) } },
                // Merge lại
                new() { MergeCells = new MergeCellsRequest { Range = GridRange(startRow, endRow, 10, 11), MergeType = "MERGE_ALL" } },
                new() { MergeCells = new MergeCellsRequest { Range = GridRange(startRow, endRow, 11, 12), MergeType = "MERGE_ALL" } },
            };
            await sheets.Spreadsheets.BatchUpdate(
                new BatchUpdateSpreadsheetRequest { Requests = mergeRequests }, _spreadsheetId).ExecuteAsync();
        }

        Console.WriteLine($"  ✅ Đã ghi kết quả: {result.TestCaseId} | Status: {result.Status} | Rows: {startRow}-{endRow}");
    }

    private GridRange GridRange(int startRow1, int endRow1, int startCol, int endCol)
    {
        return new GridRange
        {
            SheetId = _sheetId,
            StartRowIndex = startRow1 - 1,  // convert 1→0 indexed
            EndRowIndex = endRow1,           // exclusive
            StartColumnIndex = startCol,
            EndColumnIndex = endCol
        };
    }

    private async Task UpdateCell(SheetsService sheets, string range, string value)
    {
        var vr = new ValueRange { Values = new List<IList<object>> { new List<object> { value } } };
        var req = sheets.Spreadsheets.Values.Update(vr, _spreadsheetId, range);
        req.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest
            .ValueInputOptionEnum.USERENTERED;
        await req.ExecuteAsync();
    }

    private async Task<string?> GetOrCreateDriveFolder(string name, DriveService drive)
    {
        try
        {
            var list = drive.Files.List();
            list.Q = $"mimeType='application/vnd.google-apps.folder' and name='{name}' and trashed=false";
            list.Fields = "files(id)";
            var res = await list.ExecuteAsync();
            if (res.Files?.Count > 0) return res.Files[0].Id;

            var meta = new Google.Apis.Drive.v3.Data.File { Name = name, MimeType = "application/vnd.google-apps.folder" };
            var cr = drive.Files.Create(meta); cr.Fields = "id";
            return (await cr.ExecuteAsync()).Id;
        }
        catch { return null; }
    }

    private async Task<string?> UploadToDrive(string path, DriveService drive, string? folderId)
    {
        try
        {
            var meta = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(path), MimeType = "image/png",
                Parents = folderId != null ? new List<string> { folderId } : null
            };
            using var stream = new FileStream(path, FileMode.Open);
            var req = drive.Files.Create(meta, stream, "image/png"); req.Fields = "id";
            await req.UploadAsync();
            var file = req.ResponseBody;
            if (file == null) return null;

            await drive.Permissions.Create(
                new Google.Apis.Drive.v3.Data.Permission { Type = "anyone", Role = "reader" },
                file.Id).ExecuteAsync();
            return file.Id;
        }
        catch (Exception ex) { Console.WriteLine($"  ⚠️ Upload lỗi: {ex.Message}"); return null; }
    }
}
