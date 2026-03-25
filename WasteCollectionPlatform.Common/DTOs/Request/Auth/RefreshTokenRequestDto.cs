using System.ComponentModel.DataAnnotations;

namespace WasteCollectionPlatform.Common.DTOs.Request.Auth;

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// JWT token to refresh
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = null!;
}
