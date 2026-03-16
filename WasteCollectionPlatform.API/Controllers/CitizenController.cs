using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Citizen management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CitizenController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CitizenController> _logger;

    public CitizenController(
        IUnitOfWork unitOfWork,
        ILogger<CitizenController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all citizens with user info
    /// </summary>
    /// <returns>List of all citizens</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCitizens()
    {
        try
        {
            var citizens = await _unitOfWork.Citizens.GetAllAsync();
            var users = await _unitOfWork.Users.GetAllAsync();
            
            var citizenDtos = citizens.Select(c =>
            {
                var user = users.FirstOrDefault(u => u.UserId == c.UserId);
                return new
                {
                    citizenId = c.CitizenId,
                    userId = c.UserId,
                    email = user?.Email ?? "N/A",
                    fullName = user?.FullName ?? "N/A",
                    phone = user?.Phone ?? "N/A",
                    totalPoints = c.TotalPoints ?? 0,
                    status = user?.Status ?? false
                };
            }).OrderBy(c => c.userId).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Retrieved {citizenDtos.Count} citizens successfully.",
                Data = citizenDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving citizens");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving citizens.",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get citizen by user ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Citizen details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCitizenById(int id)
    {
        try
        {
            var citizen = await _unitOfWork.Citizens.GetByIdAsync(id);
            
            if (citizen == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Citizen with user ID {id} not found."
                });
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);

            var citizenDto = new
            {
                citizenId = citizen.CitizenId,
                userId = citizen.UserId,
                email = user?.Email ?? "N/A",
                fullName = user?.FullName ?? "N/A",
                phone = user?.Phone ?? "N/A",
                totalPoints = citizen.TotalPoints ?? 0,
                status = user?.Status ?? false
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Citizen retrieved successfully.",
                Data = citizenDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving citizen {UserId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the citizen.",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get statistics - count of citizens
    /// </summary>
    /// <returns>Total citizen count</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCitizenStats()
    {
        try
        {
            var citizens = await _unitOfWork.Citizens.GetAllAsync();
            
            var stats = new
            {
                totalCitizens = citizens.Count(),
                activeCitizens = citizens.Count(c => c.User != null && c.User.Status == true),
                totalPoints = citizens.Sum(c => c.TotalPoints ?? 0)
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Citizen statistics retrieved successfully.",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving citizen statistics");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving statistics.",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}