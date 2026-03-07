using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Pointhistory
{
    public int Pointlogid { get; set; }

    public int Citizenid { get; set; }

    public int? Reportid { get; set; }

    public int? Voucherid { get; set; }

    public int Pointamount { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Citizen Citizen { get; set; } = null!;

    public virtual Wastereport? Report { get; set; }

    public virtual Voucher? Voucher { get; set; }
}
