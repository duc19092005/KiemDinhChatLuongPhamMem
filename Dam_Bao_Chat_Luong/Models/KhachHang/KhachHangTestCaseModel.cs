namespace Dam_Bao_Chat_Luong.Models.KhachHang;

/// <summary>
/// Model đại diện cho 1 test case khách hàng.
/// Khác với LoginTestCase: hỗ trợ nhiều Expected Results trên nhiều rows,
/// test data dạng JSON object (không phải array).
/// </summary>
public class KhachHangTestCaseModel
{
    public string No { get; set; } = "";                        // II.1, II.2, ...
    public string TestRequirementId { get; set; } = "";         // II.1_DangKy, II.3_TimKiem, ...
    public string TestCaseId { get; set; } = "";                // II.1_DK_01, II.3_TK_01, ...
    public string TestObjective { get; set; } = "";
    public string PreConditions { get; set; } = "";
    public List<KhachHangTestStep> Steps { get; set; } = new();

    /// <summary>Dòng đầu tiên của test case trên spreadsheet (1-indexed)</summary>
    public int SpreadsheetStartRow { get; set; }

    /// <summary>Dòng cuối cùng (step cuối) trên spreadsheet (1-indexed)</summary>
    public int SpreadsheetEndRow { get; set; }
}

/// <summary>
/// Một bước (step) của test case khách hàng.
/// Mỗi step tương ứng với 1 row trên spreadsheet.
/// </summary>
public class KhachHangTestStep
{
    public int StepNumber { get; set; }
    public string Action { get; set; } = "";
    public string? TestDataRaw { get; set; }

    /// <summary>Expected result nếu có ở step này (có thể null)</summary>
    public string? ExpectedResult { get; set; }

    /// <summary>Row trên spreadsheet tương ứng (1-indexed)</summary>
    public int SpreadsheetRow { get; set; }
}
