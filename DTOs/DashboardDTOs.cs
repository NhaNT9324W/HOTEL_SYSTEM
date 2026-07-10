namespace Hotel_System.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalAccounts { get; set; }
        public int ActiveAccounts { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int TotalReservations { get; set; }
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int PendingMaintenance { get; set; }
    }

    public class ManagerDashboardDto
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public double OccupancyRate { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public int PendingTasks { get; set; }
        public int PendingMaintenance { get; set; }
        public int TotalGuests { get; set; }
        public List<RecentReservationDto> RecentReservations { get; set; } = new();
    }

    public class ReceptionistDashboardDto
    {
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int CurrentGuests { get; set; }
        public int AvailableRooms { get; set; }
        public int PendingReservations { get; set; }
        public List<RecentReservationDto> TodayArrivals { get; set; } = new();
        public List<RecentReservationDto> TodayDepartures { get; set; } = new();
    }

    public class RoomStaffDashboardDto
    {
        public int TotalAssignedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasksToday { get; set; }
        public List<TaskSummaryDto> RecentTasks { get; set; } = new();
    }

    public class RecentReservationDto
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = null!;
    }

    public class TaskSummaryDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string TaskType { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? DueDate { get; set; }
    }
}