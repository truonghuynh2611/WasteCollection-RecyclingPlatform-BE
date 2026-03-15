using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class ReportAssignment
{
    public int Assignmentid { get; set; }

    public int Reportid { get; set; }

    [Column("teamid")]
    public int TeamId { get; set; }

    public virtual WasteReport Report { get; set; } = null!;

    public virtual Team Team { get; set; } = null!;
}
