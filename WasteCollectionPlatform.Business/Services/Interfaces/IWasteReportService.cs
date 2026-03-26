using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.Common.Enums;

namespace WasteCollectionPlatform.Business.Services.Interfaces;

public interface IWasteReportService
{
	Task<IEnumerable<WasteReport>> GetAllAsync();
	Task<WasteReport?> GetByIdAsync(int id);
	Task<WasteReport> CreateAsync(CreateWasteReportDto dto);
	Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId);
	Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId);
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
    Task<WasteReport> UpdateAsync(int id, UpdateWasteReportDto dto);
    Task UpdateReportStatusAsync(int reportId, ReportStatus newStatus);
    
    // Flow 3: Sequential Workflow
    Task ApproveAndAssignToMainTeamAsync(int reportId);
    Task SubmitCompletionEvidenceAsync(int reportId, int leaderId, Microsoft.AspNetCore.Http.IFormFileCollection? imageFiles, List<string>? imageUrls, string? note);
    Task VerifyAndFinalizeReportAsync(int reportId, bool isApproved, string? adminNote);
}

