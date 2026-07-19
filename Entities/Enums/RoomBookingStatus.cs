namespace Hotel_System.Entities.Enums
{
    /**
     * [UC07 – Manage Rooms] / [UC14 – Check-in] / [UC16 – Change Room]
     * Danh mục liệt kê các trạng thái kinh doanh và tính khả dụng thương mại (Commercial Availability) của một phòng vật lý.
     * Chỉ số này đóng vai trò quyết định trong việc kiểm soát luồng phân phối phòng, hỗ trợ Tiếp tân tra cứu trực quan trên 
     * sơ đồ phòng thời gian thực (Room Matrix) và chặn các hành vi gán phòng trùng lặp (Overbooking).
     */
    public enum RoomBookingStatus
    {
        /** 
         * Phòng đang trống, sạch sẽ và sẵn sàng đưa vào kinh doanh đón khách.
         * Hệ thống cho phép Tiếp tân lựa chọn phòng này để thiết lập đơn đặt phòng mới (UC13) hoặc làm thủ tục Check-in trực tiếp.
         */
        AVAILABLE,

        /** 
         * Phòng đã được lên lịch giữ chỗ thành công cho một đoàn hoặc khách hàng cụ thể.
         * Trạng thái này tự động kích hoạt khi đơn đặt phòng liên kết được chuyển sang trạng thái 'CONFIRMED' (UC13), 
         * nhằm bảo đảm giữ đúng số phòng cho khách sắp đến và khóa quyền gán phòng này cho các giao dịch khác.
         */
        RESERVED,

        /** 
         * Phòng hiện đang có khách lưu trú thực tế trong kỳ nghỉ.
         * Hệ thống tự động chuyển đổi trạng thái của phòng sang 'OCCUPIED' ngay sau khi Tiếp tân nhấn xác nhận thủ tục Check-in thành công (UC14), 
         * đồng thời mở khóa tính năng ghi nhận tiêu dùng dịch vụ phụ thu cho số phòng này (UC17).
         */
        OCCUPIED,

        /** 
         * Phòng đang tạm dừng khai thác kinh doanh để phục vụ công tác sửa chữa, khắc phục sự cố kỹ thuật vật tư.
         * Trạng thái này được kích hoạt khi có phiếu báo cáo sự cố nghiêm trọng (UC19.4). 
         * Hệ thống sẽ loại bỏ phòng này khỏi danh sách phòng khả dụng nhằm ngăn chặn tuyệt đối việc xếp khách vào phòng lỗi.
         */
        MAINTENANCE
    }
}