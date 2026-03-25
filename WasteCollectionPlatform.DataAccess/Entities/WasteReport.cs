using System;
using System.Collections.Generic;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class WasteReport
{
    public int ReportId { get; set; }

    public int CitizenId { get; set; }

    public int AreaId { get; set; }

    public string? Description { get; set; }

    public string? WasteType { get; set; }

    public decimal? CitizenLatitude { get; set; }

    public decimal? CitizenLongitude { get; set; }

    public decimal? CollectorLatitude { get; set; }

    public decimal? CollectorLongitude { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpireTime { get; set; }

    public ReportStatus Status { get; set; }

    public int? TeamId { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual Citizen Citizen { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();

    public virtual ICollection<ReportAssignment> ReportAssignments { get; set; } = new List<ReportAssignment>();

    public virtual ICollection<ReportImage> ReportImages { get; set; } = new List<ReportImage>();

    public virtual ICollection<WasteReportItem> WasteReportItems { get; set; } = new List<WasteReportItem>();
}
