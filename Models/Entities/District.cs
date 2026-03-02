
namespace WasteReportApp.Models.Entities
{
    public class District
    {
        public int DistrictId { get; set; } // PK
        public string DistrictName { get; set; }


        public ICollection<Area> Areas { get; set; }

    }
}
