namespace Dam_Bao_Chat_Luong.Models.KhachHang;

/// <summary>
/// Kết quả test của 1 test case khách hàng.
/// Chứa kết quả từng step và status tổng.
/// </summary>
public class KhachHangTestResult
{
    public string TestCaseId { get; set; } = "";
    public string TestObjective { get; set; } = "";

    /// <summary>PASS / FAIL — status tổng cho cả test case</summary>
    public string Status { get; set; } = "";

    /// <summary>Kết quả từng step (chỉ những step có Expected Result)</summary>
    public List<StepResult> StepResults { get; set; } = new();

    /// <summary>Danh sách screenshot paths</summary>
    public List<string> ScreenshotPaths { get; set; } = new();

    /// <summary>Dòng đầu tiên trên spreadsheet (dùng để merge cells)</summary>
    public int SpreadsheetStartRow { get; set; }

    /// <summary>Dòng cuối cùng trên spreadsheet (dùng để merge cells)</summary>
    public int SpreadsheetEndRow { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Kết quả 1 step cụ thể — dùng để ghi Actual Result vào đúng row trên spreadsheet
/// </summary>
public class StepResult
{
    public int StepNumber { get; set; }

    /// <summary>Kết quả thực tế — ghi vào cột J (Actual Result)</summary>
    public string? ActualResult { get; set; }

    /// <summary>Nếu test data được thay thế → ghi giá trị mới vào cột H (Test Data)</summary>
    public string? ReplacedTestData { get; set; }

    /// <summary>Row trên spreadsheet (1-indexed)</summary>
    public int SpreadsheetRow { get; set; }
}
