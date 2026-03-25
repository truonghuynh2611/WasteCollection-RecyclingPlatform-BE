namespace WasteCollectionPlatform.Common.DTOs.Response.Collector;

public class CollectorTaskResponseDto
{
    public int ReportId { get; set; }
    public string? Address { get; set; }
    public string? Area { get; set; }
    public string? District { get; set; }
    public string? WasteType { get; set; }
    public string? Priority { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? AssignedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? Rating { get; set; }
}
