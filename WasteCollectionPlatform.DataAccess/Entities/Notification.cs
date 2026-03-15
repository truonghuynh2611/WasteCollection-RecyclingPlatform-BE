using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Notification
{
    public int Notificationid { get; set; }

    public int? Reportid { get; set; }

    public int Userid { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public bool? Isread { get; set; }

    public virtual WasteReport? Report { get; set; }

    public virtual User User { get; set; } = null!;
}
