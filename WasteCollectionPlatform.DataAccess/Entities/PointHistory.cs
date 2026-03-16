using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class PointHistory
{
    public int PointlogId { get; set; }

    public int CitizenId { get; set; }

    public int? ReportId { get; set; }

    public int? VoucherId { get; set; }

    public int PointAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Citizen Citizen { get; set; } = null!;

    public virtual WasteReport? Report { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
