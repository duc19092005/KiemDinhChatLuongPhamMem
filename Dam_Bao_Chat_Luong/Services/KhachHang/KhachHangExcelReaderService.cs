using System.Text;
using Dam_Bao_Chat_Luong.Models.KhachHang;

namespace Dam_Bao_Chat_Luong.Services.KhachHang;

/// <summary>
/// Service đọc test cases từ Google Spreadsheet — sheet "Khách hàng" (gid=364798902).
/// Độc lập với GoogleSpreadSheetService, không đụng vào service cũ.
/// </summary>
public class KhachHangExcelReaderService
{
    private readonly string _spreadsheetId = "1oG1OjLR2BR-RsCnU7DS4wMt7P22xjt16yaln69VsFHc";
    private readonly string _sheetGid = "364798902";
    private readonly HttpClient _httpClient = new();

    public async Task<List<KhachHangTestCaseModel>> GetTestCases()
    {
        var url = $"https://docs.google.com/spreadsheets/d/{_spreadsheetId}/export?format=csv&gid={_sheetGid}";
        Console.WriteLine("  → Đang tải dữ liệu từ sheet: Khách hàng...");
        var csvContent = await _httpClient.GetStringAsync(url);
        var rows = ParseCsv(csvContent);
        var testCases = ParseTestCases(rows);
        Console.WriteLine($"  ✅ Đã đọc {testCases.Count} test case(s)");
        return testCases;
    }

    public async Task<KhachHangTestCaseModel?> GetTestCaseById(string testCaseId)
    {
        var testCases = await GetTestCases();
        return testCases.FirstOrDefault(tc =>
            tc.TestCaseId.Equals(testCaseId, StringComparison.OrdinalIgnoreCase));
    }

    private List<KhachHangTestCaseModel> ParseTestCases(List<string[]> rows)
    {
        var testCases = new List<KhachHangTestCaseModel>();
        KhachHangTestCaseModel? currentTestCase = null;
        var skipValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "ĐĂNG KÝ", "TÌM KIẾM", "ĐẶT VÉ", "No.", "ĐĂNG NHẬP", "NAVIGATION", "QUẢN LÝ TÀI KHOẢN", "FLOW ĐẶT VÉ E2E" };

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Length < 6) continue;
            var noValue = row[0].Trim();
            var stepNumStr = row.Length > 5 ? row[5].Trim() : "";
            if (skipValues.Contains(noValue)) continue;
            if (string.IsNullOrEmpty(noValue) && string.IsNullOrEmpty(stepNumStr)) continue;

            if (!string.IsNullOrEmpty(noValue))
            {
                currentTestCase = new KhachHangTestCaseModel
                {
                    No = noValue,
                    TestRequirementId = row.Length > 1 ? row[1].Trim() : "",
                    TestCaseId = row.Length > 2 ? row[2].Trim() : "",
                    TestObjective = row.Length > 3 ? row[3].Trim() : "",
                    PreConditions = row.Length > 4 ? row[4].Trim() : "",
                    SpreadsheetStartRow = i + 1
                };
                testCases.Add(currentTestCase);
            }

            if (currentTestCase != null && int.TryParse(stepNumStr, out int stepNum))
            {
                var step = new KhachHangTestStep
                {
                    StepNumber = stepNum,
                    Action = row.Length > 6 ? row[6].Trim() : "",
                    TestDataRaw = row.Length > 7 && !string.IsNullOrWhiteSpace(row[7]) ? row[7].Trim() : null,
                    ExpectedResult = row.Length > 8 && !string.IsNullOrWhiteSpace(row[8]) ? row[8].Trim() : null,
                    SpreadsheetRow = i + 1
                };
                currentTestCase.Steps.Add(step);
                currentTestCase.SpreadsheetEndRow = i + 1;
            }
        }
        return testCases;
    }

    private List<string[]> ParseCsv(string csvContent)
    {
        var rows = new List<string[]>();
        var currentRow = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < csvContent.Length; i++)
        {
            char c = csvContent[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csvContent.Length && csvContent[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { currentRow.Add(field.ToString()); field.Clear(); }
                else if (c == '\r') { }
                else if (c == '\n') { currentRow.Add(field.ToString()); field.Clear(); rows.Add(currentRow.ToArray()); currentRow = new List<string>(); }
                else field.Append(c);
            }
        }
        if (field.Length > 0 || currentRow.Count > 0) { currentRow.Add(field.ToString()); rows.Add(currentRow.ToArray()); }
        return rows;
    }
}
