using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class ReportImage
{
    public int ImageId { get; set; }

    public int ReportId { get; set; }

    public string Imageurl { get; set; } = null!;

    public string? ImageType { get; set; } // Citizen or Collector

    public virtual WasteReport Report { get; set; } = null!;
}
