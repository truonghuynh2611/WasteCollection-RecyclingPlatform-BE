using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemConfigurationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SystemConfigurationController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllConfigurations()
    {
        var configs = await _unitOfWork.SystemConfigurations.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<SystemConfiguration>>.SuccessResponse(configs));
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateConfiguration(string key, [FromBody] UpdateConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Value))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Value cannot be empty."));
        }

        var config = await _unitOfWork.SystemConfigurations.GetByKeyAsync(key);
        if (config == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse($"Configuration key '{key}' not found."));
        }

        config.Value = request.Value;
        
        await _unitOfWork.SystemConfigurations.UpdateAsync(config);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponse<SystemConfiguration>.SuccessResponse(config, "Configuration updated successfully."));
    }
}

public class UpdateConfigRequest
{
    public string Value { get; set; } = null!;
}
