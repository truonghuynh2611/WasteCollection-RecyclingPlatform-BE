using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Citizen
{
    public int Citizenid { get; set; }

    public int Userid { get; set; }

    public int? Totalpoints { get; set; }

    public virtual ICollection<Pointhistory> Pointhistories { get; set; } = new List<Pointhistory>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Wastereport> Wastereports { get; set; } = new List<Wastereport>();
}
