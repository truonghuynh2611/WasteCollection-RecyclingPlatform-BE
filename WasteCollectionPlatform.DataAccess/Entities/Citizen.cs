using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Citizen
{
    public int Citizenid { get; set; }

    public int Userid { get; set; }

    public int? Totalpoints { get; set; }

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<WasteReport> WasteReports { get; set; } = new List<WasteReport>();
}
