using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WasteCollectionPlatform.DataAccess.Entities;

[Table("PendingRegistrations")]
public class PendingRegistration
{
    [Key]
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string VerificationCode { get; set; } = null!;

    [Required]
    public DateTime Expiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
