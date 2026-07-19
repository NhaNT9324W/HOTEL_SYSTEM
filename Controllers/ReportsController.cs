using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC12 - Manage Hotel Report]
     * Controller chịu trách nhiệm quản lý hệ thống báo cáo, tiếp nhận bộ lọc thời gian và điều phối dữ liệu thống kê vận hành khách sạn[cite: 1].
     * Phân quyền hệ thống chỉ cho phép các vai trò: Admin và Hotel Manager thực hiện tác vụ[cite: 1].
     */
    [Authorize(Roles = "Admin,HotelManager")]
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportsController(IReportService service) => _service = service;

        /**
         * [UC12.1 – Generate Occupancy Report]
         * Khởi tạo báo cáo công suất phòng hiển thị các số liệu trực quan về tỷ lệ lấp đầy, số phòng trống và lượt phòng đang sử dụng[cite: 1].
         * 
         * @param filter Đối tượng ReportFilterDto chứa khoảng thời gian (FromDate, ToDate) cần kết xuất báo cáo[cite: 1]
         * @return Kết quả thống kê công suất sử dụng phòng kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // POST /api/reports/occupancy
        [HttpPost("occupancy")]
        public async Task<IActionResult> GetOccupancy([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetOccupancyReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /**
         * [UC12.2 – Generate Revenue Report]
         * Khởi tạo báo cáo doanh thu chi tiết từ các nguồn tiền phòng, dịch vụ đã hoàn tất giao dịch thanh toán hóa đơn[cite: 1].
         * Các đơn đặt phòng bị hủy hoặc chưa thanh toán sẽ tự động bị loại trừ khỏi phép tính toán tổng hợp[cite: 1].
         * 
         * @param filter Đối tượng ReportFilterDto chứa khoảng thời gian (FromDate, ToDate) cần kết xuất báo cáo[cite: 1]
         * @return Dữ liệu tổng hợp doanh thu theo mốc thời gian kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // POST /api/reports/revenue
        [HttpPost("revenue")]
        public async Task<IActionResult> GetRevenue([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetRevenueReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /**
         * [UC12.3 – Generate Financial Report]
         * Khởi tạo báo cáo tài chính tổng hợp chứa các số liệu về doanh thu, chi phí vận hành thực tế và lợi nhuận khách sạn[cite: 1].
         * Công thức áp dụng tự động tại tầng nghiệp vụ: Lợi nhuận thuần = Tổng doanh thu - Tổng chi phí[cite: 1].
         * 
         * @param filter Đối tượng ReportFilterDto chứa khoảng thời gian (FromDate, ToDate) cần kết xuất báo cáo[cite: 1]
         * @return Dữ liệu tóm tắt tình hình tài chính kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // POST /api/reports/financial
        [HttpPost("financial")]
        public async Task<IActionResult> GetFinancial([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetFinancialReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /**
         * [UC12.4 – Generate Staff Performance Report]
         * Khởi tạo báo cáo đánh giá hiệu suất xử lý công việc của nhân viên dựa trên số lượng tác vụ dọn dẹp, bảo trì buồng phòng đã hoàn thành[cite: 1].
         * 
         * @param filter Đối tượng ReportFilterDto chứa khoảng thời gian (FromDate, ToDate) cần kết xuất báo cáo[cite: 1]
         * @return Số liệu phân tích năng suất xử lý công việc của nhân viên kèm HTTP trạng thái 200 OK[cite: 1]
         */
        // POST /api/reports/staffperformance
        [HttpPost("staffperformance")]
        public async Task<IActionResult> GetStaffPerformance([FromBody] ReportFilterDto filter)
        {
            var result = await _service.GetStaffPerformanceReportAsync(filter.FromDate, filter.ToDate);
            return Ok(result);
        }

        /**
         * [UC12.5 – Export Reports]
         * Kết xuất dữ liệu báo cáo sang định dạng tệp tin bảng tính Excel (.xlsx) để người dùng tải trực tiếp về thiết bị[cite: 1].
         * Tên tệp tin tải về sẽ được tự động cấu hình theo định dạng chuẩn: [LoạiBáoCáo]_report_[TừNgày]_[ĐếnNgày].xlsx[cite: 1].
         * 
         * @param type Phân loại báo cáo cần thực hiện xuất file Excel (occupancy, revenue, financial, staffperformance)[cite: 1]
         * @param fromDate Ngày bắt đầu của chu kỳ lọc dữ liệu xuất file[cite: 1]
         * @param toDate Ngày kết thúc của chu kỳ lọc dữ liệu xuất file[cite: 1]
         * @return Luồng nhị phân chứa file Excel để trình duyệt kích hoạt hộp thoại tải về (200 OK), hoặc thông báo lỗi (400 BadRequest)[cite: 1]
         */
        // GET /api/reports/export?type=occupancy&fromDate=...&toDate=...
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] string type,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var bytes = await _service.ExportToExcelAsync(type, fromDate, toDate);
                var fileName = $"{type}_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}