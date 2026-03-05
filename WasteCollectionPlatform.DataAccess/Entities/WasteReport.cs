using System;
using System.Collections.Generic;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class Wastereport
{
    public int Reportid { get; set; }

    public int Citizenid { get; set; }

    public int Areaid { get; set; }

    public string? Description { get; set; }

    public string? Wastetype { get; set; }

    public decimal? Citizenlatitude { get; set; }

    public decimal? Citizenlongitude { get; set; }

    public decimal? Collectorlatitude { get; set; }

    public decimal? Collectorlongitude { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Expiretime { get; set; }

    public virtual Area Area { get; set; } = null!;

    public virtual Citizen Citizen { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Pointhistory> Pointhistories { get; set; } = new List<Pointhistory>();

    public virtual ICollection<Reportassignment> Reportassignments { get; set; } = new List<Reportassignment>();

    public virtual ICollection<Reportimage> Reportimages { get; set; } = new List<Reportimage>();
}
