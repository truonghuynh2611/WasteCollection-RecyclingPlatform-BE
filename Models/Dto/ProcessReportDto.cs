namespace WasteReportApp.Models.Dto
{
    public class ProcessReportDto
    {
        public int ReportId { get; set; }
        public int CollectorId { get; set; }
        public bool IsValid { get; set; }
        public string? CollectorImageUrl { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
