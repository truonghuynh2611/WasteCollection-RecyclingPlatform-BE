using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Collector
{
    public int Collectorid { get; set; }

    public int Userid { get; set; }

    [Column("teamid")]
    public int TeamId { get; set; }

    public bool? Status { get; set; }

    public int? Currenttaskcount { get; set; }

    public virtual Team Team { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
