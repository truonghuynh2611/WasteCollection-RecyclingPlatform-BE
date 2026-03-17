using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

/// <summary>
/// Refresh token entity for sliding expiration authentication
/// </summary>
[Table("RefreshTokens")]
public class RefreshToken
{
    /// <summary>
    /// Refresh token ID
    /// </summary>
    [Key]
    [Column("RefreshTokenId")]
    public int RefreshtokenId { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [Required]
    [Column("UserId")]
    public int UserId { get; set; }

    /// <summary>
    /// Refresh token value (GUID)
    /// </summary>
    [Required]
    [Column("Token")]
    [MaxLength(500)]
    public string Token { get; set; } = null!;

    /// <summary>
    /// Token expiration date (30 days)
    /// </summary>
    [Required]
    [Column("ExpiresAt")]
    public DateTime Expiresat { get; set; }

    /// <summary>
    /// Token creation date
    /// </summary>
    [Required]
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Is token revoked (for logout/security)
    /// </summary>
    [Column("IsRevoked")]
    public bool? Isrevoked { get; set; }

    /// <summary>
    /// Revoked date
    /// </summary>
    [Column("RevokedAt")]
    public DateTime? Revokedat { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
