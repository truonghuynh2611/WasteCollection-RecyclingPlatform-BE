using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

[Table("Admins")]
public partial class Admin
{
    [Key]
    [Column("AdminId")]
    public int Id { get; set; }

    [Column("UserId")]
    public int UserId { get; set; }

    [Column("Department")]
    public string? Department { get; set; }

    [Column("Level")]
    public int? Level { get; set; }

    [Column("IsSuperAdmin")]
    public bool IsSuperAdmin { get; set; } = false;

    [Column("Status")]
    public bool Status { get; set; } = true;

    [Column("CreatedBy")]
    public int? CreatedBy { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("LastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual User? CreatorUser { get; set; }
}
