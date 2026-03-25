using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.DTOs.Response.PointHistory;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Point history endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PointHistoryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PointHistoryController> _logger;

    public PointHistoryController(
        IUnitOfWork unitOfWork,
        ILogger<PointHistoryController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get point history for a citizen
    /// </summary>
    /// <param name="citizenId">Citizen ID or User ID</param>
    /// <returns>List of point history entries</returns>
    [HttpGet("citizen/{citizenId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PointHistoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCitizenId(int citizenId)
    {
        try
        {
            // Try as CitizenId first
            var citizen = await _unitOfWork.Citizens.GetByIdAsync(citizenId);
            
            // Fallback to UserId
            if (citizen == null)
            {
                citizen = await _unitOfWork.Citizens.GetByUserIdAsync(citizenId);
            }

            if (citizen == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Citizen with ID {citizenId} not found."
                });
            }

            var history = await _unitOfWork.PointHistories.GetByCitizenIdWithDetailsAsync(citizen.CitizenId);
            
            var historyDtos = history.Select(h => new PointHistoryResponseDto
            {
                PointHistoryId = h.PointlogId,
                CitizenId = h.CitizenId,
                PointAmount = h.PointAmount,
                Action = h.PointAmount > 0 ? "Tích điểm" : "Đổi quà",
                CreatedAt = h.CreatedAt,
                VoucherId = h.VoucherId,
                VoucherName = h.Voucher?.VoucherName
            });

            return Ok(new ApiResponse<IEnumerable<PointHistoryResponseDto>>
            {
                Success = true,
                Message = "Point history retrieved successfully.",
                Data = historyDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving point history for citizen {CitizenId}", citizenId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving point history.",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
