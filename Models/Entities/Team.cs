using System.Text.Json.Serialization;

namespace WasteReportApp.Models.Entities
{
    public class Team
    {
        public int TeamId { get; set; }
        public int AreaId { get; set; }

        public string Name { get; set; }
        public TeamType TeamType { get; set; }   // nếu DB là enum thì ta chỉnh lại

        public Area Area { get; set; }

        [JsonIgnore]
        public ICollection<Collector> Collectors { get; set; }
    }
}
