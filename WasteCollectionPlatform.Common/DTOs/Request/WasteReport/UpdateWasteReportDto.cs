using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class UpdateWasteReportDto
{
    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string WasteType { get; set; } = null!;

    public int AreaId { get; set; }

}
