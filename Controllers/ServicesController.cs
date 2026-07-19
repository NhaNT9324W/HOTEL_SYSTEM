using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC10 – Manage Hotel Services]
     * Controller điều hướng và xử lý các yêu cầu HTTP liên quan đến quản lý danh mục dịch vụ của khách sạn (như Spa, Ăn sáng, Giặt ủi...)[cite: 1].
     * Giới hạn quyền truy cập hệ thống dành riêng cho các vai trò: Admin và Hotel Manager[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager")]
    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceManager _service;

        public ServicesController(IServiceManager service) => _service = service;

        /**
         * [UC10.1 – View List Services]
         * Lấy danh sách toàn bộ các dịch vụ khách sạn hiện có lưu trữ trong hệ thống cơ sở dữ liệu[cite: 1].
         * 
         * @return Danh sách các dịch vụ khách sạn dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/services
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _service.GetAllAsync();
            return Ok(services);
        }

        /**
         * [UC10.5 – Search Service]
         * Tìm kiếm nhanh các dịch vụ khách sạn dựa vào từ khóa lọc theo tên dịch vụ, danh mục hoặc trạng thái[cite: 1].
         * Nếu từ khóa truyền vào trống, hệ thống tự động chuyển hướng gọi hàm GetAll() để lấy toàn bộ danh sách[cite: 1].
         * 
         * @param keyword Từ khóa dùng để tìm kiếm dịch vụ, truyền qua Query String[cite: 1]
         * @return Danh sách các dịch vụ thỏa mãn điều kiện khớp một phần từ khóa kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/services/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var results = await _service.SearchAsync(keyword);
            return Ok(results);
        }

        /**
         * [UC10.2 – View Detail Service]
         * Xem thông tin chi tiết đầy đủ của một dịch vụ khách sạn cụ thể dựa trên mã hạng mục ID được chọn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (Primary Key) của dịch vụ cần truy vấn[cite: 1]
         * @return DTO chứa thông tin chi tiết dịch vụ (200 OK) hoặc trả về thông báo lỗi kèm HTTP 404 NotFound[cite: 1]
         */
        // GET /api/services/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _service.GetByIdAsync(id);
            if (service == null) return NotFound(new { message = "Service not found" });
            return Ok(service);
        }

        /**
         * [UC10.3 – Create Service]
         * Thực hiện khởi tạo và thêm mới một dịch vụ khách sạn vào hệ thống cơ sở dữ liệu[cite: 1].
         * Áp dụng Business Rule xác thực nghiệp vụ: Tên dịch vụ phải là duy nhất và giá dịch vụ phải là một giá trị số dương strictly greater than 0[cite: 1].
         * 
         * @param dto Đối tượng CreateServiceDto chứa các thông tin cấu hình dịch vụ mới gửi lên từ Request Body[cite: 1]
         * @return Thông báo tạo dịch vụ thành công (200 OK) hoặc trả về thông báo lỗi định dạng/lỗi Exception (400 BadRequest)[cite: 1]
         */
        // POST /api/services
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Service created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC10.4 – Edit Service]
         * Cập nhật các thông tin thay đổi mới (tên, mô tả, biểu phí, trạng thái) cho một dịch vụ đang tồn tại theo mã ID[cite: 1].
         * Tuân thủ quy tắc hệ thống: Việc thay đổi biểu phí dịch vụ sẽ không làm ảnh hưởng hồi tố đến các hóa đơn cũ đã sử dụng dịch vụ này trước đó[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của dịch vụ khách sạn cần thực hiện chỉnh sửa[cite: 1]
         * @param dto Đối tượng UpdateServiceDto chứa thông tin điều chỉnh dịch vụ từ Request Body[cite: 1]
         * @return Thông báo cập nhật thành công (200 OK) hoặc trả về lỗi không hợp lệ dữ liệu/Exception (400 BadRequest)[cite: 1]
         */
        // PUT /api/services/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { message = "Service updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC10.6 – Delete Service]
         * Thực hiện xóa bỏ hoặc ngưng áp dụng một dịch vụ khách sạn ra khỏi hệ thống danh mục hoạt động[cite: 1].
         * Áp dụng bộ lọc ràng buộc Active Service Protection: Hệ thống sử dụng cơ chế soft delete và từ chối xóa nếu dịch vụ này đang được liên kết hoạt động với các đơn đặt phòng chưa hoàn tất thanh toán hóa đơn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của dịch vụ khách sạn cần xử lý xóa mềm[cite: 1]
         * @return Thông báo xử lý xóa thành công (200 OK) hoặc thông báo lỗi từ chối/lỗi Exception phát sinh (400 BadRequest)[cite: 1]
         */
        // DELETE /api/services/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Service deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}