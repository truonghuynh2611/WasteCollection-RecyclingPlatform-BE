using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Admin management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdminService adminService,
        ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    private int GetCurrentAdminId()
    {
        var adminIdClaim = User.FindFirst("adminId");
        if (!int.TryParse(adminIdClaim?.Value, out var adminId))
        {
            throw new UnauthorizedException("Admin ID not found in token");
        }
        return adminId;
    }

    private bool IsSuperAdmin()
    {
        var superAdminClaim = User.FindFirst("isSuperAdmin");
        return superAdminClaim?.Value == "true";
    }

    /// <summary>
    /// Create new admin (SuperAdmin only)
    /// </summary>
    /// <param name="request">Admin creation request</param>
    /// <returns>Created admin details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GetAdminResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequestDto request)
    {
        try
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            var superAdminId = GetCurrentAdminId();
            var admin = await _adminService.CreateAdminAsync(request, superAdminId);

            return CreatedAtAction(nameof(GetAdminById), new { id = admin.Id },
                ApiResponse<GetAdminResponseDto>.SuccessResponse(admin, "Admin created successfully"));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex, "Unauthorized admin creation attempt");
            return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all admins
    /// </summary>
    /// <returns>List of all admins</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<GetAdminResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAdmins()
    {
        try
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(ApiResponse<List<GetAdminResponseDto>>.SuccessResponse(admins, "Admins retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admins");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get admin by ID
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Admin details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetAdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdminById(int id)
    {
        try
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            return Ok(ApiResponse<GetAdminResponseDto>.SuccessResponse(admin, "Admin retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving admin {id}");
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update admin information
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <param name="request">Update request</param>
    /// <returns>Updated admin details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<GetAdminResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAdmin(int id, [FromBody] UpdateAdminRequestDto request)
    {
        try
        {
            var admin = await _adminService.UpdateAdminAsync(id, request);
            return Ok(ApiResponse<GetAdminResponseDto>.SuccessResponse(admin, "Admin updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating admin {id}");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Disable admin account
    /// </summary>
    /// <param name="id">Admin ID</param>
    /// <returns>Success message</returns>
    [HttpPatch("{id}/disable")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableAdmin(int id)
    {
        try
        {
            if (!IsSuperAdmin())
            {
                return Forbid();
            }

            await _adminService.DisableAdminAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Admin disabled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error disabling admin {id}");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}

