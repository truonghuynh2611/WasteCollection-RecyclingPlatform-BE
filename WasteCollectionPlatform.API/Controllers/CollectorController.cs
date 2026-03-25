using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.DTOs.Response.Collector;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Linq;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;   // UpdateWasteReportDto
using WasteCollectionPlatform.Common.Exceptions;                // BusinessRuleException

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
                await _wasteReportService.UpdateReportStatusAsync(reportId, newStatus);
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

    /// <summary>
    /// [Leader only] Get all reports assigned to the current collector's team
    /// </summary>
    [HttpGet("leader/reports")]
    public async Task<IActionResult> GetTeamReports()
    {
        try
        {
            // 1. Lấy userId từ JWT
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access."));

            // 2. Lấy collector profile
            var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
            if (collector == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Collector profile not found."));

            // 3. Kiểm tra phải là Leader
            if (collector.Role != CollectorRole.Leader)
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Only team leader can access this endpoint."));

            // 4. Lấy danh sách report của team (dùng service đã có)
            var reports = await _wasteReportService.GetByCollectorIdAsync(collector.CollectorId);

            // 5. Map sang DTO (dùng lại CollectorTaskResponseDto đã có sẵn)
            var result = reports.Select(r => new CollectorTaskResponseDto
            {
                ReportId    = r.ReportId,
                Address     = r.Area?.Name,
                Area        = r.Area?.Name,
                District    = r.Area?.District?.DistrictName,
                WasteType   = r.WasteType,
                Priority    = "Bình thường",
                CreatedAt   = r.CreatedAt,
                AssignedBy  = "Admin",
                Status      = r.Status.ToString(),
                Note        = r.Description,
                Rating      = null
            });

            return Ok(ApiResponse<object>.SuccessResponse(result, "Team reports retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team reports for leader.");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred."));
        }
    }

    /// <summary>
    /// [Leader only] Update a report (only Pending reports can be updated)
    /// </summary>
    [HttpPut("leader/reports/{reportId}")]
    public async Task<IActionResult> UpdateReport(int reportId, [FromBody] UpdateWasteReportDto dto)
    {
        try
        {
            // 1. Validate model
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed."));

            // 2. Lấy userId từ JWT
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access."));

            // 3. Lấy collector profile
            var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
            if (collector == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Collector profile not found."));

            // 4. Kiểm tra phải là Leader
            if (collector.Role != CollectorRole.Leader)
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Only team leader can update reports."));

            // 5. Kiểm tra report có thuộc team của Leader không
            var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
            if (report == null)
                return NotFound(ApiResponse<object>.ErrorResponse($"Report {reportId} not found."));

            if (report.TeamId != collector.TeamId)
                return StatusCode(403, ApiResponse<object>.ErrorResponse("You can only update reports assigned to your team."));

            // 6. Gọi service update (đã có sẵn logic, chỉ cho phép Pending)
            var updated = await _wasteReportService.UpdateAsync(reportId, dto);

            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                updated.ReportId,
                updated.Description,
                updated.WasteType,
                updated.AreaId,
                Status = updated.Status.ToString()
            }, "Report updated successfully."));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId} by leader.", reportId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred."));
        }
    }

    /// <summary>
    /// [Leader only] Delete a report (only Pending reports can be deleted)
    /// </summary>
    [HttpDelete("leader/reports/{reportId}")]
    public async Task<IActionResult> DeleteReport(int reportId)
    {
        try
        {
            // 1. Lấy userId từ JWT
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized access."));

            // 2. Lấy collector profile
            var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
            if (collector == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Collector profile not found."));

            // 3. Kiểm tra phải là Leader
            if (collector.Role != CollectorRole.Leader)
                return StatusCode(403, ApiResponse<object>.ErrorResponse("Only team leader can delete reports."));

            // 4. Kiểm tra report có thuộc team của Leader không
            var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
            if (report == null)
                return NotFound(ApiResponse<object>.ErrorResponse($"Report {reportId} not found."));

            if (report.TeamId != collector.TeamId)
                return StatusCode(403, ApiResponse<object>.ErrorResponse("You can only delete reports assigned to your team."));

            // 5. Gọi service delete (đã có sẵn logic, chỉ cho phép Pending)
            var success = await _wasteReportService.DeleteAsync(reportId);
            if (!success)
                return NotFound(ApiResponse<object>.ErrorResponse($"Report {reportId} not found."));

            return Ok(ApiResponse<object>.SuccessResponse(null, "Report deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId} by leader.", reportId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
