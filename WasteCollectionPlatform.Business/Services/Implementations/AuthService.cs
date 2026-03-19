using System;
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
        
        // Check user status (true/null = active, false = inactive)
        if (user.Status == false)
        {
            throw new UnauthorizedException(ErrorMessages.AccountInactive);
        }

        
        // Generate JWT access token with admin details if applicable
        var token = _jwtHelper.GenerateToken(
            user.UserId, 
            user.Email, 
            user.FullName, 
            user.Role.ToString(), 
            user.Status?.ToString() ?? "false",
            null,
            false,
            user.TokenVersion
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
            ExpiresAt = refreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);


        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"User {user.Email} logged in successfully");
        
        // Get CitizenId and TotalPoints if user is a citizen
        int? citizenId = null;
        int totalPoints = 0;
        if (user.Role == UserRole.Citizen)
        {
            var citizen = await _unitOfWork.Citizens.GetByUserIdAsync(user.UserId);
            citizenId = citizen?.CitizenId;
            totalPoints = citizen?.TotalPoints ?? 0;
        }

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Status = user.Status ?? false,
            CitizenId = citizenId,
            TotalPoints = totalPoints,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }
    
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Check if email already exists in Users or PendingRegistrations
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email))
        {
            throw new BusinessRuleException(ErrorMessages.EmailAlreadyExists);
        }

        var existingPending = await _unitOfWork.PendingRegistrations.GetByEmailAsync(request.Email);
        if (existingPending != null)
        {
            // Update existing pending record instead of creating new one
            existingPending.FullName = request.FullName;
            existingPending.PasswordHash = PasswordHasher.HashPassword(request.Password);
            existingPending.Phone = request.Phone;
            existingPending.VerificationCode = new Random().Next(100000, 999999).ToString();
            existingPending.Expiry = DateTime.UtcNow.AddHours(2);
            await _unitOfWork.PendingRegistrations.UpdateAsync(existingPending);
        }
        else
        {
            // Create new pending record
            var pendingRecord = new PendingRegistration
            {
                Email = request.Email,
                FullName = request.FullName,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Phone = request.Phone,
                VerificationCode = new Random().Next(100000, 999999).ToString(),
                Expiry = DateTime.UtcNow.AddHours(2)
            };
            await _unitOfWork.PendingRegistrations.AddAsync(pendingRecord);
        }

        await _unitOfWork.SaveChangesAsync();
        
        // 2. Get the latest pending info to send email
        var latestPending = await _unitOfWork.PendingRegistrations.GetByEmailAsync(request.Email);
        
        // 3. Send verification email (async)
        _ = Task.Run(async () =>
        {
            try
            {
                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                            <h2 style='color: #2e7d32; text-align: center;'>Xác thực tài khoản Green Vietnam</h2>
                            <p>Xin chào <strong>{latestPending.FullName}</strong>,</p>
                            <p>Cảm ơn bạn đã đăng ký tham gia bảo vệ môi trường cùng Green Vietnam.</p>
                            <p>Mã xác thực của bạn là:</p>
                            <div style='background-color: #f1f8e9; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #2e7d32; border-radius: 5px; margin: 20px 0;'>
                                {latestPending.VerificationCode}
                            </div>
                            <p>Mã này sẽ hết hạn trong vòng 2 giờ.</p>
                            <p>Nếu bạn không thực hiện đăng ký này, vui lòng bỏ qua email này.</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #888; text-align: center;'>Đội ngũ Green Vietnam</p>
                        </div>
                    </body>
                    </html>";
                
                await _emailService.SendEmailAsync(latestPending.Email, "Mã xác thực tài khoản Green Vietnam", emailBody);
                _logger.LogInformation("Verification email sent to {Email} with code {Code}", latestPending.Email, latestPending.VerificationCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", latestPending.Email);
            }
        });

        return new AuthResponseDto
        {
            Email = latestPending.Email,
            FullName = latestPending.FullName,
            Role = UserRole.Citizen.ToString(),
            Status = false // Not verified yet
        };
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
        if (refreshTokenEntity.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh token has expired. Please login again.");
        }
        
        // Check if token is revoked
        if (refreshTokenEntity.IsRevoked == true)
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
        refreshTokenEntity.IsRevoked = true;
        refreshTokenEntity.RevokedAt = DateTime.UtcNow;
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
            ExpiresAt = newRefreshTokenExpiration,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        
        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.SaveChangesAsync();
        
        // Get CitizenId if user is a citizen
        int? citizenId = null;
        if (user.Role == UserRole.Citizen)
        {
            var citizen = await _unitOfWork.Citizens.GetByUserIdAsync(user.UserId);
            citizenId = citizen?.CitizenId;
        }

        return new AuthResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Status = user.Status ?? false,
            CitizenId = citizenId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };
    }
    
    public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailRequestDto request)
    {
        // 1. Find pending registration
        var pending = await _unitOfWork.PendingRegistrations.GetByCodeAsync(request.Email, request.Code);
        if (pending == null)
        {
            throw new BusinessRuleException("Mã xác thực không hợp lệ hoặc đã hết hạn.");
        }

        if (pending.Expiry < DateTime.UtcNow)
        {
            await _unitOfWork.PendingRegistrations.DeleteAsync(pending);
            await _unitOfWork.SaveChangesAsync();
            throw new BusinessRuleException("Mã xác thực đã hết hạn. Vui lòng nhấn 'Gửi lại mã'.");
        }

        // 2. Verified! Now create the real User and Citizen
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var user = new User
            {
                FullName = pending.FullName,
                Email = pending.Email,
                Password = pending.PasswordHash,
                Phone = pending.Phone,
                Role = UserRole.Citizen,
                Status = true,
                EmailVerified = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(); // Get Userid

            var citizen = new Citizen
            {
                UserId = user.UserId,
                TotalPoints = 0
            };
            await _unitOfWork.Citizens.AddAsync(citizen);

            // 3. Generate tokens for auto-login
            var token = _jwtHelper.GenerateToken(
                user.UserId, 
                user.Email, 
                user.FullName, 
                user.Role.ToString(), 
                "true"
            );
            var expirationMinutes = int.Parse(_configuration["JwtSettings:ExpirationMinutes"] ?? "60");
            
            var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
            var refreshTokenExpiration = RefreshTokenHelper.CalculateExpirationDate(30);
            
            var refreshTokenEntity = new DataAccess.Entities.RefreshToken
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpiresAt = refreshTokenExpiration,
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);

            // 4. Cleanup pending data
            await _unitOfWork.PendingRegistrations.DeleteAsync(pending);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                Status = true,
                CitizenId = citizen.CitizenId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Lỗi khi chuyển đổi dữ liệu từ Pending sang User chính thức.");
            throw;
        }
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
        user.ResetPasswordToken = resetToken;
        user.ResetTokenExpiry = resetExpiry;
        
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
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == request.Token);
        
        if (user == null)
        {
            throw new NotFoundException("Invalid reset token.");
        }
        
        // Check if token is expired
        if (user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            throw new BusinessRuleException("Reset token has expired. Please request a new password reset.");
        }
        
        // Hash new password
        user.Password = PasswordHasher.HashPassword(request.NewPassword);
        
        // Remove reset token
        user.ResetPasswordToken = null;
        user.ResetTokenExpiry = null;
        
        await _unitOfWork.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ResendVerificationCodeAsync(string email)
    {
        var pending = await _unitOfWork.PendingRegistrations.GetByEmailAsync(email);
        if (pending == null)
        {
            throw new NotFoundException("Không tìm thấy thông tin đăng ký cho email này.");
        }

        // Generate new code
        var newCode = new Random().Next(100000, 999999).ToString();
        pending.VerificationCode = newCode;
        pending.Expiry = DateTime.UtcNow.AddHours(2);

        await _unitOfWork.PendingRegistrations.UpdateAsync(pending);
        await _unitOfWork.SaveChangesAsync();

        // Resend email
        _ = Task.Run(async () =>
        {
            try
            {
                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                            <h2 style='color: #2e7d32; text-align: center;'>Gửi lại mã xác thực Green Vietnam</h2>
                            <p>Xin chào <strong>{pending.FullName}</strong>,</p>
                            <p>Đây là mã xác thực mới của bạn:</p>
                            <div style='background-color: #f1f8e9; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #2e7d32; border-radius: 5px; margin: 20px 0;'>
                                {newCode}
                            </div>
                            <p>Mã này sẽ hết hạn trong vòng 2 giờ.</p>
                            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
                            <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #888; text-align: center;'>Đội ngũ Green Vietnam</p>
                        </div>
                    </body>
                    </html>";
                
                await _emailService.SendEmailAsync(pending.Email, "Gửi lại mã xác thực tài khoản Green Vietnam", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể gửi lại email xác thực cho {Email}", pending.Email);
            }
        });

        return true;
    }
}
