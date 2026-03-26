using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class ProcessReportDto
{
    public int ReportId { get; set; }
    public int CollectorId { get; set; }
    public ReportStatus Status { get; set; }
    public string? Note { get; set; }
    public string? CollectorImageUrl { get; set; }
}
