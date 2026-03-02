using System.Text.Json.Serialization;

namespace WasteReportApp.Models.Entities
{
    public class Area
    {
        public int AreaId { get; set; } // PK
        public int DistrictId { get; set; }
        public string Name { get; set; }

        public District District { get; set; }
        [JsonIgnore]
        public ICollection<WasteReport> WasteReports { get; set; }

    }
}
