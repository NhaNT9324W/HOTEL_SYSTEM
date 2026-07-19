using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC17, UC18 - Record Service Usage & Process Check-out]
     * Controller điều hướng các tác vụ liên quan đến folio dịch vụ của khách và quy trình thủ tục trả phòng (Check-out)[cite: 1].
     * Phân quyền truy cập hệ thống dành cho các vai trò: Admin, Hotel Manager và Receptionist[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager,Receptionist")]
    [ApiController]
    [Route("api/checkout")]
    public class CheckOutController : ControllerBase
    {
        private readonly ICheckOutService _service;

        public CheckOutController(ICheckOutService service) => _service = service;

        /**
         * [UC17 – Record Service Usage]
         * Truy vấn danh sách toàn bộ các bản ghi dịch vụ bổ sung (như minibar, giặt ủi, nhà hàng) mà khách đã sử dụng dựa trên mã đặt phòng[cite: 1].
         * 
         * @param reservationId Mã định danh của đơn đặt phòng (Reservation ID) cần kiểm tra[cite: 1]
         * @return Danh sách chi tiết các ServiceUsage records kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/checkout/{reservationId}/services
        [HttpGet("{reservationId}/services")]
        public async Task<IActionResult> GetServices(int reservationId)
        {
            var result = await _service.GetServiceUsagesAsync(reservationId);
            return Ok(result);
        }

        /**
         * [UC17 – Record Service Usage]
         * Thực hiện ghi nhận một lần sử dụng dịch vụ khách sạn mới cho khách đang lưu trú, hệ thống tự động cộng dồn chi phí vào thông tin folio hóa đơn[cite: 1].
         * 
         * @param dto Đối tượng AddServiceUsageDto chứa thông tin loại dịch vụ, số lượng và mã đặt phòng[cite: 1]
         * @return Thông báo thêm dịch vụ thành công (200 OK) hoặc trả về lỗi nghiệp vụ từ database (400 BadRequest)[cite: 1]
         */
        // POST /api/checkout/services
        [HttpPost("services")]
        public async Task<IActionResult> AddService([FromBody] AddServiceUsageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.AddServiceUsageAsync(dto);
                return Ok(new { message = "Service added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC17 – Record Service Usage]
         * Loại bỏ một bản ghi dịch vụ bổ sung đã ghi nhận (áp dụng trong trường hợp tiếp tân nhập sai thông tin trước khi tính tiền folio hóa đơn)[cite: 1].
         * 
         * @param id Mã định danh duy nhất của bản ghi ServiceUsage cần xóa khỏi database[cite: 1]
         * @return Thông báo xóa dịch vụ thành công (200 OK) hoặc thông báo lỗi Exception phát sinh (400 BadRequest)[cite: 1]
         */
        // DELETE /api/checkout/services/{id}
        [HttpDelete("services/{id}")]
        public async Task<IActionResult> RemoveService(int id)
        {
            try
            {
                await _service.RemoveServiceUsageAsync(id);
                return Ok(new { message = "Service removed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC18.1 / UC18.2 – Generate Invoice & Calculate Charges]
         * Tính toán chi phí hệ thống và xuất dữ liệu xem trước hóa đơn tổng hợp (gồm tiền phòng theo số đêm, tiền dịch vụ, thuế và phí) trước khi xử lý giao dịch thanh toán[cite: 1].
         * 
         * @param reservationId Mã định danh của đơn đặt phòng cần tạm tính chi phí hóa đơn[cite: 1]
         * @return Đối tượng chứa chi tiết hóa đơn nháp (Invoice Preview DTO) kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // GET /api/checkout/{reservationId}/preview
        [HttpGet("{reservationId}/preview")]
        public async Task<IActionResult> PreviewInvoice(int reservationId)
        {
            try
            {
                var result = await _service.PreviewInvoiceAsync(reservationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /**
         * [UC18 – Process Check-out]
         * Xác nhận hoàn tất quy trình giao dịch trả phòng thực tế cho khách lưu trú[cite: 1]. 
         * Hệ thống sẽ đóng đơn đặt phòng (chuyển trạng thái sang Checked-out), giải phóng phòng về trạng thái trống (Available/Dirty), và lưu hóa đơn tài chính chính thức vào database[cite: 1].
         * 
         * @param reservationId Mã định danh của đơn đặt phòng thực hiện Check-out[cite: 1]
         * @return Đối tượng Invoice chính thức đã lập và thông tin giao dịch hoàn tất thành công (200 OK)[cite: 1]
         */
        // POST /api/checkout/{reservationId}/confirm
        [HttpPost("{reservationId}/confirm")]
        public async Task<IActionResult> CheckOut(int reservationId)
        {
            try
            {
                var result = await _service.CheckOutAsync(reservationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}