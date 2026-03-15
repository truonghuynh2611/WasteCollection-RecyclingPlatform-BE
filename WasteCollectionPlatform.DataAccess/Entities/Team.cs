using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Team
{
    [Column("teamid")]
    public int TeamId { get; set; }

    public int AreaId { get; set; }

    public string Name { get; set; } = null!;

    public int CurrentTaskCount { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual ICollection<Collector> Collectors { get; set; } = new List<Collector>();

    public virtual ICollection<ReportAssignment> ReportAssignments { get; set; } = new List<ReportAssignment>();
}
