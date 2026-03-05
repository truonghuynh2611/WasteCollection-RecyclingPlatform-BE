using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

/// <summary>
/// Enterprise entity for waste collection companies
/// </summary>
[Table("enterprise")]
public class Enterprise
{
    /// <summary>
    /// Enterprise ID (Primary Key)
    /// </summary>
    [Key]
    [Column("enterpriseid")]
    public int Enterpriseid { get; set; }

    /// <summary>
    /// User ID (Foreign Key to User table)
    /// </summary>
    [Required]
    [Column("userid")]
    public int Userid { get; set; }

    /// <summary>
    /// District ID where enterprise operates
    /// </summary>
    [Column("districtid")]
    public int? Districtid { get; set; }

    /// <summary>
    /// Types of waste the enterprise accepts (comma-separated)
    /// </summary>
    [Column("wastetypes")]
    [MaxLength(255)]
    public string? Wastetypes { get; set; }

    /// <summary>
    /// Daily capacity limit for waste collection
    /// </summary>
    [Column("dailycapacity")]
    public int? Dailycapacity { get; set; }

    /// <summary>
    /// Current load for today
    /// </summary>
    [Column("currentload")]
    public int? Currentload { get; set; }

    /// <summary>
    /// Enterprise status (active/inactive)
    /// </summary>
    [Column("status")]
    public bool? Status { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    [ForeignKey(nameof(Userid))]
    public virtual User User { get; set; } = null!;

    /// <summary>
    /// Navigation property to District
    /// </summary>
    [ForeignKey(nameof(Districtid))]
    public virtual District? District { get; set; }
}
