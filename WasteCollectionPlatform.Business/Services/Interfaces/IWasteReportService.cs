using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;
using WasteCollectionPlatform.DataAccess.Entities;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface IWasteReportService
{
	Task<IEnumerable<WasteReport>> GetAllAsync();
	Task<WasteReport?> GetByIdAsync(int id);
	Task<WasteReport> CreateAsync(CreateWasteReportDto dto);
	Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId);
	Task AssignReportAsync(int reportId);
    Task CancelReportAsync(CancelReportRequestDto request);
    Task ProcessReportAsync(
		int reportId,
		int collectorId,
		bool isValid,
		string? collectorImageUrl,
		decimal? latitude,
		decimal? longitude);
	Task<bool> DeleteAsync(int id);
	Task ConfirmReportAsync(int reportId, int collectorId);
}
