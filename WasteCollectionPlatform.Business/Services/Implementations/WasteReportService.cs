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
	private readonly IWasteReportRepository _wasteReportRepo;
	private readonly ICitizenRepository _citizenRepo;
	private readonly IAreaRepository _areaRepo;
	private readonly ITeamRepository _teamRepo;
	private readonly ICollectorRepository _collectorRepo;
	private readonly IReportImageRepository _reportImageRepo;
    private readonly ILogger<WasteReportService> _logger;
    private readonly HttpClient _httpClient;

	public WasteReportService(
		IWasteReportRepository wasteReportRepo,
		ICitizenRepository citizenRepo,
		IAreaRepository areaRepo,
		ITeamRepository teamRepo,
		ICollectorRepository collectorRepo,
		IReportImageRepository reportImageRepo,
         ILogger<WasteReportService> logger,

        HttpClient httpClient)
	{
		_wasteReportRepo = wasteReportRepo;
		_citizenRepo = citizenRepo;
		_areaRepo = areaRepo;
		_teamRepo = teamRepo;
		_collectorRepo = collectorRepo;
		_reportImageRepo = reportImageRepo;
		_logger = logger;
		_httpClient = httpClient;
	}

	public async Task<IEnumerable<WasteReport>> GetAllAsync()
	{
		return await _wasteReportRepo.GetAllAsync();
	}

	public async Task<WasteReport?> GetByIdAsync(int id)
	{
		return await _wasteReportRepo.GetByIdAsync(id);
	}

	public async Task<WasteReport> CreateAsync(CreateWasteReportDto dto)
	{
		if (!await _citizenRepo.ExistsAsync(c => c.CitizenId == dto.CitizenId))
		{
			throw new Exception("Citizen does not exist");
		}

		if (!await _areaRepo.ExistsAsync(dto.AreaId))
		{
			throw new Exception("Area does not exist");
		}

		// Handle image: either from uploaded file or from URL
		string? imageUrl = null;
		if (dto.ImageFile != null && dto.ImageFile.Length > 0)
		{
			var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reports");
			Directory.CreateDirectory(uploadsDir);
			var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
			var filePath = Path.Combine(uploadsDir, fileName);
			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await dto.ImageFile.CopyToAsync(stream);
			}
			imageUrl = $"/uploads/reports/{fileName}";
		}
		else if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
		{
			imageUrl = dto.ImageUrl;
		}

		var wasteReport = new WasteReport
		{
			Description = dto.Description,
			CitizenId = dto.CitizenId,
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

		if (!string.IsNullOrEmpty(imageUrl))
		{
			var image = new ReportImage
			{
				Imageurl = imageUrl,
				Report = wasteReport
			};
			await _reportImageRepo.AddAsync(image);
		}

		await _wasteReportRepo.SaveChangesAsync();

		return wasteReport;
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
	}
    public async Task CancelReportAsync(CancelReportRequestDto request)
    {
        var report = await _wasteReportRepo.GetByIdAsync(request.ReportId);
        if (report == null)
            throw new KeyNotFoundException("Report not found");

        // Ch? cancel report Pending
        if (report.Status != ReportStatus.Pending)
            throw new BusinessRuleException("Only reports in Pending status can be cancelled");

        // Ch? set Status = Cancelled, khÃ´ng thÃªm field CanceledReason
        report.Status = ReportStatus.Cancelled;

        await _wasteReportRepo.UpdateAsync(report);
        await _wasteReportRepo.SaveChangesAsync();

        // N?u mu?n, log lÃ½ do cancel
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
			citizen.TotalPoints += 10;
		}
		else
		{
			report.Status = ReportStatus.Cancelled;
			citizen.TotalPoints -= 5;

			if (citizen.TotalPoints < 0)
			{
				citizen.TotalPoints = 0;
			}
		}

		var team = await _teamRepo.GetByIdAsync(report.TeamId.Value);
		if (team != null && team.CurrentTaskCount > 0)
		{
			team.CurrentTaskCount--;
			await _teamRepo.UpdateAsync(team);
		}

		await _wasteReportRepo.UpdateAsync(report);
		await _citizenRepo.UpdateAsync(citizen);
		await _wasteReportRepo.SaveChangesAsync();
	}

	public async Task<IEnumerable<WasteReport>> GetByCitizenIdAsync(int citizenId)
	{
		return await _wasteReportRepo.GetByCitizenIdAsync(citizenId);
	}

public async Task<bool> DeleteAsync(int id)
	{
		var report = await _wasteReportRepo.GetByIdAsync(id);
		if (report == null)
		{
			return false;
		}

		await _wasteReportRepo.DeleteAsync(report);
		await _wasteReportRepo.SaveChangesAsync();
		return true;
	}
}

