using System.Collections.Generic;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.2 Guest Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Guests' trong cơ sở dữ liệu.
     * Lưu trữ toàn bộ hồ sơ thông tin cá nhân, phương thức liên lạc và thông tin định danh pháp lý của khách hàng.
     * Thực thể này tham gia trực tiếp vào mô-đun Quản lý hồ sơ khách hàng (UC15) và quy trình Tiếp tân lập đơn đặt phòng (UC13).
     */
    public class Guest : BaseEntity
    {
        /** 
         * Tên đầy đủ của khách hàng.
         * Áp dụng Business Rule BR-22: Ràng buộc trường thông tin bắt buộc, cấu hình độ dài tối đa 150 ký tự trên giao diện.
         */
        public string FullName { get; set; } = null!;

        /** 
         * Số điện thoại liên hệ trực tiếp của khách hàng.
         * Kết hợp cùng Email cấu thành bộ lọc kiểm tra ràng buộc phương thức liên lạc tối thiểu của hồ sơ.
         */
        public string Phone { get; set; } = null!;

        /** 
         * Số giấy tờ định danh cá nhân (Căn cước công dân - CCCD hoặc Hộ chiếu - Passport).
         * Thông tin bắt buộc phải thu thập tại quầy tiếp tân khi làm thủ tục Check-in nhằm phục vụ công tác khai báo lưu trú.
         */
        public string IdNumber { get; set; } = null!; // CCCD

        /** Địa chỉ thư điện tử cá nhân của khách hàng (có thể để trống), phục vụ gửi phôi xác nhận đặt phòng và hóa đơn điện tử. */
        public string? Email { get; set; }

        /** 
         * Cờ đánh dấu trạng thái xử lý xóa mềm (Soft Delete) hồ sơ khách hàng.
         * Mặc định là false. Khi chuyển thành true, hệ thống sẽ ẩn bản ghi khỏi các tác vụ tìm kiếm/hiển thị thông thường nhưng không xóa vĩnh viễn khỏi database để bảo toàn tính toàn vẹn dữ liệu kế toán tài chính.
         */
        public bool IsDeleted { get; set; } = false;

        /** 
         * Bộ sưu tập lịch sử toàn bộ các đơn đặt phòng (Reservations) mà khách hàng này từng thực hiện tại khách sạn.
         * Thể hiện mối quan hệ cấu trúc Một-Nhiều (One-to-Many) điều hướng ngược từ Guest sang thực thể Reservation.
         */
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}