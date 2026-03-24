using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Collector
{
    public int CollectorId { get; set; }

    public int UserId { get; set; }

    [Column("teamid")]
    public int TeamId { get; set; }

    public bool? Status { get; set; }

    public CollectorRole Role { get; set; } = CollectorRole.Member;

    public virtual Team Team { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
