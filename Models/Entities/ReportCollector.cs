namespace WasteReportApp.Models.Entities
{
    public class ReportCollector
    {
        public int ReportId { get; set; }
        public WasteReport Report { get; set; }

        public int CollectorId { get; set; }
        public Collector Collector { get; set; }

        public bool IsCompleted { get; set; }
    }
}
