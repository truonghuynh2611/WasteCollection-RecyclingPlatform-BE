using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
using WasteCollectionPlatform.Common.DTOs.Request.Team;
using WasteCollectionPlatform.Common.DTOs.Response.Team;
using WasteCollectionPlatform.Common.Exceptions;

namespace WasteCollectionPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamService.GetAllTeamsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(teams));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teams = await _teamService.GetAllTeamsAsync();
            var team = teams.FirstOrDefault(t => t.TeamId == id);
            if (team == null) return NotFound(ApiResponse<object>.ErrorResponse("Đội không tồn tại"));
            return Ok(ApiResponse<object>.SuccessResponse(team));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest request)
        {
            try
            {
                var result = await _teamService.CreateTeamAsync(request.Name, request.AreaId);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Tạo đội thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateTeamRequest request)
        {
            try
            {
                var result = await _teamService.UpdateTeamAsync(id, request.Name, request.AreaId);
                return Ok(ApiResponse<object>.SuccessResponse(result, "Cập nhật đội thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _teamService.DeleteTeamAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Xóa đội thành công"));
        }

        [HttpGet("collectors")]
        public async Task<IActionResult> GetAllCollectors()
        {
            var collectors = await _teamService.GetAllCollectorsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(collectors));
        }

        [HttpPost("create-collector")]
        public async Task<IActionResult> CreateCollector([FromBody] CreateCollectorDto request)
        {
            try
            {
                var result = await _teamService.CreateCollectorAsync(request);
                return StatusCode(201, ApiResponse<object>.SuccessResponse(result, "Tạo người thu gom thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPatch("collector/{collectorId}/toggle-status")]
        public async Task<IActionResult> ToggleCollectorStatus(int collectorId)
        {
            try
            {
                await _teamService.ToggleCollectorStatusAsync(collectorId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Cập nhật trạng thái thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("add-collector")]
        public async Task<IActionResult> AddCollector([FromBody] AddCollectorToTeamDto dto)
        {
            try
            {
                await _teamService.AddCollectorToTeamAsync(dto);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Thêm nhân viên vào đội thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("collector")]
        public async Task<IActionResult> RemoveCollector([FromBody] RemoveCollectorRequest request)
        {
            await _teamService.RemoveCollectorFromTeamAsync(request.TeamId, request.CollectorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Gỡ nhân viên khỏi đội thành công"));
        }

        [HttpPost("{teamId}/set-leader/{collectorId}")]
        public async Task<IActionResult> SetLeader(int teamId, int collectorId)
        {
            try
            {
                await _teamService.SetLeaderAsync(teamId, collectorId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Thiết lập trưởng nhóm thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("{teamId}/remove-leader/{collectorId}")]
        public async Task<IActionResult> RemoveLeader(int teamId, int collectorId)
        {
            await _teamService.RemoveLeaderAsync(teamId, collectorId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Gỡ trưởng nhóm thành công"));
        }

        [HttpPost("{teamId}/assign-area/{areaId}")]
        public async Task<IActionResult> AssignArea(int teamId, int areaId)
        {
            try
            {
                await _teamService.AssignTeamToAreaAsync(teamId, areaId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Gán khu vực cho đội thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("assign-report")]
        public async Task<IActionResult> AssignReport([FromBody] AssignReportRequest request)
        {
            try
            {
                await _teamService.AssignReportToTeamAsync(request.TeamId, request.ReportId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Gán báo cáo cho đội thành công"));
            }
            catch (BusinessRuleException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
        }
    }

    public class CreateTeamRequest
    {
        public string Name { get; set; } = null!;
        public int AreaId { get; set; }
    }

    public class AssignReportRequest
    {
        public int TeamId { get; set; }
        public int ReportId { get; set; }
    }

    public class RemoveCollectorRequest
    {
        public int TeamId { get; set; }
        public int CollectorId { get; set; }
    }
}
