using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        try
        {
            var stats = await _dashboardService.GetAdminDashboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("collector")]
    [Authorize(Roles = "Collector,Admin")]
    public async Task<IActionResult> GetCollectorDashboard()
    {
        try
        {
            // Extract UserId from token
            var userIdStr = User.FindFirst("UserId")?.Value 
                         ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("User identity not found in token");
            
            int userId = int.Parse(userIdStr);
            var stats = await _dashboardService.GetCollectorDashboardStatsAsync(userId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
