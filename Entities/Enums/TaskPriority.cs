namespace Hotel_System.Entities.Enums
{
    /**
     * [UC19 – Manage Housekeeping Tasks] / [UC19.1 – View Assigned Tasks]
     * Danh mục liệt kê các mức độ ưu tiên và độ khẩn cấp của một tác vụ buồng phòng hoặc sự cố bảo trì.
     * Chỉ số này giúp bộ phận Điều hành phân phối công việc hợp lý, đồng thời hỗ trợ nhân viên Room Staff 
     * chủ động lọc và sắp xếp thứ tự xử lý phòng nhằm tối ưu hóa thời gian chờ nhận phòng của khách hàng.
     */
    public enum TaskPriority
    {
        /** 
         * Mức độ ưu tiên thấp.
         * Áp dụng cho các tác vụ vệ sinh định kỳ, tổng vệ sinh theo tuần hoặc kiểm tra các phòng trống dài ngày không có áp lực thời gian đón khách.
         */
        Low,

        /** 
         * Mức độ ưu tiên trung bình.
         * Áp dụng cho lịch dọn dẹp phòng thường nhật của khách đang lưu trú (Occupied Rooms) hoặc chuẩn bị phôi phòng cho các lượt Check-in tiêu chuẩn trong ngày.
         */
        Medium,

        /** 
         * Mức độ ưu tiên cao / Khẩn cấp.
         * Áp dụng cho việc làm sạch phòng gấp phục vụ khách VIP, xử lý các sự cố hỏng hóc kỹ thuật phát sinh đột xuất (UC19.4), 
         * hoặc dọn dẹp phòng trống vừa Check-out trong giai đoạn cao điểm để kịp bàn giao cho lượt khách tiếp theo.
         */
        High
    }
}