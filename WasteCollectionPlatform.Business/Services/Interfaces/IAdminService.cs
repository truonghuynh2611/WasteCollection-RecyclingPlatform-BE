using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Admin;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

/// <summary>
/// Admin management service interface
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Create new admin (SuperAdmin only)
    /// </summary>
    Task<GetAdminResponseDto> CreateAdminAsync(CreateAdminRequestDto request, int superAdminId);
    
    /// <summary>
    /// Get admin by ID
    /// </summary>
    Task<GetAdminResponseDto> GetAdminByIdAsync(int adminId);
    
    /// <summary>
    /// Get all admins
    /// </summary>
    Task<List<GetAdminResponseDto>> GetAllAdminsAsync();
    
    /// <summary>
    /// Update admin information
    /// </summary>
    Task<GetAdminResponseDto> UpdateAdminAsync(int adminId, UpdateAdminRequestDto request);
    
    /// <summary>
    /// Disable admin account
    /// </summary>
    Task DisableAdminAsync(int adminId);
    
    /// <summary>
    /// Check if user is admin
    /// </summary>
    Task<bool> UserIsAdminAsync(int userId);
    
    /// <summary>
    /// Get admin by user ID
    /// </summary>
    Task<GetAdminResponseDto?> GetAdminByUserIdAsync(int userId);
    
    /// <summary>
    /// Update admin last login
    /// </summary>
    Task UpdateLastLoginAsync(int adminId);
}
