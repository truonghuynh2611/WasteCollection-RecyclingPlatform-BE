using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class District
{
    public int DistrictId { get; set; }

    public string DistrictName { get; set; } = null!;

    public virtual ICollection<Area> Areas { get; set; } = new List<Area>();
}
