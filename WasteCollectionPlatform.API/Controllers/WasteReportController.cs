using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;

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

	[HttpPost("assign/{id}")]
	public async Task<IActionResult> Assign(int id)
	{
		try
		{
			await _wasteReportService.AssignReportAsync(id);
			return Ok("Report assigned successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error assigning waste report {ReportId}", id);
			return BadRequest(ex.Message);
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
}
