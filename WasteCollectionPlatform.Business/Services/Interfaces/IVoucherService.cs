using System.Collections.Generic;
using System.Threading.Tasks;
using WasteCollectionPlatform.Common.DTOs.Request.Voucher;
using WasteCollectionPlatform.Common.DTOs.Response.Voucher;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface IVoucherService
{
    Task<IEnumerable<VoucherResponseDto>> GetAllAsync();
    Task<VoucherResponseDto?> GetByIdAsync(int id);
    Task<VoucherResponseDto> CreateAsync(CreateVoucherDto dto);
    Task<bool> UpdateAsync(int id, UpdateVoucherDto dto);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<VoucherResponseDto>> GetByCitizenIdAsync(int citizenId);
    Task<string> UploadImageAsync(Microsoft.AspNetCore.Http.IFormFile file);
    Task<VoucherResponseDto> RedeemAsync(int citizenId, int voucherId);
}
