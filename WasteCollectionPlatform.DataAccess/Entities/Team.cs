using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Team
{
    [Column("teamid")]
    public int TeamId { get; set; }

    public int Areaid { get; set; }

    public string Name { get; set; } = null!;

    public virtual Area Area { get; set; } = null!;

    public virtual ICollection<Collector> Collectors { get; set; } = new List<Collector>();

    public virtual ICollection<Reportassignment> Reportassignments { get; set; } = new List<Reportassignment>();
}
