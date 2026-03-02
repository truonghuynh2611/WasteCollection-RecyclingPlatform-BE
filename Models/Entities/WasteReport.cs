namespace WasteReportApp.Models.Entities
{
    public class WasteReport
    {
        public int ReportId { get; set; } // PK
        public int CitizenId { get; set; }
        public int AreaId { get; set; }

        public string? ImageUrl { get; set; }
        public string Description { get; set; }
        public string WasteType { get; set; }
        public int? CollectorId { get; set; }
        public Collector? Collector { get; set; }
        public string? CollectorImageUrl { get; set; }


        public decimal CitizenLatitude { get; set; }
        public decimal CitizenLongitude { get; set; }
        public decimal? CollectorLatitude { get; set; }
        public decimal? CollectorLongitude { get; set; }
        public ReportStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireTime { get; set; }

        public Citizen Citizen { get; set; }
        public Area Area { get; set; }

    }
}
