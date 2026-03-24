using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string VoucherName { get; set; } = null!;

    public string? Description { get; set; }

    public string? VoucherCode { get; set; }

    public string? Image { get; set; }

    public string? Category { get; set; }

    public int? ExpiryDays { get; set; }

    public bool? Status { get; set; }

    public int PointsRequired { get; set; }

    public int StockQuantity { get; set; }

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
