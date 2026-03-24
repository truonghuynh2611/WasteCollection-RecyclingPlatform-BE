using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WasteReportController : ControllerBase
{
	private readonly IWasteReportService _wasteReportService;
	private readonly ILogger<WasteReportController> _logger;

	public WasteReportController(
		IWasteReportService wasteReportService,
		ILogger<WasteReportController> logger)
	{
		_wasteReportService = wasteReportService;
		_logger = logger;
	}

	[HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateReport([FromForm] CreateWasteReportDto dto)
	{
		try
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _wasteReportService.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = result.ReportId }, result);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating waste report");
			return BadRequest(ex.Message);
		}
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		try
		{
			var reports = await _wasteReportService.GetAllAsync();
			return Ok(reports);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving waste reports");
			return StatusCode(500, ex.Message);
		}
	}

	[HttpGet("citizen/{citizenId}")]
        public async Task<IActionResult> GetByCitizenId(int citizenId)
        {
                try
                {
                        var reports = await _wasteReportService.GetByCitizenIdAsync(citizenId);
                        return Ok(reports);
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "Error retrieving waste reports for citizen {CitizenId}", citizenId);
                        return StatusCode(500, ex.Message);
                }
        }

        [HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		try
		{
			var report = await _wasteReportService.GetByIdAsync(id);

			if (report == null)
			{
				return NotFound($"Report with ID {id} not found.");
			}

			return Ok(report);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving waste report {ReportId}", id);
			return StatusCode(500, ex.Message);
		}
	}

	[HttpPost("process")]
	public async Task<IActionResult> Process([FromBody] ProcessReportDto dto)
	{
		try
		{
			await _wasteReportService.ProcessReportAsync(
				dto.ReportId,
				dto.CollectorId,
				dto.IsValid,
				dto.CollectorImageUrl,
				dto.Latitude,
				dto.Longitude);

			return Ok("Report processed successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing waste report {ReportId}", dto.ReportId);
			return BadRequest(ex.Message);
		}
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		try
		{
			var success = await _wasteReportService.DeleteAsync(id);

			if (!success)
			{
				return NotFound($"Report with ID {id} not found.");
			}

			return Ok("Report deleted successfully.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting waste report {ReportId}", id);
			return BadRequest(ex.Message);
		}
	}

	/// <summary>
	/// Assign waste report to team (Admin only)
	/// </summary>
	/// <param name="id">Waste Report ID</param>
	/// <returns>Success message</returns>
	[HttpPost("{id}/assign")]
	[Authorize]
	public async Task<IActionResult> AssignReport(int id)
	{
		try
		{
			// Only admin users can assign reports
			if (!IsAdmin())
			{
				return Forbid();
			}

			await _wasteReportService.AssignReportAsync(id);
			return Ok("Report assigned successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error assigning waste report {ReportId}", id);
			return BadRequest(ex.Message);
		}
	}
    [HttpPost("cancel-report")]
    public async Task<IActionResult> CancelReport([FromBody] CancelReportRequestDto request)
    {
        try
        {
            // ? truy?n nguyên DTO
            await _wasteReportService.CancelReportAsync(request);

            return Ok(ApiResponse<object>.SuccessResponse(null, "Report cancelled successfully"));
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
            _logger.LogError(ex, "Error cancelling report {ReportId}", request.ReportId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
    private bool IsAdmin()
	{
		var adminIdClaim = User.FindFirst("adminId");
		return adminIdClaim != null;
	}
}


