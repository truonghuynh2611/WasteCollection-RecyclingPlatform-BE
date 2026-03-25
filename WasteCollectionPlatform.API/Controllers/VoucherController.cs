using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Voucher;
using WasteCollectionPlatform.Common.DTOs.Response.Voucher;

namespace WasteCollectionPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    private readonly ILogger<VoucherController> _logger;

    public VoucherController(IVoucherService voucherService, ILogger<VoucherController> logger)
    {
        _voucherService = voucherService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VoucherResponseDto>>> GetAll()
    {
        var vouchers = await _voucherService.GetAllAsync();
        return Ok(vouchers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VoucherResponseDto>> GetById(int id)
    {
        var voucher = await _voucherService.GetByIdAsync(id);
        if (voucher == null) return NotFound();
        return Ok(voucher);
    }

    [HttpPost]
    public async Task<ActionResult<VoucherResponseDto>> Create(CreateVoucherDto dto)
    {
        try
        {
            var voucher = await _voucherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = voucher.VoucherId }, voucher);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating voucher");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateVoucherDto dto)
    {
        try
        {
            var result = await _voucherService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _voucherService.DeleteAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("citizen/{citizenId}")]
    public async Task<ActionResult<IEnumerable<VoucherResponseDto>>> GetByCitizen(int citizenId)
    {
        var vouchers = await _voucherService.GetByCitizenIdAsync(citizenId);
        return Ok(vouchers);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var imageUrl = await _voucherService.UploadImageAsync(file);
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading voucher image");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("redeem")]
    public async Task<IActionResult> Redeem([FromBody] RedeemVoucherRequest request)
    {
        try
        {
            var result = await _voucherService.RedeemAsync(request.CitizenId, request.VoucherId);
            return Ok(new { 
                success = true, 
                message = "Đổi quà thành công!", 
                voucherCode = result.VoucherCode,
                voucherName = result.VoucherName,
                pointsDeducted = result.PointsRequired
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error redeeming voucher");
            var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { success = false, message = innerMsg });
        }
    }
}

public class RedeemVoucherRequest
{
    public int CitizenId { get; set; }
    public int VoucherId { get; set; }
}
