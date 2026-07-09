using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [Authorize(Roles = "Admin,HotelManager,RoomStaff")]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        public TasksController(ITaskService service) => _service = service;

        // GET /api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _service.GetAllAsync();
            return Ok(tasks);
        }

        // GET /api/tasks/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var results = await _service.SearchAsync(keyword);
            return Ok(results);
        }

        // GET /api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _service.GetByIdAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });
            return Ok(task);
        }

        // GET /api/tasks/staff/{staffId}
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaff(int staffId)
        {
            var tasks = await _service.GetByStaffIdAsync(staffId);
            return Ok(tasks);
        }

        // POST /api/tasks
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Task created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { message = "Task updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Task deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tasks/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                dto.TaskId = id;
                await _service.UpdateTaskStatusAsync(id, dto.Status);
                return Ok(new { message = "Task status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PATCH /api/tasks/room-status
        [HttpPatch("room-status")]
        public async Task<IActionResult> UpdateRoomStatus([FromBody] UpdateRoomStatusDto dto)
        {
            try
            {
                await _service.UpdateRoomStatusAsync(dto.RoomId, dto.HousekeepingStatus);
                return Ok(new { message = "Room status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/tasks/maintenance
        [HttpPost("maintenance")]
        public async Task<IActionResult> ReportMaintenance([FromBody] ReportMaintenanceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.ReportedById = int.Parse(
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            try
            {
                await _service.ReportMaintenanceAsync(dto);
                return Ok(new { message = "Maintenance issue reported successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/tasks/maintenance/staff/{staffId}
        [HttpGet("maintenance/staff/{staffId}")]
        public async Task<IActionResult> GetMaintenanceByStaff(int staffId)
        {
            var result = await _service.GetMaintenanceIssuesByStaffAsync(staffId);
            return Ok(result);
        }

        // GET /api/tasks/maintenance — Lấy tất cả maintenance issues
        [HttpGet("maintenance")]
        public async Task<IActionResult> GetAllMaintenance()
        {
            var result = await _service.GetAllMaintenanceIssuesAsync();
            return Ok(result);
        }

        // PATCH /api/tasks/maintenance/{id}/status — Cập nhật status
        [HttpPatch("maintenance/{id}/status")]
        public async Task<IActionResult> UpdateMaintenanceStatus(
            int id, [FromBody] UpdateMaintenanceStatusDto dto)
        {
            try
            {
                await _service.UpdateMaintenanceStatusAsync(id, dto.Status);
                return Ok(new { message = "Maintenance status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}