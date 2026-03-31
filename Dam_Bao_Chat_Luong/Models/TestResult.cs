namespace Dam_Bao_Chat_Luong.Models;

public class TestResult
{
    public string TestCaseId { get; set; } = "";
    public string TestObjective { get; set; } = "";
    public string? Role { get; set; }
    public string Status { get; set; } = ""; // PASS / FAIL / ERROR
    public string ActualResult { get; set; } = "";
    public string ExpectedResult { get; set; } = "";
    public bool IsMatch { get; set; }
    public string? ScreenshotPath { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Dòng trong spreadsheet tương ứng (1-indexed)
    /// </summary>
    public int SpreadsheetRow { get; set; }

    public string ToNotesString()
    {
        var lines = new List<string>
        {
            $"[{Timestamp:yyyy-MM-dd HH:mm:ss}]",
            $"Role: {Role ?? "N/A"}"
        };

        return string.Join("\n", lines);
    }
}
