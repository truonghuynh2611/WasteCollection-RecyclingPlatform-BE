using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.DataAccess.Entities;
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

    /// <summary>
    /// Create a new district
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.DistrictName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Tên quận không được để trống."));

            var exists = await _unitOfWork.Districts.ExistsAsync(d => d.DistrictName == request.DistrictName.Trim());
            if (exists)
                return BadRequest(ApiResponse<object>.ErrorResponse("Quận đã tồn tại."));

            var district = new District { DistrictName = request.DistrictName.Trim() };
            await _unitOfWork.Districts.AddAsync(district);
            await _unitOfWork.SaveChangesAsync(); // Get the ID

            if (request.InitialAreaNames != null && request.InitialAreaNames.Any())
            {
                foreach (var areaName in request.InitialAreaNames.Where(n => !string.IsNullOrWhiteSpace(n)))
                {
                    var area = new Area
                    {
                        DistrictId = district.DistrictId,
                        Name = areaName.Trim()
                    };
                    await _unitOfWork.Areas.AddAsync(area);
                }
                await _unitOfWork.SaveChangesAsync();
            }

            return StatusCode(201, ApiResponse<object>.SuccessResponse(new
            {
                district.DistrictId,
                district.DistrictName,
                areasCreated = request.InitialAreaNames?.Count ?? 0
            }, $"Tạo quận thành công {(request.InitialAreaNames?.Any() == true ? $"và {request.InitialAreaNames.Count} khu vực" : "")}."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating district");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update a district
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDistrict(int id, [FromBody] UpdateDistrictRequestDto request)
    {
        try
        {
            var district = await _unitOfWork.Districts.GetByIdAsync(id);
            if (district == null)
                return NotFound(ApiResponse<object>.ErrorResponse($"Quận với ID {id} không tồn tại."));

            if (string.IsNullOrWhiteSpace(request.DistrictName))
                return BadRequest(ApiResponse<object>.ErrorResponse("Tên quận không được để trống."));

            var duplicate = await _unitOfWork.Districts.ExistsAsync(d => d.DistrictId != id && d.DistrictName == request.DistrictName.Trim());
            if (duplicate)
                return BadRequest(ApiResponse<object>.ErrorResponse("Tên quận đã tồn tại."));

            district.DistrictName = request.DistrictName.Trim();
            await _unitOfWork.Districts.UpdateAsync(district);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật quận thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating district {DistrictId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Delete a district
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDistrict(int id)
    {
        try
        {
            var district = await _unitOfWork.Districts.GetDistrictWithAreasAsync(id);
            if (district == null)
                return NotFound(ApiResponse<object>.ErrorResponse($"Quận với ID {id} không tồn tại."));

            if (district.Areas != null && district.Areas.Any())
                return BadRequest(ApiResponse<object>.ErrorResponse("Không thể xóa quận vì còn chứa các khu vực (Area)."));

            await _unitOfWork.Districts.DeleteAsync(district);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Xóa quận thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting district {DistrictId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}