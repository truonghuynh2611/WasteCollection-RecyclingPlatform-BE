using Microsoft.AspNetCore.Mvc;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.DataAccess.Repositories.Interfaces;

namespace WasteCollectionPlatform.API.Controllers;

/// <summary>
/// Team management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TeamController> _logger;

    public TeamController(
        IUnitOfWork unitOfWork,
        ILogger<TeamController> logger)
    {
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
                areaId = t.Areaid
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
                areaId = team.Areaid
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
}
