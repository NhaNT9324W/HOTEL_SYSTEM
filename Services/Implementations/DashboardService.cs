using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities.Enums;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.4 DashboardService Implementation]
     * Lớp xử lý nghiệp vụ tổng hợp số liệu thống kê (Dashboard) thời gian thực.
     * Cung cấp các chỉ số vận hành và kinh doanh cốt lõi được cá nhân hóa theo từng vai trò (Role) trong khách sạn.
     */
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext để thực hiện các truy vấn đếm và tổng hợp dữ liệu. */
        public DashboardService(AppDbContext context) => _context = context;

        // ===== ADMIN DASHBOARD =====
        /** 
         * Tổng hợp số liệu hệ thống dành cho Quản trị viên (UC05).
         * Tập trung vào giám sát tài khoản nhân sự, tổng lượng tài nguyên phòng và tình trạng bảo trì tổng thể.
         */
        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            var today = DateTime.Today;

            return new AdminDashboardDto
            {
                TotalAccounts = await _context.Accounts.CountAsync(),
                ActiveAccounts = await _context.Accounts
                    .CountAsync(a => a.Status == AccountStatus.Active),
                TotalRooms = await _context.Rooms.CountAsync(),
                AvailableRooms = await _context.Rooms
                    .CountAsync(r => r.BookingStatus == RoomBookingStatus.AVAILABLE),
                TotalReservations = await _context.Reservations.CountAsync(),
                TodayCheckIns = await _context.Reservations
                    .CountAsync(r => r.CheckInDate.Date == today
                        && r.Status == ReservationStatus.CHECKED_IN),
                TodayCheckOuts = await _context.Reservations
                    .CountAsync(r => r.CheckOutDate.Date == today
                        && r.Status == ReservationStatus.CHECKED_OUT),
                PendingMaintenance = await _context.MaintenanceIssues
                    .CountAsync(m => m.Status == "PENDING")
            };
        }

        // ===== MANAGER DASHBOARD =====
        /** 
         * Tổng hợp dữ liệu kinh doanh cho Quản lý (UC21 - Management Reporting).
         * Tính toán công suất sử dụng phòng (Occupancy Rate), tổng hợp doanh thu trong ngày/trong tháng và thống kê công việc tồn đọng.
         */
        public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms
                .CountAsync(r => r.BookingStatus == RoomBookingStatus.OCCUPIED);

            // Doanh thu tính từ các hóa đơn đã xuất trong ngày hôm nay
            var checkedOutToday = await _context.Invoices
                .Where(i => i.IssuedAt.Date == today)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            // Doanh thu lũy kế tính từ đầu tháng đến thời điểm hiện tại
            var revenueThisMonth = await _context.Invoices
                .Where(i => i.IssuedAt >= firstDayOfMonth)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            // Lấy danh sách 5 đơn đặt phòng mới nhất vừa phát sinh trên hệ thống
            var recentReservations = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new RecentReservationDto
                {
                    Id = r.Id,
                    GuestName = r.Guest!.FullName,
                    RoomNumber = r.Room!.RoomNumber,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            return new ManagerDashboardDto
            {
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                AvailableRooms = totalRooms - occupiedRooms,
                OccupancyRate = totalRooms > 0
                    ? Math.Round((double)occupiedRooms / totalRooms * 100, 1)
                    : 0,
                RevenueToday = checkedOutToday,
                RevenueThisMonth = revenueThisMonth,
                PendingTasks = await _context.HousekeepingTasks
                    .CountAsync(t => t.Status == HousekeepingTaskStatus.Pending),
                PendingMaintenance = await _context.MaintenanceIssues
                    .CountAsync(m => m.Status == "PENDING"),
                TotalGuests = await _context.Guests
                    .CountAsync(g => !g.IsDeleted),
                RecentReservations = recentReservations
            };
        }

        // ===== RECEPTIONIST DASHBOARD =====
        /** 
         * Tổng hợp số liệu vận hành Tiền sảnh trong ngày phục vụ Lễ tân (UC13/UC14).
         * Hỗ trợ theo dõi danh sách khách sắp đến (Arrivals), khách sắp đi (Departures) và số lượng phòng sẵn sàng đón khách (AVAILABLE & READY).
         */
        public async Task<ReceptionistDashboardDto> GetReceptionistDashboardAsync()
        {
            var today = DateTime.Today;

            // Danh sách khách hàng có lịch nhận phòng hoặc đã nhận phòng trong ngày
            var todayArrivals = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Where(r => r.CheckInDate.Date == today
                    && (r.Status == ReservationStatus.CONFIRMED
                        || r.Status == ReservationStatus.CHECKED_IN))
                .Select(r => new RecentReservationDto
                {
                    Id = r.Id,
                    GuestName = r.Guest!.FullName,
                    RoomNumber = r.Room!.RoomNumber,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            // Danh sách khách hàng đang lưu trú và có lịch trả phòng trong ngày hôm nay
            var todayDepartures = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Where(r => r.CheckOutDate.Date == today
                    && r.Status == ReservationStatus.CHECKED_IN)
                .Select(r => new RecentReservationDto
                {
                    Id = r.Id,
                    GuestName = r.Guest!.FullName,
                    RoomNumber = r.Room!.RoomNumber,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status.ToString()
                })
                .ToListAsync();

            return new ReceptionistDashboardDto
            {
                TodayCheckIns = todayArrivals.Count,
                TodayCheckOuts = todayDepartures.Count,
                CurrentGuests = await _context.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.CHECKED_IN),
                AvailableRooms = await _context.Rooms
                    .CountAsync(r => r.BookingStatus == RoomBookingStatus.AVAILABLE
                        && r.HousekeepingStatus == RoomHousekeepingStatus.READY), // Chỉ đếm phòng trống đã dọn sạch
                PendingReservations = await _context.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.CONFIRMED),
                TodayArrivals = todayArrivals,
                TodayDepartures = todayDepartures
            };
        }

        // ===== ROOM STAFF DASHBOARD =====
        /** 
         * Tổng hợp danh sách và tiến độ công việc được phân gán riêng cho từng nhân viên buồng phòng (UC19.1).
         * Thống kê số lượng việc chưa làm, đang làm và tổng hợp các công việc đã hoàn thành trong ngày.
         */
        public async Task<RoomStaffDashboardDto> GetRoomStaffDashboardAsync(int staffId)
        {
            var today = DateTime.Today;

            var allTasks = await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Where(t => t.AssignedToId == staffId)
                .ToListAsync();

            // Lấy 5 tác vụ mới được giao gần đây nhất để hiển thị nhanh trên Mobile/Giao diện thực địa
            var recentTasks = allTasks
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .Select(t => new TaskSummaryDto
                {
                    Id = t.Id,
                    RoomNumber = t.Room?.RoomNumber ?? "",
                    TaskType = t.TaskType.ToString(),
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    DueDate = t.DueDate
                })
                .ToList();

            return new RoomStaffDashboardDto
            {
                TotalAssignedTasks = allTasks.Count,
                PendingTasks = allTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.Pending),
                InProgressTasks = allTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.InProgress),
                CompletedTasksToday = allTasks
                    .Count(t => t.Status == HousekeepingTaskStatus.Completed
                        && t.CompletedAt.HasValue
                        && t.CompletedAt.Value.Date == today), // Kiểm toán KPIs hoàn thành trong ngày
                RecentTasks = recentTasks
            };
        }
    }
}