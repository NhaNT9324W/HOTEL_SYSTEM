using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Hotel_System.Controllers
{
    /**
     * [UC13 - Manage Reservations]
     * Controller quản lý toàn bộ quy trình đặt phòng, tra cứu tình trạng phòng trống và thực hiện thủ tục nhận phòng (Check-in) cho khách hàng[cite: 1].
     * Phân quyền hệ thống cho phép các vai trò: Admin, Hotel Manager và Receptionist phối hợp vận hành[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    [ApiController]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _service;

        public ReservationsController(IReservationService service) => _service = service;

        /**
         * [UC13.1 - View List Reservations]
         * Truy vấn và hiển thị danh sách toàn bộ các đơn đặt phòng (reservations) đã được ghi nhận trong hệ thống[cite: 1].
         * 
         * @return Danh sách các đơn đặt phòng dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/reservations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        /**
         * [UC13.4 - Search Reservation]
         * Tìm kiếm nhanh các đơn đặt phòng cụ thể dựa vào các từ khóa liên quan như Mã đặt phòng (Booking ID), Tên khách hàng (Guest Name) hoặc Số điện thoại (Phone Number)[cite: 1].
         * Nếu từ khóa trống, hệ thống tự động gọi hàm GetAll() để hiển thị toàn bộ danh sách[cite: 1].
         * 
         * @param keyword Từ khóa tìm kiếm do Tiếp tân nhập từ giao diện, truyền qua Query String[cite: 1]
         * @return Danh sách các đơn đặt phòng thỏa mãn điều kiện lọc kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/reservations/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var result = await _service.SearchAsync(keyword);
            return Ok(result);
        }

        /**
         * [UC13.2 - View Detail Reservation]
         * Truy vấn toàn bộ thông tin chi tiết của một đơn đặt phòng cụ thể (bao gồm thông tin khách lưu trú, phân bổ phòng assigned, các yêu cầu đặc biệt và trạng thái thanh toán)[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của đơn đặt phòng cần xem chi tiết[cite: 1]
         * @return Dữ liệu chi tiết đơn đặt phòng (200 OK) hoặc trả về thông báo lỗi 404 NotFound nếu không tìm thấy[cite: 1]
         */
        // GET /api/reservations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Reservation not found" });
            return Ok(result);
        }

        /**
         * [UC14 - Check Room Availability]
         * Hàm tiện ích cốt lõi dùng để truy vấn cơ sở dữ liệu kiểm tra và lọc ra danh sách các phòng còn trống (vacant) có thể đặt trong một khoảng thời gian nhất định[cite: 1].
         * Hàm áp dụng Business Rule kiểm tra: Ngày Checkout phải lớn hơn ngày Checkin[cite: 1]. Hệ thống tự động tính toán logic turnaround (phòng checkout ngày X có thể coi là trống để checkin ngày X)[cite: 1].
         * 
         * @param checkIn Ngày dự kiến nhận phòng của khách[cite: 1]
         * @param checkOut Ngày dự kiến trả phòng của khách[cite: 1]
         * @param roomTypeId Tùy chọn lọc nâng cao theo loại phòng (Standard, VIP, Family...), mặc định là null[cite: 1]
         * @return Danh sách phòng trống thỏa mãn điều kiện (200 OK) hoặc trả về lỗi 400 BadRequest nếu mốc thời gian không hợp lệ[cite: 1]
         */
        // GET /api/reservations/available-rooms?checkIn=...&checkOut=...
        [HttpGet("available-rooms")]
        public async Task<IActionResult> GetAvailableRooms(
    [FromQuery] DateTime checkIn,
    [FromQuery] DateTime checkOut,
    [FromQuery] int? roomTypeId = null)
        {
            if (checkIn >= checkOut)
                return BadRequest(new { message = "Check-out date must be after check-in date" });

            var result = await _service.GetAvailableRoomsAsync(checkIn, checkOut, roomTypeId);
            return Ok(result);
        }

        /**
         * [UC13.3 - Create Reservation]
         * Tiếp nhận yêu cầu lập đơn đặt phòng mới cho khách hàng (Walk-in trực tiếp hoặc qua điện thoại)[cite: 1].
         * Tầng dịch vụ sẽ áp dụng quy chế Strict Availability Enforcement nhằm ngăn chặn tuyệt đối tình trạng overbooking (đặt trùng phòng) trên cùng mốc thời gian[cite: 1].
         * 
         * @param dto Đối tượng CreateReservationDto chứa các mốc thời gian lưu trú, danh sách phòng lựa chọn và hồ sơ thông tin khách hàng[cite: 1]
         * @return Thông báo tạo đơn đặt phòng thành công (200 OK) hoặc phản hồi thông báo lỗi nghiệp vụ từ Exception (400 BadRequest)[cite: 1]
         */
        // POST /api/reservations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Reservation created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC13.5 - Edit Reservation]
         * Cho phép Tiếp tân chỉnh sửa thông tin của một đơn đặt phòng đang ở trạng thái 'Pending' hoặc 'Confirmed'[cite: 1].
         * Toàn bộ thay đổi về số đêm lưu trú hay nâng hạng loại phòng sẽ kích hoạt tính năng tính toán lại biểu phí chênh lệch tại Invoice Management module[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của đơn đặt phòng cần chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateReservationDto chứa các dữ liệu cập nhật mới gửi từ giao diện[cite: 1]
         * @return Thông báo cập nhật đơn đặt phòng thành công (200 OK) hoặc trả về thông báo lỗi nghiệp vụ phát sinh (400 BadRequest)[cite: 1]
         */
        // PUT /api/reservations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);
                return Ok(new { message = "Reservation updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC13.6 - Cancel Reservation]
         * Thực hiện hủy bỏ một đơn đặt phòng sắp diễn ra theo yêu cầu của khách hoặc theo chính sách quá giờ check-in (no-show)[cite: 1].
         * Hệ thống sẽ cập nhật trạng thái đơn sang 'Canceled' và áp dụng Inventory Release Policy để giải phóng block phòng trở lại trạng thái trống cho giai đoạn đó[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của đơn đặt phòng cần thực hiện hủy bỏ[cite: 1]
         * @return Thông báo hủy thành công (200 OK) hoặc báo lỗi nếu đơn đặt phòng không ở trạng thái hợp lệ để hủy (400 BadRequest)[cite: 1]
         */
        // PATCH /api/reservations/{id}/cancel
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _service.CancelAsync(id);
                return Ok(new { message = "Reservation cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC16 – Process Check-in]
         * Tiến hành thủ tục nhận phòng thực tế khi khách đến Front-desk[cite: 1]. 
         * Xử lý xác thực thông tin đơn đặt phòng, tự động cập nhật trạng thái đơn sang 'Checked-in' và chuyển đổi trạng thái buồng phòng liên quan sang 'Occupied' ngay lập tức[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của đơn đặt phòng tiến hành nhận phòng[cite: 1]
         * @return Thông báo xử lý Check-in thành công (200 OK) hoặc trả về thông báo lỗi chi tiết (400 BadRequest)[cite: 1]
         */
        // PATCH /api/reservations/{id}/checkin
        [HttpPatch("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            try
            {
                await _service.CheckInAsync(id);
                return Ok(new { message = "Check-in successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}