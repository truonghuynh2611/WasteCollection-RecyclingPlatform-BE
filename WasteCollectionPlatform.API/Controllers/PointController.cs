using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PointController : ControllerBase
{
    [HttpGet("rules")]
    public IActionResult GetPointRules()
    {
        var rules = new[]
        {
            new { id = 1, wasteType = "Nhựa", pointsPerKg = 10, description = "Chai lọ, túi nilon, đồ nhựa..." },
            new { id = 2, wasteType = "Giấy", pointsPerKg = 5, description = "Giấy báo, thùng cartoon, sách cũ..." },
            new { id = 3, wasteType = "Kim loại", pointsPerKg = 15, description = "Lon nhôm, sắt vụn, đồng..." },
            new { id = 4, wasteType = "Nguy hại", pointsPerKg = 50, description = "Pin, bóng đèn, thiết bị điện tử..." },
            new { id = 5, wasteType = "Thủy tinh", pointsPerKg = 8, description = "Chai lọ thủy tinh, gương..." }
        };

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Retrieved point rules successfully.",
            Data = rules
        });
    }
}
