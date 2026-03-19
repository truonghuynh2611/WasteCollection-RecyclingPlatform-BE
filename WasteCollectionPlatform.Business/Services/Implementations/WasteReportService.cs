using Microsoft.Extensions.Logging;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.WasteReport;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Entities;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace WasteCollectionPlatform.Business.Services.Implementations;

public class WasteReportService : IWasteReportService
{
	private readonly IWasteReportRepository _wasteReportRepo;
	private readonly ICitizenRepository _citizenRepo;
	private readonly IAreaRepository _areaRepo;
	private readonly ITeamRepository _teamRepo;
	private readonly ICollectorRepository _collectorRepo;
	private readonly IUserRepository _userRepo;
	private readonly IReportImageRepository _reportImageRepo;
	private readonly INotificationService _notificationService;
	private readonly HttpClient _httpClient;
	private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly ILogger<WasteReportService> _logger;
    private readonly IUnitOfWork _unitOfWork;


	public WasteReportService(
		IWasteReportRepository wasteReportRepo,
		ICitizenRepository citizenRepo,
		IAreaRepository areaRepo,
		ITeamRepository teamRepo,
		ICollectorRepository collectorRepo,
		IUserRepository userRepo,
		IReportImageRepository reportImageRepo,
		INotificationService notificationService,
		HttpClient httpClient,
		Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
		ILogger<WasteReportService> logger,
		IUnitOfWork unitOfWork)

	{
		_wasteReportRepo = wasteReportRepo;
		_citizenRepo = citizenRepo;
		_areaRepo = areaRepo;
		_teamRepo = teamRepo;
		_collectorRepo = collectorRepo;
		_userRepo = userRepo;
		_reportImageRepo = reportImageRepo;
		_notificationService = notificationService;

		_httpClient = httpClient;
		_env = env;
		_logger = logger;
		_unitOfWork = unitOfWork;
	}

	public async Task<IEnumerable<WasteReport>> GetAllAsync()
	{
		return await _wasteReportRepo.GetAllAsync();
	}

