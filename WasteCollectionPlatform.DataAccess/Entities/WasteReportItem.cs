using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class WasteReportItem
{
    public int ItemId { get; set; }

    public int ReportId { get; set; }

    public string WasteType { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public virtual WasteReport Report { get; set; } = null!;
}
