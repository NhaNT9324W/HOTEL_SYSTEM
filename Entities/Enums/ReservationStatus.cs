namespace Hotel_System.Entities.Enums
{
    /**
     * [UC13 – Manage Reservations] / [UC14 – Check-in] / [UC18 – Check-out]
     * Danh mục liệt kê toàn bộ các trạng thái trong vòng đời vận hành của một đơn đặt phòng (Reservation Life Cycle).
     * Chỉ số này kiểm soát chặt chẽ luồng nghiệp vụ tác nghiệp tại quầy tiếp tân, điều hướng luồng dữ liệu tài chính 
     * và tự động đồng bộ trạng thái kinh doanh (BookingStatus) của các phòng vật lý liên quan trong hệ thống.
     */
    public enum ReservationStatus
    {
        /** 
         * Đơn đặt phòng vừa được khởi tạo tạm thời hoặc tiếp nhận thông tin từ các kênh phân phối.
         * Đây là trạng thái mặc định ban đầu khi Tiếp tân lên phôi đặt phòng (UC13), hệ thống tạm thời giữ chỗ và chờ khách hàng hoàn tất thủ tục xác minh hoặc đặt cọc.
         */
        PENDING,

        /** 
         * Đơn đặt phòng đã được xác thực chính thức và bảo đảm trên hệ thống.
         * Kích hoạt khi khách hàng hoàn tất việc thanh toán khoản tiền đặt cọc giữ chỗ tuân thủ nghiêm ngặt Business Rule BR-12, phòng vật lý sẽ chuyển sang trạng thái 'RESERVED'.
         */
        CONFIRMED,

        /** 
         * Khách lưu trú đã đến khách sạn và hoàn tất thủ tục nhận phòng thực tế tại quầy lễ tân (UC14).
         * Sự kiện chuyển đổi này sẽ kích hoạt hệ thống tự động đổi trạng thái kinh doanh của phòng vật lý liên quan sang 'OCCUPIED' để bắt đầu ghi nhận thời gian lưu trú.
         */
        CHECKED_IN,

        /** 
         * Khách lưu trú đã thực hiện trả phòng, đối soát quyết toán toàn bộ folio chi phí và in hóa đơn tài chính thành công (UC18).
         * Hệ thống sẽ đóng đơn đặt phòng này, giải phóng phòng vật lý về trạng thái 'AVAILABLE' và chuyển trạng thái vệ sinh sang 'DIRTY'.
         */
        CHECKED_OUT,

        /** 
         * Đơn đặt phòng đã bị hủy bỏ theo yêu cầu chủ động của khách hàng hoặc do nhân viên có thẩm quyền hủy trên hệ thống.
         * Phòng vật lý liên kết sẽ lập tức được giải phóng, hệ thống áp dụng chính sách phạt hủy cọc tùy thuộc vào mốc thời gian xử lý nghiệp vụ.
         */
        CANCELED,

        /** 
         * Trạng thái vắng mặt, áp dụng khi khách hàng không đến làm thủ tục nhận phòng theo lịch trình đã cam kết mà không có thông báo trước.
         * Hệ thống sẽ tự động chuyển đổi hoặc cho phép Tiếp tân cập nhật thủ công sau mốc giờ giới hạn quy định (ví dụ: sau 18:00 của ngày Check-in), phục vụ quy trình xử lý tịch thu tiền cọc.
         */
        NO_SHOW
    }
}