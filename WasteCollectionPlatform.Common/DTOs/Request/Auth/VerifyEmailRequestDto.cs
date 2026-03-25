using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.Auth;

/// <summary>
/// DTO for email verification with 6-digit code
/// </summary>
public class VerifyEmailRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = null!;
}
