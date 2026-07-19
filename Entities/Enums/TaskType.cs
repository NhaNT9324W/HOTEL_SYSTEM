namespace Hotel_System.Entities.Enums
{
    /**
     * [UC19 – Manage Housekeeping Tasks] / [UC20 – Room Management & Inspection]
     * Danh mục phân loại các hình thức tác vụ vận hành, vệ sinh và bảo trì vật lý áp dụng tại các phòng trong khách sạn.
     * Chỉ số này định hình luồng công việc chuyên biệt của nhân sự thực địa (Room Staff / Kỹ thuật viên) và quy định cách thức 
     * hệ thống xử lý logic tự động cập nhật trạng thái buồng phòng hoặc trạng thái kinh doanh của phòng vật lý liên quan.
     */
    public enum TaskType
    {
        /** 
         * Tác vụ dọn dẹp và vệ sinh phòng thông thường.
         * Bao gồm công tác dọn dẹp định kỳ hàng ngày cho khách đang lưu trú hoặc tổng vệ sinh làm sạch sâu ngay sau khi Check-out (UC18), 
         * đóng vai trò là điều kiện cần để chuyển trạng thái vệ sinh của phòng từ 'DIRTY' sang 'CLEAN' (UC19.2).
         */
        Cleaning,

        /** 
         * Tác vụ sửa chữa, khắc phục và bảo trì kỹ thuật trang thiết bị vật tư.
         * Được khởi tạo dựa trên các phiếu ghi nhận sự cố hư hỏng đột xuất phát sinh trong phòng (UC19.4); 
         * phối hợp điều phối kỹ thuật viên xử lý và là cơ sở để hệ thống khóa quyền khai thác kinh doanh của phòng sang trạng thái 'MAINTENANCE' (UC07).
         */
        Maintenance,

        /** 
         * Tác vụ kiểm tra và nghiệm thu chất lượng phòng.
         * Được thực hiện độc lập bởi Trưởng bộ phận buồng phòng hoặc Giám sát viên nhằm đánh giá phòng sau khi nhân viên hoàn tất dọn dẹp. 
         * Đây là bước kiểm soát chất lượng nghiêm ngặt (UC20) để chính thức phê duyệt phòng chuyển sang trạng thái 'READY' đủ điều kiện đón khách mới.
         */
        Inspection
    }
}