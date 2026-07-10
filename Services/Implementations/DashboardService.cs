using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities.Enums;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context) => _context = context;

        // ===== ADMIN DASHBOARD =====
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
        public async Task<ManagerDashboardDto> GetManagerDashboardAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms
                .CountAsync(r => r.BookingStatus == RoomBookingStatus.OCCUPIED);

            // Revenue today
            var checkedOutToday = await _context.Invoices
                .Where(i => i.IssuedAt.Date == today)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            // Revenue this month
            var revenueThisMonth = await _context.Invoices
                .Where(i => i.IssuedAt >= firstDayOfMonth)
                .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;

            // Recent reservations
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
        public async Task<ReceptionistDashboardDto> GetReceptionistDashboardAsync()
        {
            var today = DateTime.Today;

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
                        && r.HousekeepingStatus == RoomHousekeepingStatus.READY),
                PendingReservations = await _context.Reservations
                    .CountAsync(r => r.Status == ReservationStatus.CONFIRMED),
                TodayArrivals = todayArrivals,
                TodayDepartures = todayDepartures
            };
        }

        // ===== ROOM STAFF DASHBOARD =====
        public async Task<RoomStaffDashboardDto> GetRoomStaffDashboardAsync(int staffId)
        {
            var today = DateTime.Today;

            var allTasks = await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Where(t => t.AssignedToId == staffId)
                .ToListAsync();

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
                        && t.CompletedAt.Value.Date == today),
                RecentTasks = recentTasks
            };
        }
    }
}