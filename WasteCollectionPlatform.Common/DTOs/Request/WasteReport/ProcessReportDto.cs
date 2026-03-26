namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class ProcessReportDto
{
    public int ReportId { get; set; }
    public int CollectorId { get; set; }
    public bool IsValid { get; set; }
    public string? CollectorImageUrl { get; set; }
}
