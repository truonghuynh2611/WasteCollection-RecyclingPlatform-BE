namespace WasteCollectionPlatform.Common.DTOs.Request.Voucher;

public class CreateVoucherDto
{
    public string VoucherName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? VoucherCode { get; set; }
    public string? Image { get; set; }
    public string? Category { get; set; }
    public int? ExpiryDays { get; set; }
    public int PointsRequired { get; set; }
    public int StockQuantity { get; set; }
}
