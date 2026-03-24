using Microsoft.AspNetCore.Http;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class CreateWasteReportDto
{
    public int CitizenId { get; set; }
    public int AreaId { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string WasteType { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public IFormFile? ImageFile { get; set; }
}
