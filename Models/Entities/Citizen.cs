using System.Text.Json.Serialization;


namespace WasteReportApp.Models.Entities
{
    public class Citizen
    {
        public int CitizenId { get; set; }
        public int UserId { get; set; }
        public int TotalPoints { get; set; }
        [JsonIgnore]
        public ICollection<WasteReport> WasteReports { get; set; }

       


    }
}
