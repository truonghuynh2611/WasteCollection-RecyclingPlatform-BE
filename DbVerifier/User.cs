using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class User
{
    [Column("UserId")]
    public int UserId { get; set; }

    [Column("FullName")]
    public string FullName { get; set; } = null!;

    [Column("Email")]
    public string Email { get; set; } = null!;

    [Column("Password")]
    public string Password { get; set; } = null!;

    [Column("Phone")]
    public string? Phone { get; set; }

    [Column("Role")]
    public UserRole Role { get; set; }

    [Column("Status")]
    public bool? Status { get; set; }

    // Email Verification fields
    [Column("EmailVerified")]
    public bool EmailVerified { get; set; } = false;
    
    [Column("VerificationToken")]
    public string? VerificationToken { get; set; }
    
    [Column("VerificationTokenExpiry")]
    public DateTime? VerificationTokenExpiry { get; set; }
    
    // Password Reset fields
    [Column("ResetPasswordToken")]
    public string? ResetPasswordToken { get; set; }
    
    [Column("ResetTokenExpiry")]
    public DateTime? ResetTokenExpiry { get; set; }

    public int TokenVersion { get; set; } = 0;

    public virtual Citizen? Citizen { get; set; }

    public virtual Collector? Collector { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
