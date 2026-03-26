using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class CreateWasteReportDto
{
    public int CitizenId { get; set; }
    public int AreaId { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public string? Description { get; set; }
    public string? WasteType { get; set; }
    public List<WasteReportItemDto> Items { get; set; } = new List<WasteReportItemDto>();
}
