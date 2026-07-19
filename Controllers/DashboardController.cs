using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [F08 - View Dashboard]
     * Controller trung tâm chịu trách nhiệm phân phối dữ liệu tổng quan, báo cáo thống kê động cho các bảng điều khiển (Dashboard) dựa trên phân quyền vai trò của từng tài khoản nhân viên[cite: 1].
     */
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;

        public DashboardController(IDashboardService service) => _service = service;

        /**
         * [F08 - View Dashboard] (Admin Dashboard Overview)
         * Truy vấn và tổng hợp dữ liệu dành riêng cho vai trò Quản trị viên (Admin), bao gồm các số liệu về giám sát tài khoản hệ thống, cấu hình hoạt động và dấu vết kiểm toán (audit logs)[cite: 1].
         * 
         * @return Đối tượng chứa dữ liệu thống kê hệ thống toàn cục kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/dashboard/admin
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var result = await _service.GetAdminDashboardAsync();
            return Ok(result);
        }

        /**
         * [UC11 - View Dashboard]
         * Lấy thông tin tóm tắt hoạt động vận hành và kinh doanh hàng ngày dành cho Quản lý khách sạn (Hotel Manager), bao gồm tổng số lượt đặt phòng, công suất sử dụng phòng (occupancy rate), tóm tắt doanh thu thực tế và danh sách các phòng trống[cite: 1].
         * 
         * @return Đối tượng chứa số liệu phân tích hiệu suất và doanh thu kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/dashboard/manager
        [HttpGet("manager")]
        [Authorize(Roles = "Admin,HotelManager")]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var result = await _service.GetManagerDashboardAsync();
            return Ok(result);
        }

        /**
         * [UC13 - Manage Reservations] (Receptionist Dashboard Hub)
         * Cung cấp trung tâm dữ liệu giám sát phòng dành cho bộ phận Tiếp tân (Receptionist), làm nổi bật nhanh các lượt đặt phòng dự kiến check-in trong ngày, tình trạng phòng thời gian thực để hỗ trợ xử lý walk-ins hoặc đặt phòng qua điện thoại[cite: 1].
         * 
         * @return Đối tượng dữ liệu quản lý phòng và đặt phòng kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/dashboard/receptionist
        [HttpGet("receptionist")]
        [Authorize(Roles = "Admin,HotelManager,Receptionist")]
        public async Task<IActionResult> GetReceptionistDashboard()
        {
            var result = await _service.GetReceptionistDashboardAsync();
            return Ok(result);
        }

        /**
         * [UC19 - Manage Housekeeping] (Room Staff Dashboard Hub)
         * Truy vấn bảng điều khiển đơn giản hóa dành riêng cho Nhân viên buồng phòng (Room Staff)[cite: 1]. 
         * Hệ thống trích xuất thông tin mã định danh định danh tài khoản từ JWT token hiện tại để lọc chính xác danh sách các phòng và tác vụ dọn dẹp/bảo trì đang được gán trực tiếp cho nhân viên đó[cite: 1].
         * 
         * @return Đối tượng chứa danh sách phòng trống/bẩn và các nhiệm vụ dọn dẹp được chỉ định kèm HTTP trạng thái 200 OK[cite: 1]
         */
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