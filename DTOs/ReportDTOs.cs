namespace Hotel_System.DTOs
{
    public class ReportFilterDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class OccupancyReportDto
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public double OccupancyRate { get; set; }
        public List<OccupancyDetailDto> Details { get; set; } = new();
    }

    public class OccupancyDetailDto
    {
        public string RoomNumber { get; set; } = null!;
        public string RoomTypeName { get; set; } = null!;
        public int TotalNights { get; set; }
        public string Status { get; set; } = null!;
    }

    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalReservations { get; set; }
        public decimal AverageRevenuePerReservation { get; set; }
        public List<RevenueDetailDto> Details { get; set; } = new();
    }

    public class RevenueDetailDto
    {
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal Revenue { get; set; }
    }

    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public int TotalReservations { get; set; }
        public int CancelledReservations { get; set; }
        public List<FinancialDetailDto> Details { get; set; } = new();
    }

    public class FinancialDetailDto
    {
        public string RoomTypeName { get; set; } = null!;
        public int TotalReservations { get; set; }
        public decimal Revenue { get; set; }
    }

    public class StaffPerformanceReportDto
    {
        public int TotalStaff { get; set; }
        public int TotalTasksCompleted { get; set; }
        public List<StaffPerformanceDetailDto> Details { get; set; } = new();
    }

    public class StaffPerformanceDetailDto
    {
        public string StaffName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public double CompletionRate { get; set; }
    }
}