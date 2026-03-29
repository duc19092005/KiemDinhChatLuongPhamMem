namespace Dam_Bao_Chat_Luong.Models;

public class TestCaseModel
{
    public int No { get; set; }
    public string TestRequirementId { get; set; } = "";
    public string TestCaseId { get; set; } = "";
    public string TestObjective { get; set; } = "";
    public string PreConditions { get; set; } = "";
    public List<TestStep> Steps { get; set; } = new();
    public List<LoginTestData> LoginDataList { get; set; } = new();
    public string ExpectedResult { get; set; } = "";
    /// <summary>
    /// Dòng bắt đầu của test case trong spreadsheet (1-indexed) để ghi kết quả vào cột Notes
    /// </summary>
    public int SpreadsheetStartRow { get; set; }
}

public class TestStep
{
    public int StepNumber { get; set; }
    public string Action { get; set; } = "";
    public string? TestDataRaw { get; set; }
    public string? ExpectedResult { get; set; }
}
