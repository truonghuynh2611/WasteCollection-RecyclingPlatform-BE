using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.IO;
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
	private readonly IUserRepository _userRepo;
	private readonly IReportImageRepository _reportImageRepo;
	private readonly INotificationService _notificationService;
	private readonly HttpClient _httpClient;
	private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
    private readonly ILogger<WasteReportService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITeamService _teamService;


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
		IUnitOfWork unitOfWork,
		ITeamService teamService)

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
		_teamService = teamService;
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

		// Handle fallback single image if Items is empty or for backward compatibility
		string? imageUrl = await SaveReportImageAsync(dto.ImageFile);
		if (string.IsNullOrEmpty(imageUrl) && !string.IsNullOrWhiteSpace(dto.ImageUrl))
		{
			imageUrl = dto.ImageUrl;
		}

		// Handle multiple items or single ones
		var wasteReport = new WasteReport
		{
			// Summary for parent table: join all item descriptions
			Description = dto.Items.Count > 0 
                ? "Items: " + string.Join("; ", dto.Items.Select(i => i.Description).Where(s => !string.IsNullOrWhiteSpace(s))) 
                : dto.Description,
			// Summary for parent table: join all waste types
			WasteType = dto.Items.Count > 0 
                ? string.Join(", ", dto.Items.Select(i => i.WasteType).Where(s => !string.IsNullOrWhiteSpace(s))) 
                : dto.WasteType,
			CitizenId = citizen.CitizenId, // Use the correct CitizenId from DB
			AreaId = dto.AreaId,
			CreatedAt = DateTime.UtcNow,
			Status = ReportStatus.Pending,
			ExpireTime = DateTime.UtcNow.AddHours(24),
			TeamId = null
		};

		// Handle fallback single image if no items
		if (dto.Items.Count == 0 && !string.IsNullOrEmpty(imageUrl))
		{
			wasteReport.ReportImages.Add(new ReportImage
			{
				Imageurl = imageUrl,
				ImageType = "Citizen",
				Report = wasteReport
			});
		}

		// Handle many items
		if (dto.Items != null && dto.Items.Count > 0)
		{
			foreach (var item in dto.Items)
			{
				string? itemImageUrl = null;
				if (item.ImageFile != null && item.ImageFile.Length > 0)
				{
					itemImageUrl = await SaveReportImageAsync(item.ImageFile);
				}

				wasteReport.WasteReportItems.Add(new WasteReportItem
				{
					WasteType = item.WasteType ?? "Unknown",
					Description = item.Description,
					ImageUrl = itemImageUrl,
					Report = wasteReport
				});

				// Also add to ReportImages for general views
				if (!string.IsNullOrEmpty(itemImageUrl))
				{
					wasteReport.ReportImages.Add(new ReportImage
					{
						Imageurl = itemImageUrl,
						ImageType = "Citizen",
						Report = wasteReport
					});
				}
			}
		}

		await _wasteReportRepo.AddAsync(wasteReport);

		// Handle legacy/fallback single image
		if (!string.IsNullOrEmpty(imageUrl))
		{
			wasteReport.ReportImages.Add(new ReportImage
			{
				Imageurl = imageUrl,
				ImageType = "Citizen",
				Report = wasteReport
			});
		}

		await _wasteReportRepo.SaveChangesAsync();

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

	private async Task<string?> SaveReportImageAsync(IFormFile file)
	{
		if (file == null || file.Length == 0) return null;

		var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reports");
		Directory.CreateDirectory(uploadsDir);
		var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
		var filePath = Path.Combine(uploadsDir, fileName);
		using (var stream = new FileStream(filePath, FileMode.Create))
		{
			await file.CopyToAsync(stream);
		}
		return $"/uploads/reports/{fileName}";
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
        var report = await _wasteReportRepo.GetByIdAsync(request.ReportId);
        if (report == null)
            throw new KeyNotFoundException("Report not found");

        // Ch? cancel report Pending
        if (report.Status != ReportStatus.Pending)
            throw new BusinessRuleException("Only reports in Pending status can be cancelled");

        // Ch? set Status = Failed, khÃ´ng thÃªm field CanceledReason
        report.Status = ReportStatus.Failed;

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
		int userId,
		bool isValid,
		string? collectorImageUrl)
	{
		var report = await _wasteReportRepo.GetByIdAsync(reportId);
		if (report == null)
		{
			throw new Exception("Báo cáo không tồn tại.");
		}

		if (report.Status != ReportStatus.Assigned && report.Status != ReportStatus.OnTheWay)
		{
			throw new Exception("Báo cáo phải ở trạng thái Đang thực hiện mới có thể báo cáo hoàn thành.");
		}

		if (!report.TeamId.HasValue)
		{
			throw new Exception("Báo cáo này chưa được giao cho đội nào.");
		}

		var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
		if (collector == null)
		{
			throw new Exception("Tài khoản của bạn không được tìm thấy trong hệ thống nhân viên thu gom.");
		}

		if (collector.TeamId != report.TeamId)
		{
			throw new Exception($"Bạn không thuộc Đội #{report.TeamId} được phân công cho nhiệm vụ này.");
		}

		if (collector.Role != CollectorRole.Leader)
		{
			throw new Exception("Chỉ Đội trưởng (Leader) mới có quyền gửi báo cáo hoàn thành.");
		}

		var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
		if (citizen == null)
		{
			throw new Exception("Không tìm thấy thông tin công dân tạo báo cáo này.");
		}

		if (!string.IsNullOrEmpty(collectorImageUrl) && !await IsValidImageUrlAsync(collectorImageUrl))
		{
			throw new Exception("Đường dẫn ảnh bằng chứng không hợp lệ.");
		}

		if (!string.IsNullOrWhiteSpace(collectorImageUrl))
		{
			var reportImage = new ReportImage
			{
				ReportId = report.ReportId,
				Imageurl = collectorImageUrl,
				ImageType = "Collector"
			};

			await _reportImageRepo.AddAsync(reportImage);
		}


        // Retrieve dynamic point configurations from DB
        int pointsForCompleted = 10; // Default
        int pointsForCancelled = -5; // Default
        
        var completedConfig = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CompletedReport");
        if (completedConfig != null && int.TryParse(completedConfig.Value, out int parsedCompleted))
        {
            pointsForCompleted = parsedCompleted;
        }
        
        var cancelledConfig = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CancelledReport");
        if (cancelledConfig != null && int.TryParse(cancelledConfig.Value, out int parsedCancelled))
        {
            pointsForCancelled = parsedCancelled;
        }

        _logger.LogInformation("Config: Points_CompletedReport={Compl}, Points_CancelledReport={Canc}", pointsForCompleted, pointsForCancelled);
        
        // When a Leader processes a report, we move it directly to Collected status as requested (skipping Admin approval).
		if (isValid)
		{
            _logger.LogInformation("Report {ReportId} processed by Leader. Setting to Collected and awarding points.", report.ReportId);
			report.Status = ReportStatus.Collected;
            
            // Award points immediately
            citizen.TotalPoints = (citizen.TotalPoints ?? 0) + pointsForCompleted;

            // Record PointHistory
            await _unitOfWork.PointHistories.AddAsync(new PointHistory
            {
                CitizenId = citizen.CitizenId,
                ReportId = report.ReportId,
                PointAmount = pointsForCompleted,
                CreatedAt = DateTime.Now
            });

            // Notify Citizen
            await _notificationService.SendNotificationAsync(
                citizen.UserId,
                $"Chúc mừng! Báo cáo rác của bạn (#{report.ReportId}) đã được thu gom thành công. Bạn nhận được {pointsForCompleted} điểm.",
                report.ReportId);

            // Notify Admin
            await _notificationService.SendNotificationToRoleAsync(
                UserRole.Admin,
                $"Đội #{report.TeamId} đã hoàn tất nhiệm vụ #{report.ReportId}.",
                report.ReportId);
		}
		else
		{
			report.Status = ReportStatus.Failed;
			citizen.TotalPoints = (citizen.TotalPoints ?? 0) + pointsForCancelled; // pointsForCancelled is negative

			if (citizen.TotalPoints < 0)
			{
				citizen.TotalPoints = 0;
			}
            
            // Record PointHistory
            var pointLog = new PointHistory
            {
                CitizenId = citizen!.CitizenId,
                ReportId = report.ReportId,
                PointAmount = pointsForCancelled,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.PointHistories.AddAsync(pointLog);
		}

		var team = await _teamRepo.GetByIdAsync(report.TeamId.Value);
		if (team != null && !isValid && team.CurrentTaskCount > 0)
		{
			team.CurrentTaskCount--;
			await _teamRepo.UpdateAsync(team);
		}

		await _wasteReportRepo.UpdateAsync(report);
		await _citizenRepo.UpdateAsync(citizen);
		await _unitOfWork.SaveChangesAsync();
	}

	public async Task ConfirmReportAsync(int reportId, int userId)
	{
		var report = await _wasteReportRepo.GetByIdAsync(reportId);
		if (report == null)
		{
			throw new Exception("Report not found");
		}

		if (report.Status != ReportStatus.Assigned)
		{
			throw new Exception("Báo cáo phải ở trạng thái đã phân công mới có thể xác nhận.");
		}

		var collector = await _unitOfWork.Collectors.GetByUserIdAsync(userId);
		if (collector == null)
		{
			throw new Exception("Lỗi: Tài khoản của bạn không tồn tại trong danh sách nhân viên thu gom.");
		}

		if (report.TeamId == null || collector.TeamId == null || collector.TeamId != report.TeamId)
		{
            var msg = (report.TeamId == null) ? "Báo cáo này chưa được giao cho đội nào." 
                    : (collector.TeamId == null) ? "Tài khoản của bạn hiện chưa thuộc đội nào." 
                    : $"Bạn không thuộc Đội #{report.TeamId} được phân công cho nhiệm vụ này.";
			throw new Exception(msg);
		}

		report.Status = ReportStatus.OnTheWay;
		await _wasteReportRepo.UpdateAsync(report);
		await _wasteReportRepo.SaveChangesAsync();

		// Notify Admin and Citizen that the report is being processed
		try
		{
			await _notificationService.SendNotificationToRoleAsync(
				UserRole.Admin,
				$"Báo cáo rác (Mã: #{report.ReportId}) đang được xử lý bởi nhân viên thu gom",
				report.ReportId);

			// Get the citizen's UserId to notify them
			var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
			if (citizen != null)
			{
				await _notificationService.SendNotificationAsync(
					citizen.UserId,
					$"Báo cáo rác của bạn (Mã: #{report.ReportId}) đang được xử lý",
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

    public async Task UpdateReportStatusAsync(int reportId, ReportStatus newStatus)
    {
        var report = await _wasteReportRepo.GetByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found");

        // If status remains the same, do nothing
        if (report.Status == newStatus) return;

        // Special handling for Completed (Collected) or Failed
        if (newStatus == ReportStatus.Collected || newStatus == ReportStatus.Failed)
        {
            // Use existing ProcessReportAsync logic (or a private helper)
            await ProcessReportAsync(
                reportId, 
                0, // System update (collectorId not strictly validated if skip conditions met)
                newStatus == ReportStatus.Collected, 
                null);
        }
        else
        {
            report.Status = newStatus;
            await _wasteReportRepo.UpdateAsync(report);
            await _wasteReportRepo.SaveChangesAsync();
        }
    }

    public async Task<WasteReport> UpdateAsync(int id, UpdateWasteReportDto dto)
    {
        var report = await _wasteReportRepo.GetByIdAsync(id);
        if (report == null) throw new KeyNotFoundException("Report not found");

        if (report.Status != ReportStatus.Pending)
        {
            throw new BusinessRuleException("Only pending reports can be updated");
        }

        report.Description = dto.Description;
        report.WasteType = dto.WasteType;
        report.AreaId = dto.AreaId;

        await _wasteReportRepo.UpdateAsync(report);
        await _wasteReportRepo.SaveChangesAsync();
        return report;
    }

    public async Task ApproveAndAssignToMainTeamAsync(int reportId)
    {
        var report = await _wasteReportRepo.GetByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found");

        if (report.Status != ReportStatus.Pending)
            throw new BusinessRuleException("Only Pending reports can be approved");

        var mainTeam = await _teamRepo.GetTeamWithCollectorsAsync(report.AreaId, TeamType.Main);
        if (mainTeam == null)
            throw new BusinessRuleException("Khu vực này chưa được gán Đội Chính.");

        if (mainTeam.CurrentTaskCount >= 5)
            throw new BusinessRuleException($"Đội chính của khu vực này ({mainTeam.Name}) đã đạt giới hạn tối đa 5 báo cáo đang xử lý.");

        var collectors = await _teamService.GetCollectorsByTeamIdAsync(mainTeam.TeamId);
        var leader = collectors.FirstOrDefault(c => c.Role == "Leader");
        if (leader == null)
            throw new BusinessRuleException("Đội chính của khu vực này chưa có Trưởng nhóm.");

        report.Status = ReportStatus.Assigned;
        report.TeamId = mainTeam.TeamId;
        mainTeam.CurrentTaskCount += 1;
        
        await _wasteReportRepo.UpdateAsync(report);
        await _unitOfWork.Teams.UpdateAsync(mainTeam);
        await _unitOfWork.SaveChangesAsync();

        // Notify the entire Team (Leader + Members)
        var area = await _areaRepo.GetByIdAsync(report.AreaId);
        await _notificationService.SendNotificationToTeamAsync(
            mainTeam.TeamId,
            $"Nhiệm vụ mới #{report.ReportId} tại {area?.Name} đã được gán cho đội của bạn.",
            report.ReportId);

        // Notify Citizen
        var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
        if (citizen != null)
        {
            await _notificationService.SendNotificationAsync(
                citizen.UserId,
                $"Báo cáo rác của bạn (#{report.ReportId}) đã được phê duyệt và giao cho đội thu gom #{mainTeam.Name}.",
                report.ReportId);
        }
    }

    public async Task SubmitCompletionEvidenceAsync(int reportId, int leaderId, Microsoft.AspNetCore.Http.IFormFileCollection? imageFiles, List<string>? imageUrls, string? note)
    {
        var report = await _wasteReportRepo.GetByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found");

        if (report.Status != ReportStatus.Assigned && report.Status != ReportStatus.OnTheWay)
            throw new BusinessRuleException("Báo cáo không ở trạng thái có thể nộp bằng chứng.");

        var leader = await _collectorRepo.GetByIdAsync(leaderId);
        if (leader == null || leader.Role != CollectorRole.Leader || leader.TeamId != report.TeamId)
            throw new BusinessRuleException("Chỉ trưởng nhóm được giao nhiệm vụ này mới có thể nộp bằng chứng.");

        // Handle uploaded files
        if (imageFiles != null)
        {
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "completion");
            Directory.CreateDirectory(uploadsDir);
            foreach (var file in imageFiles)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                await _reportImageRepo.AddAsync(new ReportImage
                {
                    ReportId = reportId,
                    Imageurl = $"/uploads/completion/{fileName}",
                    ImageType = "Collector"
                });
            }
        }

        // Handle URL list
        if (imageUrls != null)
        {
            foreach (var url in imageUrls)
            {
                await _reportImageRepo.AddAsync(new ReportImage
                {
                    ReportId = reportId,
                    Imageurl = url,
                    ImageType = "Collector"
                });
            }
        }

        // Retrieve citizen and point configuration
        var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
        if (citizen == null) throw new KeyNotFoundException("Citizen not found for report.");

        int pointsForCompleted = 10; // Default points
        var completedConfig = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CompletedReport");
        if (completedConfig != null && int.TryParse(completedConfig.Value, out int pc))
            pointsForCompleted = pc;

        // SKIP ADMIN APPROVAL: SET TO COLLECTED DIRECTLY
        report.Status = ReportStatus.Collected;
        
        // Award points to the citizen immediately
        citizen.TotalPoints = (citizen.TotalPoints ?? 0) + pointsForCompleted;

        // Record point history
        await _unitOfWork.PointHistories.AddAsync(new PointHistory
        {
            CitizenId = citizen.CitizenId,
            ReportId = report.ReportId,
            PointAmount = pointsForCompleted,
            CreatedAt = DateTime.Now
        });

        await _wasteReportRepo.UpdateAsync(report);
        await _citizenRepo.UpdateAsync(citizen);
        await _unitOfWork.SaveChangesAsync();

        // Notify Citizen
        await _notificationService.SendNotificationAsync(
            citizen.UserId,
            $"Tin vui! Báo cáo rác của bạn (#{report.ReportId}) đã được thu gom thành công. Bạn nhận được {pointsForCompleted} điểm tích lũy.",
            report.ReportId);
        
        // Notify Admin for information
        await _notificationService.SendNotificationToRoleAsync(
            UserRole.Admin,
            $"Leader đã xác nhận hoàn tất nhiệm vụ #{report.ReportId}.",
            report.ReportId);
    }

    public async Task VerifyAndFinalizeReportAsync(int reportId, bool isApproved, string? adminNote)
    {
        var report = await _wasteReportRepo.GetByIdAsync(reportId);
        if (report == null) throw new KeyNotFoundException("Report not found");

        if (report.Status != ReportStatus.ReportedByTeam)
            throw new BusinessRuleException("Báo cáo không ở trạng thái chờ xác nhận.");

        if (isApproved)
        {
            var citizen = await _citizenRepo.GetByIdAsync(report.CitizenId);
            int points = 10;
            var config = await _unitOfWork.SystemConfigurations.GetByKeyAsync("Points_CompletedReport");
            if (config != null && int.TryParse(config.Value, out int parsed)) points = parsed;

            report.Status = ReportStatus.Collected;
            citizen.TotalPoints = (citizen.TotalPoints ?? 0) + points;

            await _unitOfWork.PointHistories.AddAsync(new PointHistory
            {
                CitizenId = citizen.CitizenId,
                ReportId = report.ReportId,
                PointAmount = points,
                CreatedAt = DateTime.Now
            });

            // Notify Citizen
            await _notificationService.SendNotificationAsync(
                citizen.UserId,
                $"Tin vui! Báo cáo rác của bạn (#{report.ReportId}) đã được thu gom thành công. Bạn nhận được {points} điểm tích lũy.",
                report.ReportId);
        }
        else
        {
            report.Status = ReportStatus.Assigned;
            var team = await _teamRepo.GetByIdAsync(report.TeamId!.Value);
            var collectors = await _teamService.GetCollectorsByTeamIdAsync(team.TeamId);
            var leader = collectors.FirstOrDefault(c => c.Role == "Leader");
            if (leader != null)
            {
                await _notificationService.SendNotificationAsync(
                    leader.UserId,
                    $"Báo cáo hoàn thành cho nhiệm vụ #{reportId} bị từ chối. Lý do: {adminNote}. Vui lòng xử lý lại.",
                    reportId);
            }
        }

        await _wasteReportRepo.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }
}

