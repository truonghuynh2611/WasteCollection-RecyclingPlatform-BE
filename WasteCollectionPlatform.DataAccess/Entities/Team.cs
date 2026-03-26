using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Team
{
    [Column("TeamId")]
    public int TeamId { get; set; }

    public int? AreaId { get; set; }

    public string Name { get; set; } = null!;

    public int CurrentTaskCount { get; set; }

    public TeamType Type { get; set; }

    public virtual Area? Area { get; set; }

    public virtual ICollection<Collector> Collectors { get; set; } = new List<Collector>();

    public virtual ICollection<ReportAssignment> ReportAssignments { get; set; } = new List<ReportAssignment>();
}
