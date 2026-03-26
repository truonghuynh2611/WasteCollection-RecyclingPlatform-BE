using Microsoft.Extensions.Logging;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class WasteReportService : IWasteReportService
{
	private readonly INotificationService _notificationService;
	private readonly HttpClient _httpClient;
	private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly ILogger<WasteReportService> _logger;
    private readonly IUnitOfWork _unitOfWork;

	public WasteReportService(
		INotificationService notificationService,
		HttpClient httpClient,
		Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
		ILogger<WasteReportService> logger,
		IUnitOfWork unitOfWork)
	{
		_notificationService = notificationService;
		_httpClient = httpClient;
		_env = env;
		_logger = logger;
		_unitOfWork = unitOfWork;
	}

	public async Task<IEnumerable<WasteReport>> GetAllAsync()
	{
		return await _unitOfWork.WasteReports.GetAllAsync();
	}

    public async Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int id)
    {
        // Try to find citizen by CitizenId first, then by UserId
        var citizen = await _unitOfWork.Citizens.GetByIdAsync(id);
        if (citizen == null)
        {
            citizen = await _unitOfWork.Citizens.GetByUserIdAsync(id);
        }

        if (citizen == null)
        {
            return new List<WasteReport>();
        }
        return await _unitOfWork.WasteReports.GetByCitizenIdAsync(citizen.CitizenId);
    }

	public async Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId)
	{
		return await _unitOfWork.WasteReports.GetByCollectorIdAsync(collectorId);
	}

	public async Task<WasteReport?> GetByIdAsync(int id)
	{
		return await _unitOfWork.WasteReports.GetByIdAsync(id);
	}

	public async Task<WasteReport> CreateAsync(CreateWasteReportDto dto)
	{
		// Try to find citizen by CitizenId (PK) or UserId (fallback for frontend sends user.id)
		var citizen = await _unitOfWork.Citizens.GetByIdAsync(dto.CitizenId);
		if (citizen == null)
		{
			citizen = await _unitOfWork.Citizens.GetByUserIdAsync(dto.CitizenId);
		}

		if (citizen == null)
		{
			// Check if the provided ID is actually a valid User ID before auto-creating
			var user = await _unitOfWork.Users.GetByIdAsync(dto.CitizenId);
			if (user != null)
			{
				citizen = new Citizen
				{
					UserId = user.UserId,
					User = user,
					TotalPoints = 0
				};
				// Explicitly link from both sides just in case
				user.Citizen = citizen;
				
				await _unitOfWork.Citizens.AddAsync(citizen);
				await _unitOfWork.SaveChangesAsync(); // Commit NOW to get the auto-generated CitizenId
			}
			else
			{
				throw new NotFoundException($"Citizen not found for ID {dto.CitizenId}");
			}
		}

		if (!await _unitOfWork.Areas.ExistsAsync(dto.AreaId))
		{
			throw new BusinessRuleException("Area does not exist");
		}

		var wasteReport = new WasteReport
		{
			Description = string.Join("; ", dto.Items.Select(i => $"{i.WasteType}: {i.Description}")),
			CitizenId = citizen.CitizenId,
			AreaId = dto.AreaId,
			WasteType = string.Join(", ", dto.Items.Select(i => i.WasteType)),
			CreatedAt = DateTime.UtcNow,
			Status = ReportStatus.Pending,
			ExpireTime = DateTime.UtcNow.AddHours(24),
			TeamId = null
		};

		await _unitOfWork.WasteReports.AddAsync(wasteReport);

		foreach (var itemDto in dto.Items)
		{
			string? itemImageUrl = null;
			if (itemDto.ImageFile != null && itemDto.ImageFile.Length > 0)
			{
				itemImageUrl = await SaveImageAsync(itemDto.ImageFile);
			}

			var reportItem = new WasteReportItem
			{
				WasteType = itemDto.WasteType,
				Description = itemDto.Description,
				ImageUrl = itemImageUrl,
				WasteReport = wasteReport // Link to the report object
			};

			await _unitOfWork.WasteReportItems.AddAsync(reportItem);

			// Also add the image to the general ReportImages collection
			if (!string.IsNullOrEmpty(itemImageUrl))
			{
				var reportImage = new ReportImage
				{
					Imageurl = itemImageUrl,
					Report = wasteReport
				};
				await _unitOfWork.ReportImages.AddAsync(reportImage);
			}
		}
		
		await _unitOfWork.SaveChangesAsync();

		// Notify all Admins about the new report
		try
		{
			await _notificationService.SendNotificationToRoleAsync(
				UserRole.Admin,
				$"Bạn đang có 1 báo cáo rác mới từ người dân (Mã: #{wasteReport.ReportId})",
				wasteReport.ReportId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send notification for new report {ReportId}", wasteReport.ReportId);
		}

		return wasteReport;
	}

	private async Task<string> SaveImageAsync(Microsoft.AspNetCore.Http.IFormFile file)
	{
		var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
		if (!Directory.Exists(uploadsFolder))
		{
			Directory.CreateDirectory(uploadsFolder);
		}

		var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
		var filePath = Path.Combine(uploadsFolder, fileName);

		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}

		return $"/uploads/{fileName}";
	}

	private async Task<bool> IsValidImageUrlAsync(string? url)
	{
		if (string.IsNullOrWhiteSpace(url)) return false;
		if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
		if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return false;

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Head, uri);
			using var response = await _httpClient.SendAsync(request);
			if (!response.IsSuccessStatusCode) return false;
			var contentType = response.Content.Headers.ContentType?.MediaType;
			return contentType != null && contentType.StartsWith("image/");
		}
		catch
		{
			return false;
		}
	}

	public async Task AssignReportAsync(int reportId)
	{
		var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
		if (report == null) throw new NotFoundException("Report not found");
		if (report.Status != ReportStatus.Pending) throw new BusinessRuleException("Report must be pending to assign");
		if (report.ExpireTime != null && report.ExpireTime < DateTime.UtcNow) throw new BusinessRuleException("Report expired");

		var primaryTeam = await _unitOfWork.Teams.GetTeamWithCollectorsAsync(report.AreaId, TeamType.Main);
		if (primaryTeam == null) throw new BusinessRuleException("Primary team not found");

		var selectedTeam = primaryTeam;
		var noActiveCollector = !primaryTeam.Collectors.Any(c => c.Status == true);

		if (primaryTeam.CurrentTaskCount >= 5 || noActiveCollector)
		{
			var supportTeam = await _unitOfWork.Teams.GetTeamWithCollectorsAsync(report.AreaId, TeamType.Support);
			if (supportTeam != null && supportTeam.Collectors.Any(c => c.Status == true))
			{
				selectedTeam = supportTeam;
			}
			else
			{
				throw new BusinessRuleException("No available team to assign");
			}
		}

		report.TeamId = selectedTeam.TeamId;
		report.Status = ReportStatus.Assigned;
		selectedTeam.CurrentTaskCount++;

		await _unitOfWork.Teams.UpdateAsync(selectedTeam);
		await _unitOfWork.WasteReports.UpdateAsync(report);
		await _unitOfWork.SaveChangesAsync();

		try
		{
			await _notificationService.SendNotificationToTeamAsync(
				selectedTeam.TeamId,
				$"Bạn vừa được phân công thu gom một đơn báo cáo rác (Mã: #{report.ReportId})",
				report.ReportId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send notification for assigned report {ReportId}", report.ReportId);
		}
	}

    public async Task CancelReportAsync(CancelReportRequestDto request)
    {
        var report = await _unitOfWork.WasteReports.GetByIdAsync(request.ReportId);
        if (report == null) throw new NotFoundException("Report not found");
        if (report.Status != ReportStatus.Pending) throw new BusinessRuleException("Only reports in Pending status can be cancelled");

        report.Status = ReportStatus.Failed;
        await _unitOfWork.WasteReports.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(request.Reason))
        {
            _logger.LogInformation("Admin cancelled report {ReportId}. Reason: {Reason}", request.ReportId, request.Reason);
        }
    }

    public async Task ProcessReportAsync(
		int reportId,
		int collectorId,
		bool isValid,
		string? collectorImageUrl)
	{
		var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
		if (report == null) throw new NotFoundException("Report not found");
		if (report.Status != ReportStatus.Assigned) throw new BusinessRuleException("Report is not assigned");
		if (!report.TeamId.HasValue) throw new BusinessRuleException("Report has no assigned team");

		var collector = collectorId > 0 ? await _unitOfWork.Collectors.GetByIdAsync(collectorId) : null;
		if (collectorId > 0 && collector == null) throw new NotFoundException("Collector not found");
		if (collectorId > 0 && collector.TeamId != report.TeamId) throw new BusinessRuleException("You are not in the assigned team");
		if (collectorId > 0 && collector.Role != CollectorRole.Leader) throw new BusinessRuleException("Only team leader can submit completion report");

		var citizen = await _unitOfWork.Citizens.GetByIdAsync(report.CitizenId);
		if (citizen == null) throw new NotFoundException("Citizen not found");

		if (!string.IsNullOrEmpty(collectorImageUrl) && !await IsValidImageUrlAsync(collectorImageUrl))
		{
			throw new BusinessRuleException("Invalid collector image URL.");
		}

		if (!string.IsNullOrWhiteSpace(collectorImageUrl))
		{
			var reportImage = new ReportImage
			{
				ReportId = report.ReportId,
				Imageurl = collectorImageUrl
			};
			await _unitOfWork.ReportImages.AddAsync(reportImage);
		}


        int pointsForCompleted = 10;
        int pointsForCancelled = -5;
        
        var completedConfig = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CompletedReport");
        if (completedConfig != null && int.TryParse(completedConfig.Value, out int parsedCompleted))
            pointsForCompleted = parsedCompleted;
        
        var cancelledConfig = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CancelledReport");
        if (cancelledConfig != null && int.TryParse(cancelledConfig.Value, out int parsedCancelled))
            pointsForCancelled = parsedCancelled;

		if (isValid)
		{
			report.Status = ReportStatus.Collected;
			citizen.TotalPoints = (citizen.TotalPoints ?? 0) + pointsForCompleted;
            
            var pointLog = new PointHistory
            {
                CitizenId = citizen.CitizenId,
                ReportId = report.ReportId,
                PointAmount = pointsForCompleted,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PointHistories.AddAsync(pointLog);
		}
		else
		{
			report.Status = ReportStatus.Failed;
			citizen.TotalPoints = Math.Max(0, (citizen.TotalPoints ?? 0) + pointsForCancelled);
            
            var pointLog = new PointHistory
            {
                CitizenId = citizen.CitizenId,
                ReportId = report.ReportId,
                PointAmount = pointsForCancelled,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.PointHistories.AddAsync(pointLog);
		}

		var team = await _unitOfWork.Teams.GetByIdAsync(report.TeamId.Value);
		if (team != null && team.CurrentTaskCount > 0)
		{
			team.CurrentTaskCount--;
			await _unitOfWork.Teams.UpdateAsync(team);
		}

		await _unitOfWork.WasteReports.UpdateAsync(report);
		await _unitOfWork.Citizens.UpdateAsync(citizen);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task ConfirmReportAsync(int reportId, int collectorId)
	{
		var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
		if (report == null) throw new NotFoundException("Report not found");
		if (report.Status != ReportStatus.Assigned) throw new BusinessRuleException("Report must be in Assigned status to confirm");

		var collector = await _unitOfWork.Collectors.GetByIdAsync(collectorId);
		if (collector == null) throw new NotFoundException("Collector not found");
		if (collector.TeamId != report.TeamId) throw new BusinessRuleException("You are not in the assigned team");

		report.Status = ReportStatus.OnTheWay;
		await _unitOfWork.WasteReports.UpdateAsync(report);
		await _unitOfWork.SaveChangesAsync();

		try
		{
			await _notificationService.SendNotificationToRoleAsync(
				UserRole.Admin,
				$"Báo cáo rác (Mã: #{report.ReportId}) đang được xử lí bởi nhân viên thu gom",
				report.ReportId);

			var citizen = await _unitOfWork.Citizens.GetByIdAsync(report.CitizenId);
			if (citizen != null)
			{
				await _notificationService.SendNotificationAsync(
					citizen.UserId,
					$"Báo cáo rác của bạn (Mã: #{report.ReportId}) đang được xử lí",
					report.ReportId);
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send notification for confirmed report {ReportId}", report.ReportId);
		}
	}

    public async Task<bool> DeleteAsync(int id)
	{
		var report = await _unitOfWork.WasteReports.GetByIdAsync(id);
		if (report == null) return false;
		if (report.Status != ReportStatus.Pending) throw new BusinessRuleException("Only pending reports can be deleted");

		await _unitOfWork.WasteReports.DeleteAsync(report);
		await _unitOfWork.SaveChangesAsync();
		return true;
	}

    public async Task UpdateReportStatusAsync(int reportId, ReportStatus newStatus)
    {
        var report = await _unitOfWork.WasteReports.GetByIdAsync(reportId);
        if (report == null) throw new NotFoundException("Report not found");
        if (report.Status == newStatus) return;

        if (newStatus == ReportStatus.Collected || newStatus == ReportStatus.Failed)
        {
            await ProcessReportAsync(reportId, 0, newStatus == ReportStatus.Collected, null);
        }
        else
        {
            report.Status = newStatus;
            await _unitOfWork.WasteReports.UpdateAsync(report);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<WasteReport> UpdateAsync(int id, UpdateWasteReportDto dto)
    {
        var report = await _unitOfWork.WasteReports.GetByIdAsync(id);
        if (report == null) throw new NotFoundException("Report not found");
        if (report.Status != ReportStatus.Pending) throw new BusinessRuleException("Only pending reports can be updated");

        report.Description = dto.Description;
        report.WasteType = dto.WasteType;
        report.AreaId = dto.AreaId;
        

        await _unitOfWork.WasteReports.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return report;
    }
}

