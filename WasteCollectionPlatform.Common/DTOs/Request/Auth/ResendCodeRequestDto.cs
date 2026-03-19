using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.Auth;

/// <summary>
/// DTO for resending verification code
/// </summary>
public class ResendCodeRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
