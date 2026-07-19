using System;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.5 Invoice Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Invoices' trong cơ sở dữ liệu.
     * Lưu trữ toàn bộ chứng từ tài chính và hồ sơ quyết toán chính thức khi khách hàng hoàn tất thủ tục trả phòng.
     * Thực thể này đóng vai trò trung tâm trong mô-đun Quản lý Thanh toán và Xuất hóa đơn (UC18), 
     * ghi nhận chi tiết dòng tiền doanh thu thu được từ tiền phòng vật lý và các dịch vụ phụ thu tiện ích.
     */
    public class Invoice : BaseEntity
    {
        /** 
         * Mã định danh ngoại khóa liên kết duy nhất tới đơn đặt phòng tương ứng (Reservation).
         * Thiết lập ràng buộc quan hệ Một-Một (One-to-One) nghiêm ngặt tại Fluent API: 
         * Mỗi đơn đặt phòng chỉ được phép tạo và xuất bản duy nhất một hóa đơn tài chính chính thức.
         */
        public int ReservationId { get; set; }

        /** Thuộc tính điều hướng liên kết chi tiết hồ sơ thông tin của đơn đặt phòng (Reservation) liên quan. */
        public Reservation? Reservation { get; set; }

        /** 
         * Tổng chi phí thuê phòng vật lý của khách hàng trong suốt kỳ lưu trú.
         * Áp dụng định dạng kiểu dữ liệu tiền tệ decimal(18,2) tuân thủ nghiêm ngặt Business Rule BR-35.
         */
        public decimal RoomCharge { get; set; }

        /** 
         * Tổng biểu phí tiêu dùng tất cả các dịch vụ tiện ích bổ sung phát sinh trong kỳ lưu trú.
         * Được hệ thống tự động tổng hợp dựa trên lịch sử bản ghi ServiceUsage liên kết với đơn đặt phòng.
         */
        public decimal ServiceCharge { get; set; }

        /** 
         * Tổng số tiền cuối cùng khách hàng bắt buộc phải thanh toán khi làm thủ tục Check-out.
         * Giá trị được tính toán tự động dựa trên công thức nghiệp vụ: TotalAmount = RoomCharge + ServiceCharge.
         */
        public decimal TotalAmount { get; set; }

        /** Tổng số đêm lưu trú thực tế của khách hàng tại khách sạn, dùng làm cơ sở dữ liệu gốc để đối chiếu và tính toán khoản tiền RoomCharge. */
        public int Nights { get; set; }

        /** 
         * Mốc thời gian chính thức in và ban hành hóa đơn tài chính này. 
         * Hệ thống thiết lập mặc định theo chuẩn múi giờ quốc tế UTC (DateTime.UtcNow) để phục vụ đồng bộ dữ liệu kế toán tài chính.
         */
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}