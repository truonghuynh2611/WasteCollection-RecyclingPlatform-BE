using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Reportimage
{
    public int Imageid { get; set; }

    public int Reportid { get; set; }

    public string Imageurl { get; set; } = null!;

    public virtual Wastereport Report { get; set; } = null!;
}
