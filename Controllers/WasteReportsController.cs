using Microsoft.AspNetCore.Mvc;
using WasteReportApp.Models.Dto;
using WasteReportApp.Service;

namespace WasteReportApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WasteReportsController : ControllerBase
    {
        private readonly IWasteReportService _wasteReportService;

        public WasteReportsController(IWasteReportService wasteReportService)
        {
            _wasteReportService = wasteReportService;
        }

        // =========================================
        // 1️⃣ CREATE REPORT
        // =========================================
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateWasteReportDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _wasteReportService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.ReportId }, result);
        }

        // =========================================
        // 2️⃣ GET ALL REPORTS
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _wasteReportService.GetAllAsync();
            return Ok(reports);
        }

        // =========================================
        // 3️⃣ GET REPORT BY ID
        // =========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var report = await _wasteReportService.GetByIdAsync(id);

            if (report == null)
                return NotFound($"Report with ID {id} not found.");

            return Ok(report);
        }
        [HttpPost("assign/{id}")]
        public async Task<IActionResult> Assign(int id)
        {
            await _wasteReportService.AssignReportAsync(id);
            return Ok("Report assigned successfully");
        }

        // ===============================
        // PROCESS REPORT (Collector)
        // ===============================

        [HttpPost("process")]
        public async Task<IActionResult> Process(ProcessReportDto dto)
        {
            await _wasteReportService.ProcessReportAsync(
                dto.ReportId,
                dto.CollectorId,
                dto.IsValid,
                dto.CollectorImageUrl,
                dto.Latitude,
                dto.Longitude);

            return Ok("Report processed successfully");
        }


        //=========================================
        //5️⃣ DELETE REPORT
        //=========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _wasteReportService.DeleteAsync(id);

            if (!success)
                return NotFound($"Report with ID {id} not found.");

            return Ok("Report deleted successfully.");
        }
    }
}
