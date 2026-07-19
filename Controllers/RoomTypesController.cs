using Microsoft.AspNetCore.Mvc;
using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Hotel_System.Controllers
{
    /**
     * [UC9 - Manage Room Type]
     * Controller chịu trách nhiệm điều hướng và quản lý danh mục cấu hình các loại phòng/hạng phòng (ví dụ: Standard, Deluxe, VIP Suite) trong hệ thống[cite: 1].
     * Giới hạn quyền sử dụng toàn bộ các API trong lớp này cho vai trò Admin và Hotel Manager[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager")]
    [ApiController]
    [Route("api/roomtypes")]
    public class RoomTypesController : ControllerBase
    {
        private readonly IRoomTypeService _service;
        public RoomTypesController(IRoomTypeService service) => _service = service;

        /**
         * [UC9.4 - View List Room Type]
         * Lấy danh sách toàn bộ các loại phòng hiện đang được cấu hình trong hệ thống khách sạn[cite: 1].
         * 
         * @return Danh sách các loại phòng dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        /**
         * [UC9.5 - View Detail Room Type]
         * Xem thông tin chi tiết cấu hình và định mức của một loại phòng cụ thể dựa trên mã ID được chọn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (Primary Key) của hạng phòng cần truy vấn[cite: 1]
         * @return DTO thông tin chi tiết loại phòng (200 OK) hoặc trả về HTTP 404 NotFound nếu không tìm thấy[cite: 1]
         */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        /**
         * [UC9.1 - Create Room Type]
         * Thực hiện khởi tạo và thêm mới một hạng mục loại phòng vào hệ thống cơ sở dữ liệu khách sạn[cite: 1].
         * Áp dụng Business Rule kiểm tra ràng buộc: Tên loại phòng (Room Type Name) nhập vào phải là duy nhất, không được phép trùng lặp[cite: 1].
         * 
         * @param dto Đối tượng CreateRoomTypeDto chứa dữ liệu định mức hạng phòng gửi lên từ Request Body[cite: 1]
         * @return Đối tượng dữ liệu loại phòng vừa tạo thành công kèm HTTP trạng thái 201 Created[cite: 1]
         */
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomTypeDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /**
         * [UC9.2 - Edit Room Type]
         * Cập nhật các thông số định mức thay đổi mới (bao gồm giá cơ bản base price, sức chứa tối đa và trạng thái hiển thị) cho một loại phòng đang tồn tại theo mã ID[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của loại phòng cần thực hiện chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateRoomTypeDto chứa thông tin cập nhật hạng phòng từ Request Body[cite: 1]
         * @return HTTP trạng thái 204 NoContent nếu lưu thành công, hoặc trả về lỗi 404 NotFound nếu hạng phòng không tồn tại[cite: 1]
         */
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomTypeDto dto)
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }

        /**
         * [UC9.3 - Delete Room Type]
         * Thực hiện xóa mềm loại phòng khỏi danh mục áp dụng thực tế bằng phương thức SoftDeleteAsync[cite: 1].
         * Tuân thủ nghiêm ngặt bộ quy tắc kiểm soát Room Type Deletion Constraint: Hệ thống không xóa vĩnh viễn và chỉ chuyển trạng thái sang ngưng hoạt động (Inactive) nếu hạng phòng này đang liên kết với phòng thực tế hoặc lịch sử đặt phòng để đảm bảo tính toàn vẹn dữ liệu kế toán[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của loại phòng cần thực hiện xử lý xóa mềm[cite: 1]
         * @return HTTP trạng thái 204 NoContent nếu xử lý thành công, hoặc trả về lỗi 404 NotFound nếu không tìm thấy hạng phòng[cite: 1]
         */
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.SoftDeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}