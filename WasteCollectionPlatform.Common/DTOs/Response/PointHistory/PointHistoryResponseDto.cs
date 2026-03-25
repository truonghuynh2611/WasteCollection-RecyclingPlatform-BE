namespace WasteCollectionPlatform.Common.DTOs.Response.PointHistory;

public class PointHistoryResponseDto
{
    public int PointHistoryId { get; set; }
    public int CitizenId { get; set; }
    public int PointAmount { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public int? VoucherId { get; set; }
    public string? VoucherName { get; set; }
}
