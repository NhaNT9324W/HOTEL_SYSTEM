namespace Hotel_System.DTOs
{
    /**
     * [F08 - View Dashboard]
     * DTO tổng hợp dữ liệu thống kê toàn cục dành riêng cho bảng điều khiển của Quản trị viên (Admin)[cite: 1].
     * Hỗ trợ hiển thị các chỉ số tổng quan về tài khoản, buồng phòng, đơn đặt phòng và sự cố hệ thống[cite: 1].
     */
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

    /**
     * [UC11 - View Dashboard]
     * DTO tổng hợp số liệu phân tích hiệu suất vận hành và tài chính dành cho Quản lý khách sạn (Hotel Manager)[cite: 1].
     * Cung cấp dữ liệu về công suất sử dụng phòng (occupancy rate), doanh thu thực tế thu được từ các giao dịch thanh toán hoàn tất[cite: 1].
     */
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

    /**
     * [UC13 - Manage Reservations]
     * DTO cung cấp thông tin màn hình trung tâm cho bộ phận Tiếp tân (Receptionist Dashboard Hub)[cite: 1].
     * Tập trung hiển thị các lượt khách dự kiến check-in/check-out trong ngày (arrivals/departures) và số lượng phòng trống thời gian thực[cite: 1].
     */
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

    /**
     * [UC19 - Manage Housekeeping]
     * DTO chứa dữ liệu tóm tắt công việc cá nhân dành riêng cho Nhân viên buồng phòng (Room Staff)[cite: 1].
     * Giúp nhân viên theo dõi nhanh tổng số tác vụ được giao, số lượng công việc theo từng trạng thái tiến độ (pending, in-progress)[cite: 1].
     */
    public class RoomStaffDashboardDto
    {
        public int TotalAssignedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasksToday { get; set; }
        public List<TaskSummaryDto> RecentTasks { get; set; } = new();
    }

    /**
     * [UC11 / UC13 - Dashboard Auxiliary Model]
     * DTO phụ trợ mô tả cấu trúc rút gọn của một đơn đặt phòng để hiển thị trực quan theo dạng danh sách trên bảng điều khiển[cite: 1].
     */
    public class RecentReservationDto
    {
        public int Id { get; set; }
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = null!;
    }

    /**
     * [UC19 - Manage Housekeeping Tasks]
     * DTO phụ trợ mô tả cấu trúc rút gọn của một tác vụ buồng phòng, hiển thị tóm tắt loại công việc, trạng thái tiến độ và độ ưu tiên[cite: 1].
     */
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