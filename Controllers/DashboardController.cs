using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        // GET /api/dashboard/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _service.GetAdminDashboardAsync();
            return Ok(result);
        }

        // GET /api/dashboard/manager
        [HttpGet("manager")]
        [Authorize(Roles = "Admin,HotelManager")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var result = await _service.GetManagerDashboardAsync();
            return Ok(result);
        }

        // GET /api/dashboard/receptionist
        [HttpGet("receptionist")]
        [Authorize(Roles = "Admin,HotelManager,Receptionist")]
        public async Task<IActionResult> GetReceptionistDashboard()
        {
            var result = await _service.GetReceptionistDashboardAsync();
            return Ok(result);
        }

        // GET /api/dashboard/roomstaff
        [HttpGet("roomstaff")]
        [Authorize(Roles = "RoomStaff")]
        public async Task<IActionResult> GetRoomStaffDashboard()
        {
            var staffId = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _service.GetRoomStaffDashboardAsync(staffId);
            return Ok(result);
        }
    }
}