using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.DTOs.Request.Auth;
using WasteCollectionPlatform.Common.DTOs.Response.Auth;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    /// <summary>
    /// Login endpoint
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication response with JWT token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }
            
            var result = await _authService.LoginAsync(request);
            
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, SuccessMessages.LoginSuccess));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            var detailedError = $"{ErrorMessages.InternalServerError} | {ex.Message} | {ex.InnerException?.Message}";
            return StatusCode(500, ApiResponse<object>.ErrorResponse(detailedError));
        }
    }
    
    /// <summary>
    /// Register new user endpoint
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <returns>Authentication response</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }
            
            var result = await _authService.RegisterAsync(request);
            
            // For 6-digit code flow, status is false until verified
            var message = "Đăng ký thành công. Vui lòng kiểm tra email để lấy mã xác thực 6 số.";
            
            return StatusCode(201, ApiResponse<AuthResponseDto>.SuccessResponse(result, message));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
    
    /// <summary>
    /// Refresh JWT token endpoint
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New authentication response with refreshed token</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }
            
            var result = await _authService.RefreshTokenAsync(request.Token);
            
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, SuccessMessages.TokenRefreshed));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found during token refresh");
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
    
    /// <summary>
    /// Verify email address with 6-digit code
    /// </summary>
    /// <param name="request">Email and 6-digit code</param>
    /// <returns>Success response with auth data</returns>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(request);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Email verified successfully. You are now logged in."));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Email verification failed - business rule: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }

    /// <summary>
    /// Resend verification code
    /// </summary>
    /// <param name="request">Email address</param>
    /// <returns>Success message</returns>
    [HttpPost("resend-code")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResendCode([FromBody] ResendCodeRequestDto request)
    {
        try
        {
            await _authService.ResendVerificationCodeAsync(request.Email);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Mã xác thực đã được gửi lại."));
        }
        catch (NotFoundException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend code");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
    
    /// <summary>
    /// Request password reset
    /// </summary>
    /// <param name="request">Email address</param>
    /// <returns>Success response</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }
            
            await _authService.ForgotPasswordAsync(request);
            
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "If the email exists, a password reset link has been sent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password request");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
    
    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="request">Reset token and new password</param>
    /// <returns>Success response</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }
            
            await _authService.ResetPasswordAsync(request);
            
            return Ok(ApiResponse<object>.SuccessResponse(new object(), "Password reset successfully. You can now login with your new password."));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Password reset failed - token not found");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Password reset failed - business rule: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
}
