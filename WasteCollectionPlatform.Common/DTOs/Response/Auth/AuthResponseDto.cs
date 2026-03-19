using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Common.DTOs.Response.Auth;

/// <summary>
/// Authentication response data transfer object
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    
    public string RefreshToken { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
    
    public bool Status { get; set; }
    
    public int? CitizenId { get; set; }
    
    public int TotalPoints { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}
