using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Hotel_System.Controllers
{
    /**
     * [UC15 – Manage Guest Profiles]
     * Controller điều hướng và xử lý các yêu cầu HTTP liên quan đến quản lý hồ sơ thông tin khách hàng[cite: 1].
     * Phân quyền truy cập hệ thống dành cho các vai trò: Admin, Hotel Manager và Receptionist[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    [ApiController]
    [Route("api/guests")]
    public class GuestsController : ControllerBase
    {
        private readonly IGuestService _service;
        public GuestsController(IGuestService service) => _service = service;

        /**
         * [UC15 – Manage Guest Profiles] (AF-1: Search guest trước hành động)
         * Lấy danh sách toàn bộ khách hàng hoặc tìm kiếm lọc nhanh danh sách theo từ khóa tên hoặc số điện thoại[cite: 1].
         * 
         * @param search Từ khóa tìm kiếm hồ sơ khách hàng truyền qua Query String, có thể null[cite: 1]
         * @return Danh sách hồ sơ khách hàng khớp bộ lọc kèm HTTP trạng thái 200 OK
         */
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
            => Ok(await _service.GetAllAsync(search));

        /**
         * [UC15.2 – View Guest Profile]
         * Truy vấn chi tiết toàn bộ hồ sơ thông tin đã lưu trữ của một khách hàng cụ thể dựa vào ID[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của hồ sơ khách hàng cần truy vấn[cite: 1]
         * @return Chi tiết hồ sơ thông tin khách hàng (200 OK) hoặc trả về lỗi HTTP 404 NotFound nếu không tồn tại[cite: 1]
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetDetailAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /**
         * [UC15.1 – Create Guest Information]
         * Thực hiện khởi tạo và đăng ký một hồ sơ thông tin khách hàng mới vào cơ sở dữ liệu[cite: 1].
         * Hệ thống bắt buộc phải cung cấp đầy đủ FullName và ít nhất một phương thức liên lạc Phone hoặc Email[cite: 1].
         * 
         * @param dto Đối tượng CreateGuestDto chứa thông tin cá nhân khách hàng gửi lên từ Request Body[cite: 1]
         * @return Thông báo tạo hồ sơ thành công (200 OK) hoặc thông báo lỗi ràng buộc nghiệp vụ đầu vào (400 BadRequest)[cite: 1]
         */
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGuestDto dto)
        {
            var (success, message) = await _service.CreateAsync(dto);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        /**
         * [UC15.3 – Edit Guest Profile]
         * Cập nhật các trường thông tin thay đổi mới cho hồ sơ khách hàng đang tồn tại dựa trên mã ID[cite: 1].
         * Quá trình lưu trữ sẽ kiểm tra và đưa ra cảnh báo nếu phát hiện trùng lặp thông tin liên lạc với một khách hàng khác[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của hồ sơ khách hàng cần thực hiện chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateGuestDto chứa thông tin cá nhân cập nhật mới từ Request Body[cite: 1]
         * @return Thông báo chỉnh sửa thành công (200 OK) hoặc thông báo lỗi xử lý nghiệp vụ từ tầng dịch vụ (400 BadRequest)[cite: 1]
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGuestDto dto)
        {
            var (success, message) = await _service.UpdateAsync(id, dto);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        /**
         * [UC15 – Manage Guest Profiles] (Xóa hồ sơ khách hàng)
         * Thực hiện xóa hồ sơ thông tin khách hàng ra khỏi danh sách hiển thị tác vụ chủ động[cite: 1].
         * Phương thức này áp dụng cơ chế soft delete (thiết lập flag IsDeleted = 1) nhằm duy trì tính toàn vẹn dữ liệu cho hóa đơn tài chính và lịch sử đặt phòng khứ hồi của khách sạn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của hồ sơ khách hàng cần xóa[cite: 1]
         * @return Thông báo xóa dữ liệu thành công (200 OK) hoặc thông báo lỗi phát sinh từ Exception (400 BadRequest)[cite: 1]
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _service.DeleteAsync(id);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}