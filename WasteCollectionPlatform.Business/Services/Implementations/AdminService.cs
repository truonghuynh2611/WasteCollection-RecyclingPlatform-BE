using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Admin;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace WasteCollectionPlatform.Business.Services.Implementations;

/// <summary>
/// Admin management service implementation
/// </summary>
public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAdminRepository adminRepo,
        IUserRepository userRepo,
        IUnitOfWork unitOfWork,
        ILogger<AdminService> logger)
    {
        _adminRepo = adminRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GetAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, int superAdminId)
    {
        // Check email exists
        if (await _userRepo.EmailExistsAsync(request.Email))
        {
            throw new Exception("Email already exists");
        }

        // Verify super admin exists and is super admin
        var superAdmin = await _adminRepo.GetByUserIdAsync(superAdminId);
        if (superAdmin == null || !superAdmin.IsSuperAdmin)
        {
            throw new UnauthorizedException("Only super admin can create new admin");
        }

        // Start transaction
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Create user
            var user = new User
            {
                Email = request.Email,
                FullName = request.FullName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Phone = request.Phone,
                Role = UserRole.Admin,
                Status = true,
                Emailverified = true,
                TokenVersion = 0
            };

            await _userRepo.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Create admin profile
            var admin = new Admin
            {
                UserId = user.UserId,
                Department = request.Department,
                Level = request.Level ?? 1,
                IsSuperAdmin = request.IsSuperAdmin,
                Status = true,
                CreatedBy = superAdminId,
                CreatedAt = DateTime.UtcNow
            };

            await _adminRepo.AddAsync(admin);
            await _unitOfWork.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            _logger.LogInformation($"Admin created: {admin.Id} by SuperAdmin: {superAdminId}");

            return MapToDto(admin, user);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating admin");
            throw;
        }
    }

    public async Task<GetAdminResponseDto> GetAdminByIdAsync(int adminId)
    {
        var admin = await _adminRepo.GetByIdWithDetailsAsync(adminId);
        if (admin == null)
        {
            throw new Exception("Admin not found");
        }

        return MapToDto(admin, admin.User);
    }

    public async Task<List<GetAdminResponseDto>> GetAllAdminsAsync()
    {
        var admins = await _adminRepo.GetAllWithDetailsAsync();
        return admins.Select(a => MapToDto(a, a.User)).ToList();
    }

    public async Task<GetAdminResponseDto> UpdateAdminAsync(int adminId, UpdateAdminRequestDto request)
    {
        var admin = await _adminRepo.GetByIdWithDetailsAsync(adminId);
        if (admin == null)
        {
            throw new Exception("Admin not found");
        }

        if (request.Department != null)
            admin.Department = request.Department;

        if (request.Level.HasValue)
            admin.Level = request.Level.Value;

        if (request.Status.HasValue)
            admin.Status = request.Status.Value;

        await _adminRepo.UpdateAsync(admin);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"Admin updated: {adminId}");

        return MapToDto(admin, admin.User);
    }

    public async Task DisableAdminAsync(int adminId)
    {
        var admin = await _adminRepo.GetByIdWithDetailsAsync(adminId);
        if (admin == null)
        {
            throw new Exception("Admin not found");
        }

        admin.Status = false;

        // Invalidate tokens by incrementing tokenVersion
        var user = admin.User;
        user.TokenVersion++;

        await _adminRepo.UpdateAsync(admin);
        await _userRepo.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation($"Admin disabled: {adminId}");
    }

    public async Task<bool> UserIsAdminAsync(int userId)
    {
        return await _adminRepo.UserIsAdminAsync(userId);
    }

    public async Task<GetAdminResponseDto?> GetAdminByUserIdAsync(int userId)
    {
        var admin = await _adminRepo.GetByUserIdAsync(userId);
        if (admin == null)
            return null;

        var user = await _userRepo.GetByIdAsync(admin.UserId);
        if (user == null)
            return null;

        return MapToDto(admin, user);
    }

    public async Task UpdateLastLoginAsync(int adminId)
    {
        var admin = await _adminRepo.GetByIdAsync(adminId);
        if (admin != null)
        {
            admin.LastLoginAt = DateTime.UtcNow;
            await _adminRepo.UpdateAsync(admin);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    private GetAdminResponseDto MapToDto(Admin admin, User user)
    {
        return new GetAdminResponseDto
        {
            Id = admin.Id,
            UserId = admin.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Department = admin.Department,
            Level = admin.Level,
            IsSuperAdmin = admin.IsSuperAdmin,
            Status = admin.Status,
            CreatedAt = admin.CreatedAt,
            LastLoginAt = admin.LastLoginAt
        };
    }
}
