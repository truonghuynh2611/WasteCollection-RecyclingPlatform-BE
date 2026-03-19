using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AreasController : ControllerBase
{
    private readonly IAreaService _areaService;
    private readonly ILogger<AreasController> _logger;

    public AreasController(IAreaService areaService, ILogger<AreasController> logger)
    {
        _areaService = areaService;
        _logger = logger;
    }

    /// <summary>a
    /// Admin tạo Area theo District
    /// </summary>
   // [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateArea([FromBody] CreateAreaRequestDto request)
    {
        try
        {
            // 🔥 Validate Model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
            }

            var result = await _areaService.CreateAreaAsync(request);

            return StatusCode(201,
                ApiResponse<object>.SuccessResponse(result, "Tạo Area thành công"));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Create Area failed");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating Area");

            return StatusCode(500,
                ApiResponse<object>.ErrorResponse(
                    ex.InnerException?.Message ?? ex.Message
                ));
        }
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var areas = await _areaService.GetAllAreasAsync();
        return Ok(ApiResponse<object>.SuccessResponse(areas));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var area = await _areaService.GetAreaByIdAsync(id);
        if (area == null) return NotFound(ApiResponse<object>.ErrorResponse("Area not found"));
        return Ok(ApiResponse<object>.SuccessResponse(area));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArea(int id, [FromBody] UpdateAreaRequestDto request)
    {
        await _areaService.UpdateAreaAsync(id, request);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Area updated"));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArea(int id)
    {
        await _areaService.DeleteAreaAsync(id);
        return Ok(ApiResponse<object>.SuccessResponse(null, "Area deleted"));
    }
}