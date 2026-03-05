using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.DTOs.Request.Auth;
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
            
            return Ok(ApiResponse<object>.SuccessResponse(result, SuccessMessages.LoginSuccess));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Login failed for email: {Email}", request.Email);
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ErrorMessages.InternalServerError));
        }
    }
    
    /// <summary>
    /// Register new user endpoint
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <returns>Authentication response</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
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
            
            // PostgreSQL schema: Status is bool (true = active, false/null = inactive)
            var message = result.Status 
                ? SuccessMessages.RegisterSuccess
                : SuccessMessages.RegisterSuccessPending;
            
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, message));
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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
            
            return Ok(ApiResponse<object>.SuccessResponse(result, SuccessMessages.TokenRefreshed));
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
    /// Verify email address
    /// </summary>
    /// <param name="token">Verification token from email</param>
    /// <returns>Success response</returns>
    [HttpGet("verify-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Token is required."));
            }
            
            await _authService.VerifyEmailAsync(token);
            
            return Ok(ApiResponse<object>.SuccessResponse(null, "Email verified successfully. You can now login."));
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Email verification failed - token not found");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
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
            
            return Ok(ApiResponse<object>.SuccessResponse(null, "If the email exists, a password reset link has been sent."));
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
            
            return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully. You can now login with your new password."));
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
