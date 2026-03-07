using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class District
{
    public int Districtid { get; set; }

    public string Districtname { get; set; } = null!;

    public virtual ICollection<Area> Areas { get; set; } = new List<Area>();
}
