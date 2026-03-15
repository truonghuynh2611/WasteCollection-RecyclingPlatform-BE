using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Area
{
    public int AreaId { get; set; }

    public int DistrictId { get; set; }

    public string Name { get; set; } = null!;

    public virtual District District { get; set; } = null!;

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<WasteReport> WasteReports { get; set; } = new List<WasteReport>();
}
