using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Voucher;
using WasteCollectionPlatform.Common.DTOs.Response.Voucher;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class VoucherService : IVoucherService
{
    private readonly IVoucherRepository _voucherRepo;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly IUnitOfWork _unitOfWork;

    public VoucherService(IVoucherRepository voucherRepo, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, IUnitOfWork unitOfWork)
    {
        _voucherRepo = voucherRepo;
        _env = env;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<VoucherResponseDto>> GetAllAsync()
    {
        var vouchers = await _voucherRepo.GetAllAsync();
        return vouchers.Select(v => MapToResponse(v));
    }

    public async Task<VoucherResponseDto?> GetByIdAsync(int id)
    {
        var voucher = await _voucherRepo.GetByIdAsync(id);
        return voucher != null ? MapToResponse(voucher) : null;
    }

    public async Task<VoucherResponseDto> CreateAsync(CreateVoucherDto dto)
    {
        var voucher = new Voucher
        {
            VoucherName = dto.VoucherName,
            Description = dto.Description,
            VoucherCode = dto.VoucherCode,
            Image = dto.Image,
            Category = dto.Category,
            ExpiryDays = dto.ExpiryDays,
            PointsRequired = dto.PointsRequired,
            StockQuantity = dto.StockQuantity,
            Status = true
        };

        await _voucherRepo.AddAsync(voucher);
        await _voucherRepo.SaveChangesAsync();

        return MapToResponse(voucher);
    }

    public async Task<bool> UpdateAsync(int id, UpdateVoucherDto dto)
    {
        var voucher = await _voucherRepo.GetByIdAsync(id);
        if (voucher == null) return false;

        voucher.VoucherName = dto.VoucherName;
        voucher.Description = dto.Description;
        voucher.VoucherCode = dto.VoucherCode;
        voucher.Image = dto.Image;
        voucher.Category = dto.Category;
        voucher.ExpiryDays = dto.ExpiryDays;
        voucher.PointsRequired = dto.PointsRequired;
        voucher.StockQuantity = dto.StockQuantity;
        voucher.Status = dto.Status;

        await _voucherRepo.UpdateAsync(voucher);
        await _voucherRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var voucher = await _voucherRepo.GetByIdAsync(id);
        if (voucher == null) return false;

        await _voucherRepo.DeleteAsync(voucher);
        await _voucherRepo.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<VoucherResponseDto>> GetByCitizenIdAsync(int citizenId)
    {
        // Try to find citizen by CitizenId first, then by UserId (fallback)
        var citizen = await _unitOfWork.Citizens.GetByIdAsync(citizenId);
        if (citizen == null)
        {
            citizen = await _unitOfWork.Citizens.GetByUserIdAsync(citizenId);
        }

        if (citizen == null)
        {
            return new List<VoucherResponseDto>();
        }

        var vouchers = await _voucherRepo.GetByCitizenIdAsync(citizen.CitizenId);
        return vouchers.Select(v => MapToResponse(v));
    }

    public async Task<string> UploadImageAsync(Microsoft.AspNetCore.Http.IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new System.Exception("No file uploaded");

        // Save to BE wwwroot/voucher
        var wwwrootVoucherPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "voucher");

        if (!Directory.Exists(wwwrootVoucherPath))
        {
            Directory.CreateDirectory(wwwrootVoucherPath);
        }

        var fileName = $"{System.Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(wwwrootVoucherPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path — served by UseStaticFiles from wwwroot
        return $"/voucher/{fileName}";
    }

    private static VoucherResponseDto MapToResponse(Voucher v)
    {
        return new VoucherResponseDto
        {
            VoucherId = v.VoucherId,
            VoucherName = v.VoucherName,
            Description = v.Description,
            VoucherCode = v.VoucherCode,
            Image = v.Image,
            Category = v.Category,
            ExpiryDays = v.ExpiryDays,
            PointsRequired = v.PointsRequired,
            StockQuantity = v.StockQuantity,
            Status = v.Status ?? true
        };
    }

    public async Task<VoucherResponseDto> RedeemAsync(int citizenId, int voucherId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var voucher = await _voucherRepo.GetByIdAsync(voucherId);
            if (voucher == null)
                throw new System.Exception("Voucher không tồn tại.");

            if (voucher.StockQuantity <= 0)
                throw new System.Exception("Voucher đã hết hàng.");

            var citizen = await _unitOfWork.Citizens.GetByIdAsync(citizenId);
            if (citizen == null)
                throw new System.Exception("Citizen không tồn tại.");

            if ((citizen.TotalPoints ?? 0) < voucher.PointsRequired)
                throw new System.Exception("Bạn không đủ điểm để đổi voucher này.");

            // Deduct points
            citizen.TotalPoints = (citizen.TotalPoints ?? 0) - voucher.PointsRequired;

            // Decrease stock
            voucher.StockQuantity -= 1;

            // Record in PointHistory (negative amount = redeemed)
            var pointLog = new PointHistory
            {
                CitizenId = citizenId,
                VoucherId = voucherId,
                PointAmount = -voucher.PointsRequired,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.PointHistories.AddAsync(pointLog);
            await _unitOfWork.CommitTransactionAsync();

            return MapToResponse(voucher);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
