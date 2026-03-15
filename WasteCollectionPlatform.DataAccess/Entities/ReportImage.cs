using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class ReportImage
{
    public int Imageid { get; set; }

    public int Reportid { get; set; }

    public string Imageurl { get; set; } = null!;

    public virtual WasteReport Report { get; set; } = null!;
}
