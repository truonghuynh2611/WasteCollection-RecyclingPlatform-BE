using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WasteCollectionPlatform.Business.Services.Interfaces;
using WasteCollectionPlatform.Common.DTOs.Response.Common;
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

        [HttpPut("{id}/collectors")]
        public async Task<IActionResult> AddCollector(int id, [FromBody] int collectorId)
        {
            try
            {
                await _teamService.AddCollectorToTeamAsync(id, collectorId);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Thêm nhân viên vào đội thành công"));
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
}
