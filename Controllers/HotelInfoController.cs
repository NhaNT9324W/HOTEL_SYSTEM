using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC06 - Edit Hotel Information]
     * Controller quản lý thông tin cấu hình chung của khách sạn (tên, địa chỉ, hotline, email, mô tả)[cite: 1].
     * Toàn bộ các API trong này đều được bảo vệ nghiêm ngặt, chỉ cho phép tài khoản có phân quyền Admin truy cập[cite: 1].
     */
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/hotelinfo")]
    public class HotelInfoController : ControllerBase
    {
        private readonly IHotelInfoService _service;

        public HotelInfoController(IHotelInfoService service) => _service = service;

        /**
         * [UC06 - Edit Hotel Information] (Bước đọc dữ liệu cấu hình)
         * Lấy thông tin cấu hình tổng thể hiện tại của khách sạn từ database để hiển thị lên form chỉnh sửa trên UI[cite: 1].
         * Bảng này được thiết kế theo dạng singleton/độc lập để phục vụ in ấn header/footer trên hóa đơn tài chính[cite: 1].
         * 
         * @return Đối tượng chứa thông tin khách sạn (HotelInfo DTO) nếu tìm thấy (200 OK), ngược lại trả về lỗi 404 NotFound[cite: 1]
         */
        // GET /api/hotelinfo
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var info = await _service.GetAsync();
            if (info == null) return NotFound(new { message = "Hotel information not found" });
            return Ok(info);
        }

        /**
         * [UC06 - Edit Hotel Information] (Bước cập nhật dữ liệu cấu hình)
         * Thực hiện cập nhật các thay đổi mới đối với thông tin cốt lõi của khách sạn (vận hành hệ thống)[cite: 1].
         * Tuân thủ các Business Rule nghiêm ngặt: Các trường bắt buộc gồm Tên khách sạn, Số điện thoại và Địa chỉ vật lý tuyệt đối không được để trống[cite: 1].
         * 
         * @param dto Đối tượng UpdateHotelInfoDto chứa thông tin chỉnh sửa mới được gửi lên từ Request Body[cite: 1]
         * @return Thông báo cập nhật dữ liệu thành công (200 OK), lỗi ModelState đầu vào hoặc lỗi Exception nghiệp vụ (400 BadRequest)[cite: 1]
         */
        // PUT /api/hotelinfo
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateHotelInfoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(dto);
                return Ok(new { message = "Hotel information updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}