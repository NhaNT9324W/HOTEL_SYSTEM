using Microsoft.AspNetCore.Mvc;
using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Hotel_System.Controllers
{
    /**
     * [UC07 - Manage Room]
     * Controller chịu trách nhiệm quản lý cấu hình các phòng vật lý trong hệ thống khách sạn[cite: 1].
     * Giới hạn quyền sử dụng toàn bộ các API trong này cho vai trò Admin và Hotel Manager[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager")]
    [ApiController]
    [Route("api/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _service;
        public RoomsController(IRoomService service) => _service = service;

        /**
         * [UC7.3 - View List Room]
         * Lấy danh sách toàn bộ các phòng hiện có trong cơ sở dữ liệu hệ thống[cite: 1].
         * 
         * @return Danh sách các phòng dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        /**
         * [UC7.5 - View Detail Room]
         * Xem thông tin chi tiết đầy đủ của một phòng cụ thể dựa trên mã ID được chọn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (Primary Key) của phòng cần truy vấn[cite: 1]
         * @return DTO thông tin phòng chi tiết (200 OK) hoặc trả về HTTP 404 NotFound nếu không tồn tại[cite: 1]
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /**
         * [UC7.1 - Create Room]
         * Thực hiện tạo mới một phòng vật lý và thêm vào danh mục quản lý của khách sạn[cite: 1].
         * Áp dụng Business Rule xác thực: Số phòng (Room Number) nhập vào phải là duy nhất, giá phòng và sức chứa phải lớn hơn 0[cite: 1].
         * 
         * @param dto Đối tượng CreateRoomDto chứa dữ liệu khởi tạo phòng gửi lên từ Request Body[cite: 1]
         * @return Đối tượng dữ liệu phòng vừa tạo thành công (201 Created) hoặc trả về thông báo lỗi ràng buộc (400 BadRequest)[cite: 1]
         */
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
        {
            var (success, error, data) = await _service.CreateAsync(dto);
            if (!success) return BadRequest(new { message = error });
            return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
        }

        /**
         * [UC7.2 - Edit Room]
         * Cập nhật các trường thông tin thay đổi mới cho phòng đang tồn tại dựa trên mã ID phòng[cite: 1].
         * Cho phép chỉnh sửa số phòng, loại phòng, số tầng, sức chứa, mô tả hoặc trạng thái vật lý của phòng[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của phòng cần thực hiện chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateRoomDto chứa thông tin thay đổi cập nhật mới từ Request Body[cite: 1]
         * @return HTTP trạng thái 204 NoContent nếu lưu thành công, hoặc trả về thông báo lỗi nghiệp vụ từ tầng dịch vụ (400 BadRequest)[cite: 1]
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto dto)
        {
            var (success, error) = await _service.UpdateAsync(id, dto);
            if (!success) return BadRequest(new { message = error });
            return NoContent();
        }

        /**
         * [UC7.6 - Delete Room]
         * Thực hiện xóa vĩnh viễn thông tin một phòng ra khỏi hệ thống khách sạn[cite: 1].
         * Áp dụng Business Rule bảo vệ: Phòng đang gắn liền với các đơn đặt phòng hoạt động (Active Reservations) tuyệt đối không được phép xóa[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của phòng cần thực hiện xóa vĩnh viễn[cite: 1]
         * @return HTTP trạng thái 204 NoContent nếu xóa thành công, hoặc trả về lỗi 404 NotFound nếu phòng không tồn tại[cite: 1]
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }

        /**
         * [UC07 - Manage Room] (Hàm lọc danh sách thu gọn)
         * Truy vấn nhanh và trích xuất danh sách phòng tối giản (chỉ bao gồm Id, RoomNumber, RoomTypeName, Floor)[cite: 1].
         * API này hỗ trợ cung cấp nguồn dữ liệu sạch để hiển thị lên các cấu phần Dropdown/Selection trên UI tiếp tân hoặc phân gán tác vụ buồng phòng[cite: 1].
         * 
         * @return Danh sách dữ liệu phòng thu gọn phục vụ chọn lựa nhanh kèm HTTP trạng thái 200 OK[cite: 1]
         */
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var rooms = await _service.GetAllAsync();
            var result = rooms.Select(r => new
            {
                r.Id,
                r.RoomNumber,
                r.RoomTypeName,
                r.Floor
            });
            return Ok(result);
        }
    }
}