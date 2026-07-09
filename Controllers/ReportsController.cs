using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [Authorize(Roles = "Admin,HotelManager")]
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service) => _service = service;

        // POST /api/reports/occupancy
        [HttpPost("occupancy")]
        public async Task<IActionResult> GetOccupancy([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetOccupancyReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        // POST /api/reports/revenue
        [HttpPost("revenue")]
        public async Task<IActionResult> GetRevenue([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetRevenueReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        // POST /api/reports/financial
        [HttpPost("financial")]
        public async Task<IActionResult> GetFinancial([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetFinancialReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        // POST /api/reports/staffperformance
        [HttpPost("staffperformance")]
        public async Task<IActionResult> GetStaffPerformance([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetStaffPerformanceReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        // GET /api/reports/export?type=occupancy&fromDate=...&toDate=...
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] string type,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var bytes = await _service.ExportToExcelAsync(type, fromDate, toDate);
                var fileName = $"{type}_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}