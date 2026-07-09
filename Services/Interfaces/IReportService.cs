using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IReportService
    {
        Task<OccupancyReportDto> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate);
        Task<RevenueReportDto> GetRevenueReportAsync(DateTime fromDate, DateTime toDate);
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime fromDate, DateTime toDate);
        Task<StaffPerformanceReportDto> GetStaffPerformanceReportAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportToExcelAsync(string reportType, DateTime fromDate, DateTime toDate);
    }
}