using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC05 - Manage Account]
     * Controller chịu trách nhiệm điều hướng và xử lý các yêu cầu HTTP liên quan đến quản lý tài khoản người dùng[cite: 1].
     * Toàn bộ các API bên trong class này đều yêu cầu xác thực bằng cơ chế JWT token và giới hạn quyền cho Admin[cite: 1].
     */
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _service;

        public AccountsController(IAccountService service) => _service = service;

        /**
         * [UC5.4 - View List Account]
         * Lấy danh sách toàn bộ các tài khoản nhân viên và quản lý hiện có trong database của hệ thống[cite: 1].
         * 
         * @return Trả về danh sách tài khoản dưới dạng DTO kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/accounts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _service.GetAllAsync();
            return Ok(accounts);
        }

        /**
         * [UC5.5 - Search Account]
         * Tìm kiếm tài khoản người dùng dựa trên từ khóa như username, fullname, email hoặc trạng thái tài khoản[cite: 1].
         * Nếu từ khóa truyền vào rỗng, API sẽ tự động kích hoạt hàm GetAll() để lấy toàn bộ danh sách tài khoản nhằm tối ưu trải nghiệm UI[cite: 1].
         * 
         * @param keyword Từ khóa dùng để tìm kiếm, truyền qua Query String[cite: 1]
         * @return Danh sách các tài khoản khớp với từ khóa kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/accounts/search?keyword=...
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAll();

            var results = await _service.SearchAsync(keyword);
            return Ok(results);
        }

        /**
         * [UC5.6 - View Detail Account]
         * Xem thông tin chi tiết đầy đủ của một tài khoản cụ thể dựa vào ID được chọn[cite: 1].
         * 
         * @param id Mã định danh duy nhất (Primary Key) của tài khoản cần truy vấn trong hệ thống[cite: 1]
         * @return Thông tin chi tiết tài khoản nếu tồn tại (200 OK), ngược lại trả về thông báo lỗi kèm HTTP 404 NotFound[cite: 1]
         */
        // GET /api/accounts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _service.GetByIdAsync(id);
            if (account == null) return NotFound(new { message = "Account not found" });
            return Ok(account);
        }

        /**
         * [UC5.1 - Create Account]
         * Thực hiện tạo mới một tài khoản nhân viên (Hotel Manager, Receptionist, hoặc Room Staff)[cite: 1].
         * Hệ thống sẽ tự động xác thực định dạng dữ liệu đầu vào và kiểm tra tính duy nhất của Email/Username trong database trước khi lưu trữ[cite: 1].
         * 
         * @param dto Đối tượng chứa dữ liệu đầu vào phục vụ cho việc tạo tài khoản gửi lên từ Request Body[cite: 1]
         * @return Thông báo thành công nếu lưu thành công (200 OK), hoặc thông báo lỗi nghiệp vụ chi tiết nếu xảy ra Exception (400 BadRequest)[cite: 1]
         */
        // POST /api/accounts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.CreateAsync(dto);
                return Ok(new { message = "Account created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC5.2 - Edit Account]
         * Cập nhật thông tin của một tài khoản đang tồn tại trong hệ thống (bao gồm FullName, Phone, trạng thái, hoặc phân quyền vai trò)[cite: 1].
         * Tuân thủ quy tắc Business Rule hệ thống: Email không được thay đổi sau khi tài khoản đã được khởi tạo thành công[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của tài khoản cần thực hiện chỉnh sửa[cite: 1]
         * @param dto Đối tượng chứa thông tin cập nhật mới được gửi lên từ Request Body[cite: 1]
         * @return Thông báo cập nhật dữ liệu thành công (200 OK), hoặc lỗi định dạng dữ liệu/lỗi Exception (400 BadRequest)[cite: 1]
         */
        // PUT /api/accounts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { message = "Account updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC5.3 - Delete Account]
         * Thực hiện xóa tài khoản nhân viên khỏi hệ thống (áp dụng cơ chế soft delete nhằm bảo toàn dữ liệu ràng buộc liên quan đến audit logs và lịch sử tác vụ)[cite: 1].
         * Áp dụng Business Rule bảo vệ: Quản trị viên (Admin) không được phép tự xóa tài khoản của chính mình khi đang trong phiên đăng nhập[cite: 1].
         * 
         * @param id Mã định danh duy nhất (ID) của tài khoản cần xóa khỏi database[cite: 1]
         * @return Thông báo xử lý xóa thành công (200 OK) hoặc trả về thông báo lỗi nghiệp vụ phát sinh (400 BadRequest)[cite: 1]
         */
        // DELETE /api/accounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Account deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC5.4 - View List Account] (Hàm lọc mở rộng chuyên biệt)
         * Truy vấn nhanh danh sách tài khoản thuộc bộ phận nhân viên phục vụ phòng (Room Staff)[cite: 1].
         * Hàm này lọc danh sách tài khoản theo enum vai trò là RoomStaff nhằm mục đích hỗ trợ hiển thị dữ liệu phục vụ gán tác vụ dọn dẹp hoặc bảo trì phòng[cite: 1].
         * 
         * @return Danh sách dữ liệu tối giản gồm Id và FullName của toàn bộ nhân viên Room Staff kèm HTTP trạng thái 200 OK[cite: 1]
         */
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff()
        {
            var staffList = await _service.GetAllAsync();
            var result = staffList
                .Where(a => a.Role == Hotel_System.Entities.Enums.Role.RoomStaff)
                .Select(a => new { a.Id, a.FullName });
            return Ok(result);
        }
    }
}