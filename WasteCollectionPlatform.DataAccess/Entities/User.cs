using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.DataAccess.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    [Column("role")]
    public UserRole Role { get; set; }

    public bool? Status { get; set; }

    // Email Verification fields
    [Column("emailverified")]
    public bool Emailverified { get; set; } = false;
    
    [Column("verificationtoken")]
    public string? Verificationtoken { get; set; }
    
    [Column("verificationtokenexpiry")]
    public DateTime? Verificationtokenexpiry { get; set; }
    
    // Password Reset fields
    [Column("resetpasswordtoken")]
    public string? Resetpasswordtoken { get; set; }
    
    [Column("resettokenexpiry")]
    public DateTime? Resettokenexpiry { get; set; }

    public int TokenVersion { get; set; } = 0;

    public virtual Citizen? Citizen { get; set; }

    public virtual Collector? Collector { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
