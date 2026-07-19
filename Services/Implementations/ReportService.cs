using ClosedXML.Excel;
using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities.Enums;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.8 ReportService Implementation]
     * Lớp xử lý nghiệp vụ tổng hợp dữ liệu, thống kê hiệu năng vận hành và phân tích tài chính (UC21).
     * Tích hợp công cụ ClosedXML hỗ trợ kết xuất báo cáo định dạng Excel cấu hình động.
     */
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext để thực hiện các truy vấn gộp và thống kê nâng cao. */
        public ReportService(AppDbContext context) => _context = context;

        // ===== OCCUPANCY REPORT =====
        /** 
         * Thống kê công suất và mật độ sử dụng phòng vật lý (Occupancy Rate) trong một khoảng thời gian.
         * Thuật toán lọc bỏ các đơn đặt phòng đã hủy (CANCELED) và tính toán khoảng thời gian giao thoa lịch lưu trú.
         */
        public async Task<OccupancyReportDto> GetOccupancyReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var totalRooms = await _context.Rooms.CountAsync();

            var reservations = await _context.Reservations
                .Include(r => r.Room).ThenInclude(r => r!.RoomType)
                .Where(r => r.Status != ReservationStatus.CANCELED
                    && r.CheckInDate < toDate
                    && r.CheckOutDate > fromDate)
                .ToListAsync();

            var occupiedRoomIds = reservations.Select(r => r.RoomId).Distinct().Count();
            var occupancyRate = totalRooms > 0
                ? Math.Round((double)occupiedRoomIds / totalRooms * 100, 2)
                : 0;

            var details = reservations.Select(r => new OccupancyDetailDto
            {
                RoomNumber = r.Room?.RoomNumber ?? "",
                RoomTypeName = r.Room?.RoomType?.Name ?? "",
                TotalNights = (int)(r.CheckOutDate - r.CheckInDate).TotalDays,
                Status = r.Status.ToString()
            }).ToList();

            return new OccupancyReportDto
            {
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRoomIds,
                OccupancyRate = occupancyRate,
                Details = details
            };
        }

        // ===== REVENUE REPORT =====
        /** 
         * Báo cáo chi tiết doanh thu phòng dựa trên danh sách các đơn đặt phòng đã hoàn tất quy trình Checkout (CHECKED_OUT).
         * Hỗ trợ bộ phận Kế toán đối soát dòng tiền phòng thực tế phát sinh trong kỳ báo cáo.
         */
        public async Task<RevenueReportDto> GetRevenueReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room).ThenInclude(r => r!.RoomType)
                .Where(r => r.Status == ReservationStatus.CHECKED_OUT
                    && r.CheckOutDate >= fromDate
                    && r.CheckOutDate <= toDate)
                .ToListAsync();

            var details = reservations.Select(r =>
            {
                var nights = (int)(r.CheckOutDate - r.CheckInDate).TotalDays;
                var revenue = nights * (r.Room?.RoomType?.BasePrice ?? 0);
                return new RevenueDetailDto
                {
                    GuestName = r.Guest?.FullName ?? "",
                    RoomNumber = r.Room?.RoomNumber ?? "",
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Nights = nights,
                    Revenue = revenue
                };
            }).ToList();

            var totalRevenue = details.Sum(d => d.Revenue);

            return new RevenueReportDto
            {
                TotalRevenue = totalRevenue,
                TotalReservations = reservations.Count,
                AverageRevenuePerReservation = reservations.Count > 0
                    ? Math.Round(totalRevenue / reservations.Count, 0)
                    : 0,
                Details = details
            };
        }

        // ===== FINANCIAL REPORT =====
        /** 
         * Phân tích tài chính chuyên sâu, gom nhóm doanh thu (GroupBy) theo từng hạng phòng (Room Type).
         * Giúp nhà quản lý đánh giá hiệu quả kinh doanh của từng nhóm sản phẩm để điều chỉnh chiến lược giá.
         */
        public async Task<FinancialReportDto> GetFinancialReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Room).ThenInclude(r => r!.RoomType)
                .Where(r => r.CheckOutDate >= fromDate && r.CheckOutDate <= toDate)
                .ToListAsync();

            var checkedOut = reservations
                .Where(r => r.Status == ReservationStatus.CHECKED_OUT)
                .ToList();

            var details = checkedOut
                .GroupBy(r => r.Room?.RoomType?.Name ?? "Unknown")
                .Select(g =>
                {
                    var revenue = g.Sum(r =>
                        (int)(r.CheckOutDate - r.CheckInDate).TotalDays
                        * (r.Room?.RoomType?.BasePrice ?? 0));
                    return new FinancialDetailDto
                    {
                        RoomTypeName = g.Key,
                        TotalReservations = g.Count(),
                        Revenue = revenue
                    };
                }).ToList();

            var totalRevenue = details.Sum(d => d.Revenue);

            return new FinancialReportDto
            {
                TotalRevenue = totalRevenue,
                RoomRevenue = totalRevenue,
                TotalReservations = reservations.Count,
                CancelledReservations = reservations
                    .Count(r => r.Status == ReservationStatus.CANCELED),
                Details = details
            };
        }

        // ===== STAFF PERFORMANCE REPORT =====
        /** 
         * Thống kê báo cáo KPIs hiệu năng làm việc của nhân viên buồng phòng (Room Staff).
         * Tính toán tỷ lệ hoàn thành công việc (Completion Rate) dựa trên số lượng tác vụ dọn dẹp/bảo trì (UC19/UC20).
         */
        public async Task<StaffPerformanceReportDto> GetStaffPerformanceReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var staffList = await _context.Accounts
                .Where(a => a.Role == Role.RoomStaff && a.Status == AccountStatus.Active)
                .ToListAsync();

            var tasks = await _context.HousekeepingTasks
                .Where(t => t.CreatedAt >= fromDate && t.CreatedAt <= toDate)
                .ToListAsync();

            var details = staffList.Select(s =>
            {
                var staffTasks = tasks.Where(t => t.AssignedToId == s.Id).ToList();
                var completed = staffTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.Completed);
                var pending = staffTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.Pending);
                var inProgress = staffTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.InProgress);

                return new StaffPerformanceDetailDto
                {
                    StaffName = s.FullName,
                    Role = s.Role.ToString(),
                    TotalTasks = staffTasks.Count,
                    CompletedTasks = completed,
                    PendingTasks = pending,
                    InProgressTasks = inProgress,
                    CompletionRate = staffTasks.Count > 0
                        ? Math.Round((double)completed / staffTasks.Count * 100, 2)
                        : 0
                };
            }).ToList();

            return new StaffPerformanceReportDto
            {
                TotalStaff = staffList.Count,
                TotalTasksCompleted = tasks
                    .Count(t => t.Status == HousekeepingTaskStatus.Completed),
                Details = details
            };
        }

        // ===== EXPORT EXCEL =====
        /** 
         * Điểm tập trung điều hướng yêu cầu xuất dữ liệu Excel sang mảng byte stream.
         * Khởi tạo Workbook và gọi Helper tương ứng dựa trên tham số phân loại biểu mẫu (Report Type).
         */
        public async Task<byte[]> ExportToExcelAsync(
            string reportType, DateTime fromDate, DateTime toDate)
        {
            using var workbook = new XLWorkbook();

            switch (reportType.ToLower())
            {
                case "occupancy":
                    var occupancy = await GetOccupancyReportAsync(fromDate, toDate);
                    ExportOccupancy(workbook, occupancy, fromDate, toDate);
                    break;
                case "revenue":
                    var revenue = await GetRevenueReportAsync(fromDate, toDate);
                    ExportRevenue(workbook, revenue, fromDate, toDate);
                    break;
                case "financial":
                    var financial = await GetFinancialReportAsync(fromDate, toDate);
                    ExportFinancial(workbook, financial, fromDate, toDate);
                    break;
                case "staffperformance":
                    var staff = await GetStaffPerformanceReportAsync(fromDate, toDate);
                    ExportStaffPerformance(workbook, staff, fromDate, toDate);
                    break;
                default:
                    throw new Exception("Invalid report type");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ===== EXCEL HELPERS =====

        /** Tạo trang tính mật độ sử dụng phòng, đổ dữ liệu meta-data và format bảng màu DarkBlue chuyên nghiệp. */
        private void ExportOccupancy(XLWorkbook wb, OccupancyReportDto data,
            DateTime from, DateTime to)
        {
            var ws = wb.Worksheets.Add("Occupancy Report");

            // Meta Header
            ws.Cell(1, 1).Value = "OCCUPANCY REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Period: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
            ws.Cell(3, 1).Value = $"Total Rooms: {data.TotalRooms}";
            ws.Cell(4, 1).Value = $"Occupied Rooms: {data.OccupiedRooms}";
            ws.Cell(5, 1).Value = $"Occupancy Rate: {data.OccupancyRate}%";

            // Table Header Styling
            var headers = new[] { "Room Number", "Room Type", "Total Nights", "Status" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(7, i + 1).Value = headers[i];
                ws.Cell(7, i + 1).Style.Font.Bold = true;
                ws.Cell(7, i + 1).Style.Fill.BackgroundColor = XLColor.DarkBlue;
                ws.Cell(7, i + 1).Style.Font.FontColor = XLColor.White;
            }

            // Đổ dữ liệu chi tiết danh sách phòng
            for (int i = 0; i < data.Details.Count; i++)
            {
                var d = data.Details[i];
                ws.Cell(8 + i, 1).Value = d.RoomNumber;
                ws.Cell(8 + i, 2).Value = d.RoomTypeName;
                ws.Cell(8 + i, 3).Value = d.TotalNights;
                ws.Cell(8 + i, 4).Value = d.Status;
            }

            ws.Columns().AdjustToContents();
        }

        /** Tạo trang tính doanh thu chi tiết, hiển thị số liệu VNĐ kèm format màu DarkGreen kế toán. */
        private void ExportRevenue(XLWorkbook wb, RevenueReportDto data,
            DateTime from, DateTime to)
        {
            var ws = wb.Worksheets.Add("Revenue Report");

            ws.Cell(1, 1).Value = "REVENUE REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Period: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
            ws.Cell(3, 1).Value = $"Total Revenue: {data.TotalRevenue:N0} VND";
            ws.Cell(4, 1).Value = $"Total Reservations: {data.TotalReservations}";
            ws.Cell(5, 1).Value = $"Average Revenue/Reservation: {data.AverageRevenuePerReservation:N0} VND";

            var headers = new[] { "Guest Name", "Room", "Check-in", "Check-out", "Nights", "Revenue (VND)" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(7, i + 1).Value = headers[i];
                ws.Cell(7, i + 1).Style.Font.Bold = true;
                ws.Cell(7, i + 1).Style.Fill.BackgroundColor = XLColor.DarkGreen;
                ws.Cell(7, i + 1).Style.Font.FontColor = XLColor.White;
            }

            for (int i = 0; i < data.Details.Count; i++)
            {
                var d = data.Details[i];
                ws.Cell(8 + i, 1).Value = d.GuestName;
                ws.Cell(8 + i, 2).Value = d.RoomNumber;
                ws.Cell(8 + i, 3).Value = d.CheckInDate.ToString("dd/MM/yyyy");
                ws.Cell(8 + i, 4).Value = d.CheckOutDate.ToString("dd/MM/yyyy");
                ws.Cell(8 + i, 5).Value = d.Nights;
                ws.Cell(8 + i, 6).Value = (double)d.Revenue;
            }

            ws.Columns().AdjustToContents();
        }

        /** Tạo trang tính tổng kết tài chính phân tích nhóm sản phẩm, format bảng màu DarkOrange. */
        private void ExportFinancial(XLWorkbook wb, FinancialReportDto data,
            DateTime from, DateTime to)
        {
            var ws = wb.Worksheets.Add("Financial Report");

            ws.Cell(1, 1).Value = "FINANCIAL REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Period: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
            ws.Cell(3, 1).Value = $"Total Revenue: {data.TotalRevenue:N0} VND";
            ws.Cell(4, 1).Value = $"Total Reservations: {data.TotalReservations}";
            ws.Cell(5, 1).Value = $"Cancelled Reservations: {data.CancelledReservations}";

            var headers = new[] { "Room Type", "Total Reservations", "Revenue (VND)" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(7, i + 1).Value = headers[i];
                ws.Cell(7, i + 1).Style.Font.Bold = true;
                ws.Cell(7, i + 1).Style.Fill.BackgroundColor = XLColor.DarkOrange;
                ws.Cell(7, i + 1).Style.Font.FontColor = XLColor.White;
            }

            for (int i = 0; i < data.Details.Count; i++)
            {
                var d = data.Details[i];
                ws.Cell(8 + i, 1).Value = d.RoomTypeName;
                ws.Cell(8 + i, 2).Value = d.TotalReservations;
                ws.Cell(8 + i, 3).Value = (double)d.Revenue;
            }

            ws.Columns().AdjustToContents();
        }

        /** Tạo trang tính hiệu suất nhân sự buồng phòng phục vụ đánh giá nhân viên, format bảng màu DarkViolet. */
        private void ExportStaffPerformance(XLWorkbook wb, StaffPerformanceReportDto data,
            DateTime from, DateTime to)
        {
            var ws = wb.Worksheets.Add("Staff Performance");

            ws.Cell(1, 1).Value = "STAFF PERFORMANCE REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Period: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
            ws.Cell(3, 1).Value = $"Total Staff: {data.TotalStaff}";
            ws.Cell(4, 1).Value = $"Total Tasks Completed: {data.TotalTasksCompleted}";

            var headers = new[] {
                "Staff Name", "Role", "Total Tasks",
                "Completed", "Pending", "In Progress", "Completion Rate (%)"
            };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cell(6, i + 1).Value = headers[i];
                ws.Cell(6, i + 1).Style.Font.Bold = true;
                ws.Cell(6, i + 1).Style.Fill.BackgroundColor = XLColor.DarkViolet;
                ws.Cell(6, i + 1).Style.Font.FontColor = XLColor.White;
            }

            for (int i = 0; i < data.Details.Count; i++)
            {
                var d = data.Details[i];
                ws.Cell(7 + i, 1).Value = d.StaffName;
                ws.Cell(7 + i, 2).Value = d.Role;
                ws.Cell(7 + i, 3).Value = d.TotalTasks;
                ws.Cell(7 + i, 4).Value = d.CompletedTasks;
                ws.Cell(7 + i, 5).Value = d.PendingTasks;
                ws.Cell(7 + i, 6).Value = d.InProgressTasks;
                ws.Cell(7 + i, 7).Value = d.CompletionRate;
            }

            ws.Columns().AdjustToContents();
        }
    }
}