namespace Hotel_System.DTOs
{
    /**
     * [UC12 - Manage Hotel Report]
     * DTO tiếp nhận bộ lọc phạm vi thời gian (Từ ngày - Đến ngày) từ giao diện để thực hiện truy vấn kết xuất dữ liệu báo cáo thống kê[cite: 1].
     */
    public class ReportFilterDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    /**
     * [UC12.1 – Generate Occupancy Report]
     * DTO tổng hợp dữ liệu báo cáo công suất phòng khách sạn trong một giai đoạn được lựa chọn[cite: 1].
     * Chứa các chỉ số về tỷ lệ lấp đầy tổng thể và danh sách chi tiết trạng thái từng phòng[cite: 1].
     */
    public class OccupancyReportDto
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public double OccupancyRate { get; set; }
        public List<OccupancyDetailDto> Details { get; set; } = new();
    }

    /**
     * [UC12.1 – Generate Occupancy Report]
     * DTO phụ trợ mô tả chi tiết thông số vận hành của một phòng vật lý cụ thể trong kỳ báo cáo (số đêm phòng được thuê thực tế, số phòng, hạng phòng)[cite: 1].
     */
    public class OccupancyDetailDto
    {
        public string RoomNumber { get; set; } = null!;
        public string RoomTypeName { get; set; } = null!;
        public int TotalNights { get; set; }
        public string Status { get; set; } = null!;
    }

    /**
     * [UC12.2 – Generate Revenue Report]
     * DTO tổng hợp số liệu báo cáo doanh thu thu được từ phòng và dịch vụ dựa trên các giao dịch thanh toán hóa đơn đã hoàn thành[cite: 1].
     */
    public class RevenueReportDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalReservations { get; set; }
        public decimal AverageRevenuePerReservation { get; set; }
        public List<RevenueDetailDto> Details { get; set; } = new();
    }

    /**
     * [UC12.2 – Generate Revenue Report]
     * DTO phụ trợ phân tích chi tiết dòng tiền doanh thu mang lại từ một đơn đặt phòng cụ thể của khách hàng[cite: 1].
     */
    public class RevenueDetailDto
    {
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal Revenue { get; set; }
    }

    /**
     * [UC12.3 – Generate Financial Report]
     * DTO tổng hợp tình hình tài chính tổng thể của khách sạn, phân tách rõ nguồn thu từ tiền phòng và theo dõi biến động các đơn đặt phòng[cite: 1].
     */
    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal RoomRevenue { get; set; }
        public int TotalReservations { get; set; }
        public int CancelledReservations { get; set; }
        public List<FinancialDetailDto> Details { get; set; } = new();
    }

    /**
     * [UC12.3 – Generate Financial Report]
     * DTO phụ trợ phân rã chi tiết cơ cấu doanh thu tài chính đóng góp theo từng phân loại hạng phòng (Room Type)[cite: 1].
     */
    public class FinancialDetailDto
    {
        public string RoomTypeName { get; set; } = null!;
        public int TotalReservations { get; set; }
        public decimal Revenue { get; set; }
    }

    /**
     * [UC12.4 – Generate Staff Performance Report]
     * DTO tổng hợp dữ liệu báo cáo hiệu suất và năng suất lao động của toàn bộ đội ngũ nhân sự trong kỳ đánh giá[cite: 1].
     */
    public class StaffPerformanceReportDto
    {
        public int TotalStaff { get; set; }
        public int TotalTasksCompleted { get; set; }
        public List<StaffPerformanceDetailDto> Details { get; set; } = new();
    }

    /**
     * [UC12.4 – Generate Staff Performance Report]
     * DTO phụ trợ thống kê và phân tích chi tiết năng suất công việc của từng nhân viên buồng phòng dựa trên tỷ lệ hoàn thành tác vụ (completion rate) được giao[cite: 1].
     */
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