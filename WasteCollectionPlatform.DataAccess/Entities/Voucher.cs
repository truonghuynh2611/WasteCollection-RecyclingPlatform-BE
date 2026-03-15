using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Voucher
{
    public int Voucherid { get; set; }

    public string Vouchername { get; set; } = null!;

    public bool? Status { get; set; }

    public int Pointsrequired { get; set; }

    public int Stockquantity { get; set; }

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
