namespace WasteReportApp.Models.Entities
{
    public class ReportAssignment
    {
        public int Id { get; set; }

        public int WasteReportId { get; set; }
        public WasteReport WasteReport { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; }

        public bool IsMainTeam { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}
