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

	[HttpPost("confirm/{id}")]
	public async Task<IActionResult> Confirm(int id, [FromQuery] int collectorId)
	{
		try
		{
			await _wasteReportService.ConfirmReportAsync(id, collectorId);
			return Ok("Report confirmed and is now being processed");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error confirming waste report {ReportId}", id);
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
	public async Task<IActionResult> AssignReport(int id)
	{
		try
		{
			await _wasteReportService.ApproveAndAssignToMainTeamAsync(id);
			return Ok("Report approved and assigned to Main Team successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error approving and assigning waste report {ReportId}", id);
			return BadRequest(ex.Message);
		}
	}

    [HttpPost("submit-completion")]
    public async Task<IActionResult> SubmitCompletion([FromForm] SubmitCompletionEvidenceDto dto)
    {
        try
        {
            await _wasteReportService.SubmitCompletionEvidenceAsync(dto.ReportId, dto.LeaderId, dto.ImageFiles, dto.ImageUrls, dto.Note);
            return Ok("Evidence submitted for review");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting completion for report {ReportId}", dto.ReportId);
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("verify-completion")]
    public async Task<IActionResult> VerifyCompletion([FromBody] VerifyCompletionDto dto)
    {
        try
        {
            await _wasteReportService.VerifyAndFinalizeReportAsync(dto.ReportId, dto.IsApproved, dto.AdminNote);
            return Ok(dto.IsApproved ? "Report finalized and points awarded" : "Report rejected and returned to team");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying completion for report {ReportId}", dto.ReportId);
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWasteReportDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _wasteReportService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating waste report {ReportId}", id);
            return StatusCode(500, ex.Message);
        }
    }

    private bool IsAdmin()
	{
		
		return User.IsInRole("Admin");
	}
}


