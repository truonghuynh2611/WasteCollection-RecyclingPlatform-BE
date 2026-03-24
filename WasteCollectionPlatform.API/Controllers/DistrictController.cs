using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// District management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DistrictController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DistrictController> _logger;

    public DistrictController(
        IUnitOfWork unitOfWork,
        ILogger<DistrictController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all districts
    /// </summary>
    /// <returns>List of all districts</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDistricts()
    {
        try
        {
            var districts = await _unitOfWork.Districts.GetAllDistrictsWithAreasAsync();
            
            var districtDtos = districts.Select(d => new
            {
                districtId = d.DistrictId,
                districtName = d.DistrictName,
                areasCount = d.Areas?.Count ?? 0
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Retrieved {districtDtos.Count} districts successfully.",
                Data = districtDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving districts");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving districts.",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get district by ID
    /// </summary>
    /// <param name="id">District ID</param>
    /// <returns>District details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDistrictById(int id)
    {
        try
        {
            var district = await _unitOfWork.Districts.GetDistrictWithAreasAsync(id);
            
            if (district == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"District with ID {id} not found."
                });
            }

            var districtDto = new
            {
                districtId = district.DistrictId,
                districtName = district.DistrictName,
                areas = district.Areas?.Select(a => new
                {
                    areaId = a.AreaId,
                    areaName = a.Name
                }).ToList()
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "District retrieved successfully.",
                Data = districtDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving district {DistrictId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the district.",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}