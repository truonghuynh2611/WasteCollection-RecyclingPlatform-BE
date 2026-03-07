using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

/// <summary>
/// Refresh token entity for sliding expiration authentication
/// </summary>
[Table("refreshtoken")]
public class RefreshToken
{
    /// <summary>
    /// Refresh token ID
    /// </summary>
    [Key]
    [Column("refreshtokenid")]
    public int Refreshtokenid { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    [Column("userid")]
    public int Userid { get; set; }

    /// <summary>
    /// Refresh token value (GUID)
    /// </summary>
    [Required]
    [Column("token")]
    [MaxLength(500)]
    public string Token { get; set; } = null!;

    /// <summary>
    /// Token expiration date (30 days)
    /// </summary>
    [Required]
    [Column("expiresat")]
    public DateTime Expiresat { get; set; }

    /// <summary>
    /// Token creation date
    /// </summary>
    [Required]
    [Column("createdat")]
    public DateTime Createdat { get; set; }

    /// <summary>
    /// Is token revoked (for logout/security)
    /// </summary>
    [Column("isrevoked")]
    public bool? Isrevoked { get; set; }

    /// <summary>
    /// Revoked date
    /// </summary>
    [Column("revokedat")]
    public DateTime? Revokedat { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    [ForeignKey(nameof(Userid))]
    public virtual User User { get; set; } = null!;
}
