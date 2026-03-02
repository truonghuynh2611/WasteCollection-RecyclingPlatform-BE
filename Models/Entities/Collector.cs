using System.Text.Json.Serialization;

namespace WasteReportApp.Models.Entities
{
    public class Collector
    {
        public int CollectorId { get; set; }
        public int UserId { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public bool Status { get; set; }
        public int CurrentTaskCount { get; set; }

        [JsonIgnore]
        public ICollection<WasteReport> WasteReports { get; set; }
    }
}
