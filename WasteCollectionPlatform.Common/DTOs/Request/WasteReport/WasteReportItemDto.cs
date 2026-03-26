using Microsoft.AspNetCore.Http;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class WasteReportItemDto
{
    public string WasteType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IFormFile? ImageFile { get; set; }
}
