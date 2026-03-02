namespace WasteReportApp.Models.Dto
{
    public class CreateWasteReportDto
    {
        public int CitizenId { get; set; }
        public int AreaId { get; set; }
        public string? ImageUrl { get; set; }
        public string Description { get; set; }
        public string WasteType { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
     
    }
}
