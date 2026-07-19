using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC19 - Manage Housekeeping Tasks]
     * Controller chịu trách nhiệm quản lý toàn bộ các tác vụ buồng phòng, bao gồm dọn dẹp, bảo trì và kiểm tra trạng thái phòng[cite: 1].
     * Quyền truy cập được cấu hình linh hoạt cho bộ phận quản trị (Admin), điều hành (Hotel Manager) và nhân viên thực thi (Room Staff)[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager,RoomStaff")]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _service;

        public TasksController(ITaskService service) => _service = service;

        /**
         * [UC19 - Manage Housekeeping Tasks]
         * Lấy danh sách toàn bộ các tác vụ buồng phòng và bảo trì hiện có trong hệ thống khách sạn[cite: 1].
         * 
         * @return Danh sách tác vụ dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tasks = await _service.GetAllAsync();
            return Ok(tasks);
        }

        /**
         * [UC19 - Manage Housekeeping Tasks]
         * Tìm kiếm và lọc các tác vụ buồng phòng dựa trên từ khóa khớp một phần (như số phòng, loại tác vụ, hoặc mức độ ưu tiên)[cite: 1].
         * Nếu từ khóa trống, hệ thống sẽ tự động điều hướng gọi hàm GetAll() để hiển thị toàn bộ danh sách[cite: 1].
         * 
         * @param keyword Từ khóa dùng để tra cứu tác vụ, truyền qua Query String[cite: 1]
         * @return Danh sách tác vụ thỏa mãn bộ lọc kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/tasks/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var results = await _service.SearchAsync(keyword);
            return Ok(results);
        }

        /**
         * [UC19 - Manage Housekeeping Tasks]
         * Xem chi tiết một tác vụ buồng phòng cụ thể dựa vào mã ID[cite: 1].
         * 
         * @param id Mã định danh duy nhất (Primary Key) của tác vụ cần truy vấn[cite: 1]
         * @return Thông tin chi tiết tác vụ (200 OK) hoặc trả về thông báo lỗi kèm HTTP 404 NotFound nếu không tìm thấy[cite: 1]
         */
        // GET /api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var task = await _service.GetByIdAsync(id);
            if (task == null) return NotFound(new { message = "Task not found" });
            return Ok(task);
        }

        /**
         * [UC19.1 - View Assigned Tasks]
         * Truy vấn danh sách các tác vụ dọn dẹp hoặc bảo trì đang được phân công trực tiếp cho một nhân viên cụ thể dựa trên ID nhân viên[cite: 1].
         * Phục vụ hiển thị danh sách nhiệm vụ cá nhân cho giao diện di động/máy tính bảng của Room Staff[cite: 1].
         * 
         * @param staffId Mã định danh duy nhất (ID) của nhân viên buồng phòng cần lấy danh sách tác vụ[cite: 1]
         * @return Danh sách các tác vụ được gán cho nhân viên tương ứng kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/tasks/staff/{staffId}
        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaff(int staffId)
        {
            var tasks = await _service.GetByStaffIdAsync(staffId);
            return Ok(tasks);
        }

        /**
         * [UC19 - Manage Housekeeping Tasks] (Khởi tạo tác vụ)
         * Cho phép Quản lý (Hotel Manager) hoặc Admin khởi tạo một tác vụ buồng phòng mới (Cleaning, Maintenance, hoặc Inspection) và gán cho nhân viên[cite: 1].
         * 
         * @param dto Đối tượng CreateTaskDto chứa thông tin phòng, nhân viên được chỉ định, loại tác vụ và mức độ ưu tiên[cite: 1]
         * @return Thông báo tạo tác vụ thành công (200 OK) hoặc trả về thông báo lỗi nghiệp vụ đầu vào (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19 - Manage Housekeeping Tasks] (Cập nhật thông tin tác vụ)
         * Chỉnh sửa thông tin chi tiết cấu hình (như mô tả công việc, thay đổi người thực hiện, thời hạn do ngày dueDate) của một tác vụ đang tồn tại[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của tác vụ cần chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateTaskDto chứa thông tin điều chỉnh mới gửi từ Request Body[cite: 1]
         * @return Thông báo cập nhật thành công (200 OK) hoặc trả về thông báo lỗi phát sinh Exception (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19 - Manage Housekeeping Tasks] (Xóa tác vụ)
         * Xóa bỏ một tác vụ buồng phòng ra khỏi cơ sở dữ liệu hệ thống khách sạn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của tác vụ cần xóa[cite: 1]
         * @return Thông báo xóa tác vụ thành công (200 OK) hoặc trả về lỗi Exception (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19.3 - Update Task Status]
         * Cập nhật tiến độ xử lý của một tác vụ dọn dẹp/bảo trì được giao (chuyển đổi trạng thái theo luồng logic nghiêm ngặt: PENDING -> IN_PROGRESS -> COMPLETED)[cite: 1].
         * Quy trình này đảm bảo kiểm tra điều kiện Task Ownership Constraint để nhân viên chỉ có thể cập nhật tác vụ của chính mình[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của tác vụ cần cập nhật trạng thái tiến độ[cite: 1]
         * @param dto Đối tượng UpdateTaskStatusDto chứa trạng thái mới (enum) gửi từ giao diện Room Staff[cite: 1]
         * @return Thông báo cập nhật tiến độ thành công (200 OK) hoặc báo lỗi nếu vi phạm luồng chuyển đổi trạng thái (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19.2 - Update Room Status]
         * Cập nhật trạng thái vật lý thực tế của phòng khách sạn (ví dụ: chuyển đổi từ trạng thái bẩn 'Dirty' sang sạch 'Clean' hoặc sẵn sàng đón khách 'Ready')[cite: 1].
         * Tuân thủ quy tắc Room Staff Status Constraints: Nhân viên buồng phòng chỉ được phép thay đổi trạng thái vật lý/vận hành, tuyệt đối bị nghiêm cấm can thiệp vào trạng thái đặt phòng (như Occupied, Reserved)[cite: 1].
         * 
         * @param dto Đối tượng UpdateRoomStatusDto chứa mã phòng (RoomId) và trạng thái buồng phòng mới (HousekeepingStatus)[cite: 1]
         * @return Thông báo cập nhật trạng thái phòng thành công (200 OK) hoặc báo lỗi nếu sai quy tắc chuyển đổi (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19.4 - Report Room Maintenance Issues]
         * Tiếp nhận thông tin báo cáo về sự cố hỏng hóc thiết bị, vật tư phát sinh trong phòng do nhân viên phát hiện[cite: 1].
         * Hệ thống trích xuất thông tin người báo cáo (ReportedById) tự động từ JWT token, đồng thời áp dụng quy chế Automatic Status Locking: Nếu mức độ sự cố được đánh dấu là nghiêm trọng ('Critical'), trạng thái phòng sẽ bị khóa tự động sang 'Under Maintenance' để chặn Tiếp tân gán phòng cho khách mới[cite: 1].
         * 
         * @param dto Đối tượng ReportMaintenanceDto chứa số phòng, danh mục sự cố hỏng hóc, mô tả chi tiết và mức độ nghiêm trọng[cite: 1]
         * @return Thông báo lập phiếu sự cố bảo trì thành công (200 OK) hoặc trả về thông báo báo lỗi nghiệp vụ ràng buộc (400 BadRequest)[cite: 1]
         */
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

        /**
         * [UC19.1 - View Assigned Tasks] (Sự cố bảo trì theo nhân viên)
         * Lấy danh sách toàn bộ các sự cố hỏng hóc/bảo trì đang được phân công theo dõi hoặc xử lý cho một nhân viên kỹ thuật cụ thể[cite: 1].
         * 
         * @param staffId Mã định danh duy nhất (ID) của nhân viên kỹ thuật hoặc buồng phòng được gán trách nhiệm[cite: 1]
         * @return Danh sách sự cố bảo trì tương ứng kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/tasks/maintenance/staff/{staffId}
        [HttpGet("maintenance/staff/{staffId}")]
        public async Task<IActionResult> GetMaintenanceByStaff(int staffId)
        {
            var result = await _service.GetMaintenanceIssuesByStaffAsync(staffId);
            return Ok(result);
        }

        /**
         * [UC19 - Manage Housekeeping Tasks] (Xem toàn bộ sự cố)
         * Lấy danh sách tổng hợp toàn bộ các phiếu sự cố bảo trì hỏng hóc thiết bị đang có trên toàn hệ thống khách sạn để Admin/Manager tiện theo dõi và đôn đốc sửa chữa[cite: 1].
         * 
         * @return Danh sách tất cả các phiếu sự cố bảo trì (Maintenance Issues) kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/tasks/maintenance — Lấy tất cả maintenance issues
        [HttpGet("maintenance")]
        public async Task<IActionResult> GetAllMaintenance()
        {
            var result = await _service.GetAllMaintenanceIssuesAsync();
            return Ok(result);
        }

        /**
         * [UC19.3 - Update Task Status] (Cập nhật trạng thái sửa chữa)
         * Cho phép cập nhật tiến độ xử lý của phiếu sự cố bảo trì thiết bị trong phòng (ví dụ: chuyển đổi trạng thái từ PENDING sang RESOLVED sau khi thiết bị đã được sửa chữa hoặc thay mới)[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của phiếu sự cố bảo trì cần thực hiện cập nhật[cite: 1]
         * @param dto Đối tượng UpdateMaintenanceStatusDto chứa chuỗi trạng thái xử lý mới từ kỹ thuật viên[cite: 1]
         * @return Thông báo cập nhật trạng thái phiếu bảo trì thành công (200 OK) hoặc trả về thông báo lỗi Exception phát sinh (400 BadRequest)[cite: 1]
         */
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