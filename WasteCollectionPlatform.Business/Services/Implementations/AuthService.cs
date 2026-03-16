using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Constants;
using WasteCollectionPlatform.Common.DTOs.Request.Auth;
using WasteCollectionPlatform.Common.DTOs.Response.Auth;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.Common.Helpers;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtHelper _jwtHelper;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;
    
    public AuthService(
        IUnitOfWork unitOfWork,
        JwtHelper jwtHelper,
        IConfiguration configuration,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtHelper = jwtHelper;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Find user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }
        
        // Verify password
        if (!PasswordHasher.VerifyPassword(request.Password, user.Password))
        {
            throw new UnauthorizedException(ErrorMessages.InvalidCredentials);
        }
        
        // Check user status (true = active, false/null = inactive)
        if (user.Status != true)
        {
            throw new UnauthorizedException(ErrorMessages.AccountInactive);
        }
        
        // Generate JWT access token
        var token = _jwtHelper.GenerateToken(
            user.UserId, 
            user.Email, 
            user.FullName, 
            user.Role.ToString(), 
            user.Status?.ToString() ?? "false"
        );
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
        
        // Generate refresh token (30 days)
        var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
        var refreshTokenExpiration = RefreshTokenHelper.CalculateExpirationDate(30);
        
        // Save refresh token to database
        var refreshTokenEntity = new DataAccess.Entities.RefreshToken
        {
            UserId = user.UserId,
            Token = refreshToken,
            Expiresat = refreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            Isrevoked = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Status = user.Status ?? false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }
    
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if email already exists
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            throw new BusinessRuleException(ErrorMessages.EmailAlreadyExists);
        }
        
        // Validate team if Collector role
        if (request.Role == UserRole.Collector && request.TeamId.HasValue)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
            if (team == null)
            {
                throw new BusinessRuleException("Invalid team specified.");
            }
        }
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            // Create user entity with PostgreSQL schema
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = PasswordHasher.HashPassword(request.Password),
                Phone = request.Phone,
                Role = request.Role, // PostgreSQL user_role ENUM
                Status = true, // Active by default
                Emailverified = false, // Email not verified yet
                Verificationtoken = Guid.NewGuid().ToString(),
                Verificationtokenexpiry = DateTime.UtcNow.AddHours(24) // 24 hours to verify
            };
            
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(); // Save to get generated Userid
            
            // Create role-specific entity
            switch (request.Role)
            {
                case UserRole.Citizen:
                    var citizen = new Citizen
                    {
                        UserId = user.UserId,
                        TotalPoints = 0
                    };
                    await _unitOfWork.Citizens.AddAsync(citizen);
                    break;
                
                case UserRole.Collector:
                    if (!request.TeamId.HasValue)
                    {
                        throw new BusinessRuleException("Team ID is required for Collector registration.");
                    }
                    
                    var collector = new Collector
                    {
                        UserId = user.UserId,
                        TeamId = request.TeamId.Value,
                        Status = true,
                        Role = CollectorRole.Member
                    };
                    await _unitOfWork.Collectors.AddAsync(collector);
                    break;
                
                case UserRole.Enterprise:
                    var enterprise = new Enterprise
                    {
                        UserId = user.UserId,
                        DistrictId = request.DistrictId,
                        Wastetypes = request.WasteTypes,
                        Dailycapacity = request.DailyCapacity ?? 100, // Default capacity
                        Currentload = 0,
                        Status = true
                    };
                    await _unitOfWork.Enterprises.AddAsync(enterprise);
                    break;
            }
            
            // Generate refresh token (30 days) - BEFORE committing transaction
            var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
            var refreshTokenExpiration = RefreshTokenHelper.CalculateExpirationDate(30);
            
            // Save refresh token to database - INSIDE transaction
            var refreshTokenEntity = new DataAccess.Entities.RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                Expiresat = refreshTokenExpiration,
                CreatedAt = DateTime.UtcNow,
                Isrevoked = false
            };
            
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            
            // Save all changes (role-specific entity + refresh token) and commit transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            // Generate JWT access token (after successful transaction)
            var token = _jwtHelper.GenerateToken(
                user.UserId, 
                user.Email, 
                user.FullName, 
                user.Role.ToString(), 
                user.Status?.ToString() ?? "false"
            );
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
            
            // Send verification email (async, don't wait)
            _ = Task.Run(async () =>
            {
                try
                {
                    var verificationLink = $"http://localhost:5173/verify-email?token={user.Verificationtoken}";
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2>Verify Your Account</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Thank you for registering with Waste Collection Platform.</p>
                            <p>Please click the link below to verify your email address:</p>
                            <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Verify Email</a></p>
                            <p>Or copy this link to your browser:</p>
                            <p>{verificationLink}</p>
                            <p>This link will expire in 24 hours.</p>
                            <p>Best regards,<br/>Waste Collection Platform Team</p>
                        </body>
                        </html>";
                    
                    await _emailService.SendEmailAsync(user.Email, "Verify your account", emailBody);
                    _logger.LogInformation("Verification email sent to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
                }
            });
            
            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Status = user.Status ?? false,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
    
    public async Task<AuthResponseDto> RefreshTokenAsync(string token)
    {
        // Validate refresh token from database
        var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
        
        if (refreshTokenEntity == null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }
        
        // Check if token is expired
        if (refreshTokenEntity.Expiresat <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired. Please login again.");
        }
        
        // Check if token is revoked
        if (refreshTokenEntity.Isrevoked == true)
        {
            throw new UnauthorizedException("Refresh token has been revoked.");
        }
        
        // Get user
        var user = await _unitOfWork.Users.GetByIdAsync(refreshTokenEntity.UserId);
        if (user == null)
        {
            throw new NotFoundException(ErrorMessages.UserNotFound);
        }
        
        // Check user status (true = active)
        if (user.Status != true)
        {
            throw new UnauthorizedException("Cannot refresh token for inactive user.");
        }
        
        // Revoke old refresh token
        refreshTokenEntity.Isrevoked = true;
        refreshTokenEntity.Revokedat = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        
        // Generate new access token
        var newToken = _jwtHelper.GenerateToken(
            user.UserId, 
            user.Email, 
            user.FullName, 
            user.Role.ToString(), 
            user.Status?.ToString() ?? "false"
        );
        var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
        
        // Generate NEW refresh token (sliding expiration - extends 30 days)
        var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
        var newRefreshTokenExpiration = RefreshTokenHelper.CalculateExpirationDate(30);
        
        // Save new refresh token to database
        var newRefreshTokenEntity = new DataAccess.Entities.RefreshToken
        {
            UserId = user.UserId,
            Token = newRefreshToken,
            Expiresat = newRefreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            Isrevoked = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Status = user.Status ?? false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }
    
    public async Task<bool> VerifyEmailAsync(string token)
    {
        // Find user by verification token
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Verificationtoken == token);
        
        if (user == null)
        {
            throw new NotFoundException("Invalid verification token.");
        }
        
        // Check if token is expired
        if (user.Verificationtokenexpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Verification token has expired. Please request a new one.");
        }
        
        // Check if already verified
        if (user.Emailverified)
        {
            throw new BusinessRuleException("Email already verified.");
        }
        
        // Update user
        user.Emailverified = true;
        user.Verificationtoken = null;
        user.Verificationtokenexpiry = null;
        
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        // Find user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        
        if (user == null)
        {
            // Don't reveal that email doesn't exist for security
            // But still return success
            return true;
        }
        
        // Generate reset token
        var resetToken = Guid.NewGuid().ToString();
        var resetExpiry = DateTime.UtcNow.AddMinutes(30); // 30 minutes to reset
        
        // Update user
        user.Resetpasswordtoken = resetToken;
        user.Resettokenexpiry = resetExpiry;
        
        await _unitOfWork.SaveChangesAsync();
        
        // Send password reset email (async, don't wait)
        _ = Task.Run(async () =>
        {
            try
            {
                var resetLink = $"http://localhost:5173/reset-password?token={resetToken}";
                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Reset Your Password</h2>
                        <p>Hello {user.FullName},</p>
                        <p>We received a request to reset your password.</p>
                        <p>Click the link below to reset your password:</p>
                        <p><a href='{resetLink}' style='background-color: #f44336; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Reset Password</a></p>
                        <p>Or copy this link to your browser:</p>
                        <p>{resetLink}</p>
                        <p>This link will expire in 30 minutes.</p>
                        <p>If you didn't request this, please ignore this email.</p>
                        <p>Best regards,<br/>Waste Collection Platform Team</p>
                    </body>
                    </html>";
                
                await _emailService.SendEmailAsync(user.Email, "Reset your password", emailBody);
                _logger.LogInformation("Password reset email sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
            }
        });
        
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        // Find user by reset token
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Resetpasswordtoken == request.Token);
        
        if (user == null)
        {
            throw new NotFoundException("Invalid reset token.");
        }
        
        // Check if token is expired
        if (user.Resettokenexpiry == null || user.Resettokenexpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Reset token has expired. Please request a new password reset.");
        }
        
        // Hash new password
        user.Password = PasswordHasher.HashPassword(request.NewPassword);
        
        // Remove reset token
        user.Resetpasswordtoken = null;
        user.Resettokenexpiry = null;
        
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }
}
