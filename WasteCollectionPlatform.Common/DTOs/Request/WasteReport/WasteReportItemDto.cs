using Microsoft.AspNetCore.Http;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class WasteReportItemDto
{
    public string WasteType { get; set; } = null!;
    public string? Description { get; set; }
    public IFormFile? ImageFile { get; set; }
}