    public async Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int id)
    {
        // Try to find citizen by CitizenId first, then by UserId
        var citizen = await _citizenRepo.GetByIdAsync(id);
        if (citizen == null)
        {
            citizen = await _citizenRepo.GetByUserIdAsync(id);
        }

        if (citizen == null)
        {
            return new List<WasteReport>();
        }
        return await _wasteReportRepo.GetByCitizenIdAsync(citizen.CitizenId);
    }

	public async Task<IEnumerable<WasteReport>> GetByCollectorIdAsync(int collectorId)
	{
		return await _wasteReportRepo.GetByCollectorIdAsync(collectorId);
	}

	public async Task<WasteReport?> GetByIdAsync(int id)
	{
		return await _wasteReportRepo.GetByIdAsync(id);
	}

	public async Task<WasteReport> CreateAsync(CreateWasteReportDto dto)
	{
		// Try to find citizen by CitizenId (PK) or UserId (fallback for frontend sends user.id)
		var citizen = await _citizenRepo.GetByIdAsync(dto.CitizenId);
		if (citizen == null)
		{
			citizen = await _citizenRepo.GetByUserIdAsync(dto.CitizenId);
		}

		if (citizen == null)
		{
			// Check if the provided ID is actually a valid User ID before auto-creating
			var user = await _userRepo.GetByIdAsync(dto.CitizenId);
			if (user != null)
			{
				citizen = new Citizen
				{
					UserId = user.UserId,
					TotalPoints = 0
				};
				await _citizenRepo.AddAsync(citizen);
				await _wasteReportRepo.SaveChangesAsync(); // Save to get the new CitizenId
			}
			else
			{
				throw new Exception($"Citizen not found for ID {dto.CitizenId}");
			}
		}

		if (!await _areaRepo.ExistsAsync(dto.AreaId))
		{
			throw new Exception("Area does not exist");
		}

		string? finalImageUrl = dto.ImageUrl;

		// Handle local file upload
		if (dto.ImageFile != null && dto.ImageFile.Length > 0)
		{
			finalImageUrl = await SaveImageAsync(dto.ImageFile);
		}
		else if (!string.IsNullOrEmpty(dto.ImageUrl) && !await IsValidImageUrlAsync(dto.ImageUrl))
		{
			throw new Exception("Invalid image source. Please provide a valid URL or upload a file.");
		}

		var wasteReport = new WasteReport
		{
			Description = dto.Description,
			CitizenId = citizen.CitizenId, // Use the correct CitizenId from DB
			AreaId = dto.AreaId,
			WasteType = dto.WasteType,
			CitizenLatitude = dto.Latitude,
			CitizenLongitude = dto.Longitude,
			CreatedAt = DateTime.UtcNow,
			Status = ReportStatus.Pending,
			ExpireTime = DateTime.UtcNow.AddHours(24),
			TeamId = null
		};

		await _wasteReportRepo.AddAsync(wasteReport);
		if (!string.IsNullOrEmpty(finalImageUrl))
		{
			var image = new ReportImage
			{
				Imageurl = finalImageUrl,
				Report = wasteReport
			};
			await _reportImageRepo.AddAsync(image);
		}
		await _wasteReportRepo.SaveChangesAsync();

		// Notify all Admins about the new report
		try
		{
			await _notificationService.SendNotificationToRoleAsync(
				UserRole.Admin,
				$"Báº¡n Ä‘ang cĂ³ 1 bĂ¡o cĂ¡o rĂ¡c má»›i tá»« ngÆ°á»i dĂ¢n (MĂ£: #{wasteReport.ReportId})",
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

		// Return the relative URL (Assuming the app runs on localhost:5000)
		return $"/uploads/{fileName}";
	}

	private async Task<bool> IsValidImageUrlAsync(string? url)
	{
		if (string.IsNullOrWhiteSpace(url))
		{
			return false;
		}

		if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
		{
			return false;
		}

		if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
		{
			return false;
		}

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Head, uri);
			using var response = await _httpClient.SendAsync(request);

			if (!response.IsSuccessStatusCode)
			{
				return false;
			}

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
		var report = await _wasteReportRepo.GetByIdAsync(reportId);
		if (report == null)
		{
			throw new Exception("Report not found");
		}

		if (report.Status != ReportStatus.Pending)
		{
			throw new Exception("Report must be pending to assign");
		}

		if (report.ExpireTime != null && report.ExpireTime < DateTime.UtcNow)
		{
			throw new Exception("Report expired");
		}

		var primaryTeam = await _teamRepo.GetTeamWithCollectorsAsync(report.AreaId, TeamType.Main);
		if (primaryTeam == null)
		{
			throw new Exception("Primary team not found");
		}

		var selectedTeam = primaryTeam;
		var noActiveCollector = !primaryTeam.Collectors.Any(c => c.Status == true);

		if (primaryTeam.CurrentTaskCount >= 5 || noActiveCollector)
		{
			var supportTeam = await _teamRepo.GetTeamWithCollectorsAsync(report.AreaId, TeamType.Support);

			if (supportTeam != null && supportTeam.Collectors.Any(c => c.Status == true))
			{
				selectedTeam = supportTeam;
			}
			else
			{
				throw new Exception("No available team to assign");
			}
		}

		report.TeamId = selectedTeam.TeamId;
		report.Status = ReportStatus.Assigned;
		selectedTeam.CurrentTaskCount++;

		await _teamRepo.UpdateAsync(selectedTeam);
		await _wasteReportRepo.UpdateAsync(report);
		await _wasteReportRepo.SaveChangesAsync();

		// Notify all Collectors in the assigned team
		try
		{
			await _notificationService.SendNotificationToTeamAsync(
				selectedTeam.TeamId,
				$"Báº¡n vá»«a Ä‘Æ°á»£c phĂ¢n cĂ´ng thu gom má»™t Ä‘Æ¡n bĂ¡o cĂ¡o rĂ¡c (MĂ£: #{report.ReportId})",
				report.ReportId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to send notification for assigned report {ReportId}", report.ReportId);
		}
	}
    public async Task CancelReportAsync(CancelReportRequestDto request)
    {
        var report = await _wasteReportRepo.GetByIdAsync(request.ReportId);
        if (report == null)
            throw new KeyNotFoundException("Report not found");

        // Ch? cancel report Pending
        if (report.Status != ReportStatus.Pending)
            throw new BusinessRuleException("Only reports in Pending status can be cancelled");

        // Ch? set Status = Cancelled, không thêm field CanceledReason
        report.Status = ReportStatus.Cancelled;

        await _wasteReportRepo.UpdateAsync(report);
        await _wasteReportRepo.SaveChangesAsync();

        // N?u mu?n, log lư do cancel
        if (!string.IsNullOrEmpty(request.Reason))
        {
            _logger.LogInformation("Admin cancelled report {ReportId}. Reason: {Reason}", request.ReportId, request.Reason);
        }
    }
    public async Task ProcessReportAsync(
		int reportId,
		int collectorId,
		bool isValid,
		string? collectorImageUrl,
		decimal? latitude,
		decimal? longitude)
	{
		var report = await _wasteReportRepo.GetByIdAsync(reportId);
		if (report == null)
		{
			throw new Exception("Report not found");
		}

		if (report.Status != ReportStatus.Assigned)
		{
			throw new Exception("Report is not assigned");
		}

		if (!report.TeamId.HasValue)
		{
			throw new Exception("Report has no assigned team");
		}

		var collector = await _collectorRepo.GetByIdAsync(collectorId);
		if (collector == null)
		{
			throw new Exception("Collector not found");
		}

		if (collector.TeamId != report.TeamId)
		{
			throw new Exception("You are not in the assigned team");
		}

		if (collector.Role != CollectorRole.Leader)
		{
			throw new Exception("Only team leader can submit completion report");
		}

		var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
		if (citizen == null)
		{
			throw new Exception("Citizen not found");
		}

		if (!string.IsNullOrEmpty(collectorImageUrl) && !await IsValidImageUrlAsync(collectorImageUrl))
		{
			throw new Exception("Invalid collector image URL.");
		}

		if (!string.IsNullOrWhiteSpace(collectorImageUrl))
		{
			var reportImage = new ReportImage
			{
				ReportId = report.ReportId,
				Imageurl = collectorImageUrl
			};

			await _reportImageRepo.AddAsync(reportImage);
		}

		report.CollectorLatitude = latitude;
		report.CollectorLongitude = longitude;

		if (isValid)
		{
			report.Status = ReportStatus.Completed;
			citizen.TotalPoints = (citizen.TotalPoints ?? 0) + 10;
            
            // Record PointHistory
            var pointLog = new PointHistory
            {
                CitizenId = citizen.CitizenId,
                PointAmount = 10,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.PointHistories.AddAsync(pointLog);
		}
		else
		{
			report.Status = ReportStatus.Cancelled;
			citizen.TotalPoints = (citizen.TotalPoints ?? 0) - 5;

			if (citizen.TotalPoints < 0)
			{
				citizen.TotalPoints = 0;
			}
            
            // Record PointHistory
            var pointLog = new PointHistory
            {
                CitizenId = citizen.CitizenId,
                PointAmount = -5,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.PointHistories.AddAsync(pointLog);
		}

		var team = await _teamRepo.GetByIdAsync(report.TeamId.Value);
		if (team != null && team.CurrentTaskCount > 0)
		{
			team.CurrentTaskCount--;
			await _teamRepo.UpdateAsync(team);
		}

		await _wasteReportRepo.UpdateAsync(report);
		await _citizenRepo.UpdateAsync(citizen);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task ConfirmReportAsync(int reportId, int collectorId)
	{
		var report = await _wasteReportRepo.GetByIdAsync(reportId);
		if (report == null)
		{
			throw new Exception("Report not found");
		}

		if (report.Status != ReportStatus.Assigned)
		{
			throw new Exception("Report must be in Assigned status to confirm");
		}

		var collector = await _collectorRepo.GetByIdAsync(collectorId);
		if (collector == null)
		{
			throw new Exception("Collector not found");
		}

		if (collector.TeamId != report.TeamId)
		{
			throw new Exception("You are not in the assigned team");
		}

		report.Status = ReportStatus.Processing;
		await _wasteReportRepo.UpdateAsync(report);
		await _wasteReportRepo.SaveChangesAsync();

		// Notify Admin and Citizen that the report is being processed
		try
		{
			await _notificationService.SendNotificationToRoleAsync(
				UserRole.Admin,
				$"BĂ¡o cĂ¡o rĂ¡c (MĂ£: #{report.ReportId}) Ä‘ang Ä‘Æ°á»£c xá»­ lĂ­ bá»Ÿi nhĂ¢n viĂªn thu gom",
				report.ReportId);

			// Get the citizen's UserId to notify them
			var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
			if (citizen != null)
			{
				await _notificationService.SendNotificationAsync(
					citizen.UserId,
					$"BĂ¡o cĂ¡o rĂ¡c cá»§a báº¡n (MĂ£: #{report.ReportId}) Ä‘ang Ä‘Æ°á»£c xá»­ lĂ­",
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
		var report = await _wasteReportRepo.GetByIdAsync(id);
		if (report == null)
		{
			return false;
		}

		if (report.Status != ReportStatus.Pending)
		{
			throw new Exception("Only pending reports can be deleted");
		}

		await _wasteReportRepo.DeleteAsync(report);
		await _wasteReportRepo.SaveChangesAsync();
		return true;
	}
}
