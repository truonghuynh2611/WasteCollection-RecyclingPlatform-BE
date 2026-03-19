using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.DTOs.Response.Collector;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Collector portal endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CollectorController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWasteReportService _wasteReportService;
    private readonly ILogger<CollectorController> _logger;


    public CollectorController(
        IUnitOfWork unitOfWork,
        IWasteReportService wasteReportService,
        ILogger<CollectorController> logger)
    {
        _unitOfWork = unitOfWork;
        _wasteReportService = wasteReportService;
        _logger = logger;
    }

    /// <summary>
    /// Get tasks assigned to the current collector's team
    /// </summary>
    /// <returns>List of tasks</returns>
    [HttpGet("tasks")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollectorTaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTasks()
    {
        try
        {
            // Get UserId from JWT
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access."));
            }

            var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
            if (collector == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Collector profile not found."));
            }

            var reports = await _wasteReportService.GetByCollectorIdAsync(collector.CollectorId);
            
            var taskDtos = reports.Select(r => new CollectorTaskResponseDto
            {
                ReportId = r.ReportId,
                Address = r.Area?.Name,
                Area = r.Area?.Name,
                District = r.Area?.District?.DistrictName,
                WasteType = r.WasteType,
                Priority = "Bình thường", // Mocked for now as it's not in DB yet
                CreatedAt = r.CreatedAt,
                AssignedBy = "Admin", // Mocked
                Status = r.Status.ToString(),
                Note = r.Description,
                Rating = null // Mocked
            });

            return Ok(ApiResponse<IEnumerable<CollectorTaskResponseDto>>.SuccessResponse(taskDtos, "Retrieved tasks successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for collector.");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving tasks."));
        }
    }

    /// <summary>
    /// Update task status
    /// </summary>
    /// <param name="reportId">Report ID</param>
    /// <param name="status">New status</param>
    /// <returns>Success response</returns>
    [HttpPatch("tasks/{reportId}/status")]
    public async Task<IActionResult> UpdateTaskStatus(int reportId, [FromBody] string status)
    {
        try
        {
            var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
            if (report == null) return NotFound(ApiResponse<object>.ErrorResponse("Report not found."));

            if (Enum.TryParse<ReportStatus>(status, true, out var newStatus))
            {
                report.Status = newStatus;
                await _unitOfWork.WasteReports.UpdateAsync(report);
                await _unitOfWork.SaveChangesAsync();
                return Ok(ApiResponse<object>.SuccessResponse(null, "Status updated successfully."));
            }

            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid status value."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status.");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating status."));
        }
    }
}
