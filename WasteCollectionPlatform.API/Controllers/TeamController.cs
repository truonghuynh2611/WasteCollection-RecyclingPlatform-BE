using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.Enums;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
using WasteCollectionPlatform.Common.DTOs.Request.Collector;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.Exceptions;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Team management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly ITeamService _teamService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TeamController> _logger;

    public TeamController(
        ITeamService teamService,
        IUnitOfWork unitOfWork,
        ILogger<TeamController> logger)
    {
        _teamService = teamService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all teams
    /// </summary>
    /// <returns>List of all teams</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTeams()
    {
        try
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();
            
            var result = new List<object>();
            foreach (var t in teams)
            {
                var area = t.AreaId.HasValue ? await _unitOfWork.Areas.GetByIdAsync(t.AreaId.Value) : null;
                var collectors = await _teamService.GetCollectorsByTeamIdAsync(t.TeamId);
                result.Add(new
                {
                    teamId = t.TeamId,
                    name = t.Name,
                    areaId = t.AreaId,
                    areaName = area?.Name,
                    type = (int)t.Type,
                    collectors = collectors
                });
            }

            return Ok(ApiResponse<object>.SuccessResponse(result, "Teams retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get team by ID
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Team details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamById(int id)
    {
        try
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(id);
            
            if (team == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Team with ID {id} not found."));
            }

            var area = team.AreaId.HasValue ? await _unitOfWork.Areas.GetByIdAsync(team.AreaId.Value) : null;
            var collectors = await _teamService.GetCollectorsByTeamIdAsync(id);

            var teamDto = new
            {
                teamId = team.TeamId,
                name = team.Name,
                areaId = team.AreaId,
                areaName = area?.Name,
                type = (int)team.Type,
                collectors = collectors
            };

            return Ok(ApiResponse<object>.SuccessResponse(teamDto, "Team retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
    //[Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequestDto request)
    {
        try
        {
            var result = await _teamService.CreateTeamAsync(request);

            return StatusCode(201,
                ApiResponse<object>.SuccessResponse(result, "Tạo Team thành công"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                ApiResponse<object>.ErrorResponse(
                    ex.InnerException?.Message ?? ex.Message
                ));
        }

    }
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamRequestDto request)
    {
        try
        {
            await _teamService.UpdateTeamAsync(id, request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Team updated successfully"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    // ========================= DELETE =========================
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        try
        {
            await _teamService.DeleteTeamAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Team deleted successfully"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting team {TeamId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("create-collector")]
    public async Task<IActionResult> CreateCollector([FromBody] CreateCollectorRequestDto request)
    {
        try
        {
            var result = await _teamService.CreateCollectorAsync(request);
            return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Tạo Collector thành công"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating collector");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{teamId}/set-leader/{collectorId}")]
    public async Task<IActionResult> SetLeader(int teamId, int collectorId)
    {
        try
        {
            await _teamService.SetLeaderAsync(teamId, collectorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã thiết lập trưởng nhóm thành công"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting leader");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{teamId}/remove-leader/{collectorId}")]
    public async Task<IActionResult> RemoveLeader(int teamId, int collectorId)
    {
        try
        {
            await _teamService.RemoveLeaderAsync(teamId, collectorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã gỡ chức vụ trưởng nhóm thành công"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing leader");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("add-collector")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCollector([FromBody] AddCollectorToTeamRequestDto request)
    {
        try
        {
            await _teamService.AddCollectorToTeamAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Thêm Collector vào Team thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
    [HttpGet("{teamId}/collectors")]
    public async Task<IActionResult> GetCollectorsByTeam(int teamId)
    {
        try
        {
            var collectors = await _teamService.GetCollectorsByTeamIdAsync(teamId);
            return Ok(ApiResponse<object>.SuccessResponse(collectors, "Collectors retrieved successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving collectors for team {TeamId}", teamId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.InnerException?.Message ?? ex.Message));
        }
    }

    [HttpDelete("collector")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveCollector([FromBody] RemoveCollectorFromTeamRequestDto request)
    {
        try
        {
            await _teamService.RemoveCollectorFromTeamAsync(request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Collector removed from team successfully"));
        }
        catch (BusinessRuleException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing collector {CollectorId} from team {TeamId}", request.CollectorId, request.TeamId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("collectors")]
    public async Task<IActionResult> GetAllCollectors()
    {
        try
        {
            var collectors = await _teamService.GetAllCollectorsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(collectors, "Tất cả Collector đã được lấy thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all collectors");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{teamId}/assign-area/{areaId}")]
    public async Task<IActionResult> AssignTeamToArea(int teamId, int areaId)
    {
        try
        {
            await _teamService.AssignTeamToAreaAsync(teamId, areaId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Đã gán team vào khu vực thành công"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning team {TeamId} to area {AreaId}", teamId, areaId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("fix-db")]
    public async Task<IActionResult> FixDb()
    {
        try
        {
            var teams = (await _unitOfWork.Teams.GetAllAsync()).ToList();
            if (!teams.Any()) return BadRequest("No teams found. Please create some teams first.");

            var currentCollectors = await _unitOfWork.Collectors.GetAllAsync();
            int currentCount = currentCollectors.Count();
            int targetNew = 20;

            var random = new Random();
            for (int i = 0; i < targetNew; i++)
            {
                var email = $"collector_{Guid.NewGuid().ToString().Substring(0, 8)}@waste.com";
                var team = teams[i % teams.Count];
                
                var request = new CreateCollectorRequestDto
                {
                    FullName = $"Collector {currentCount + i + 1}",
                    Email = email,
                    Password = "Password123",
                    Phone = $"09{random.Next(10000000, 99999999)}",
                    TeamId = team.TeamId,
                    Role = CollectorRole.Member
                };

                await _teamService.CreateCollectorAsync(request);
            }

            // Ensure each team has a leader
            foreach (var team in teams)
            {
                var teamCollectors = await _teamService.GetCollectorsByTeamIdAsync(team.TeamId);
                if (!teamCollectors.Any(c => c.Role == "Leader"))
                {
                    var firstMember = teamCollectors.FirstOrDefault();
                    if (firstMember != null)
                    {
                        await _teamService.SetLeaderAsync(team.TeamId, firstMember.CollectorId);
                    }
                }
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, $"Successfully seeded {targetNew} collectors and verified leaders."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FixDb");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
