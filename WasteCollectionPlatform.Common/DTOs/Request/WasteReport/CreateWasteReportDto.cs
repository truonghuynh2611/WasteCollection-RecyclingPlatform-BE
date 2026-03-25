using Microsoft.AspNetCore.Http;

namespace WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

public class CreateWasteReportDto
{
    public int CitizenId { get; set; }
    public int AreaId { get; set; }
    public List<WasteReportItemDto> Items { get; set; } = new List<WasteReportItemDto>();
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
