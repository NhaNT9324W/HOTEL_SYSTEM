using Hotel_System.DTOs;
using System;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.8.I IReportService Interface]
     * Giao diện quy định các phương thức tổng hợp dữ liệu báo cáo, thống kê hiệu năng vận hành và phân tích tài chính.
     * Làm hợp đồng cho ReportService thực thi, phục vụ phân hệ Báo cáo quản trị (UC21) của bộ phận Quản lý và Kế toán.
     */
    public interface IReportService
    {
        /** Thống kê công suất và mật độ sử dụng phòng vật lý (Occupancy Rate) thực tế trong kỳ báo cáo (UC21). */
        Task<OccupancyReportDto> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate);

        /** Thống kê chi tiết doanh thu phòng phát sinh từ các đơn đặt phòng đã hoàn tất quyết toán trả phòng (UC21). */
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate);

        /** Phân tích tài chính chuyên sâu, gom nhóm doanh thu thực tế theo từng phân loại hạng phòng (UC21). */
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime fromDate, DateTime toDate);

        /** Thống kê chỉ số KPIs hiệu năng và tỷ lệ hoàn thành công việc của đội ngũ nhân sự buồng phòng (UC19/UC20/UC21). */
        Task<StaffPerformanceReportDto> GetStaffPerformanceReportAsync(DateTime fromDate, DateTime toDate);

        /** Cung cấp luồng xuất dữ liệu báo cáo động ra tệp định dạng Excel dưới dạng mảng byte stream (UC21). */
        Task<byte[]> ExportToExcelAsync(string reportType, DateTime fromDate, DateTime toDate);
    }
}