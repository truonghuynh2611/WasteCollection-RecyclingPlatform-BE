using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Request.Admin;
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
            
            var teamDtos = teams.Select(t => new
            {
                teamId = t.TeamId,
                name = t.Name,
                areaId = t.AreaId
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Teams retrieved successfully.",
                Data = teamDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teams");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving teams.",
                Errors = new List<string> { ex.Message }
            });
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
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Team with ID {id} not found.",
                    Errors = new List<string> { "Team not found." }
                });
            }

            var teamDto = new
            {
                teamId = team.TeamId,
                name = team.Name,
                areaId = team.AreaId
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Team retrieved successfully.",
                Data = teamDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving team {TeamId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving team.",
                Errors = new List<string> { ex.Message }
            });
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
                ApiResponse<object>.SuccessResponse(result, "T?o Team thành công"));
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
}
