using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Response.Common;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DistrictController : ControllerBase
{
    private readonly IDistrictService _districtService;

    public DistrictController(IDistrictService districtService)
    {
        _districtService = districtService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var districts = await _districtService.GetAllAsync();
        return Ok(ApiResponse<object>.SuccessResponse(districts));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var district = await _districtService.GetByIdAsync(id);
        if (district == null) return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy Quận/Huyện"));
        return Ok(ApiResponse<object>.SuccessResponse(district));
    }
}