using WasteCollectionPlatform.Common.DTOs.Request.Auth;
using WasteCollectionPlatform.Common.DTOs.Response.Auth;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and generate JWT token
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    
    /// <summary>
    /// Register new user
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    
    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<AuthResponseDto> RefreshTokenAsync(string token);
    
    /// <summary>
    /// Verify user email with verification token
    /// </summary>
    Task<bool> VerifyEmailAsync(string token);
    
    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    
    /// <summary>
    /// Reset user password with token
    /// </summary>
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    
    /// <summary>
    /// Resend verification email to user
    /// </summary>
    Task<bool> ResendVerificationEmailAsync(string email);
}
