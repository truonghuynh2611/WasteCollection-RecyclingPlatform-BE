using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class RejectReportDto
{
    [Required]
    public string Reason { get; set; } = null!;
}
