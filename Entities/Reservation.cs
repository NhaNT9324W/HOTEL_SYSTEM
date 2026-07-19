using Hotel_System.Entities.Enums;
using System;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.7 Reservation Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Reservations' trong cơ sở dữ liệu.
     * Quản lý toàn bộ thông tin chi tiết về các đơn đặt phòng của khách lưu trú, thời gian lưu ở dự kiến và liên kết phòng vật lý.
     * Thực thể này đóng vai trò lõi trong mô-đun Quản lý đặt phòng (UC13).
     * 
     * Lưu ý kiến trúc: Các quy tắc nghiệp vụ kiểm tra trạng thái phòng trống, kiểm tra trùng lịch đặt phòng (Overlapping) 
     * và ràng buộc trạng thái phòng vật lý được xử lý và kiểm tra tập trung tại tầng Service Layer, không đặt logic này 
     * trực tiếp trong lớp Entity để bảo đảm tính phân tách trách nhiệm (Separation of Concerns).
     */
    public class Reservation : BaseEntity
    {
        /** 
         * Mã định danh ngoại khóa liên kết trực tiếp tới phòng vật lý được lựa chọn thuê (Room).
         * Tham gia vào bộ lọc kiểm tra tính khả dụng của phòng tại tầng nghiệp vụ trước khi ghi nhận đặt phòng.
         */
        public int RoomId { get; set; }

        /** Thuộc tính điều hướng liên kết thông tin chi tiết của phòng vật lý liên quan. */
        public Room? Room { get; set; }

        /** Mã định danh ngoại khóa liên kết tới hồ sơ thông tin của khách hàng thực hiện đăng ký đặt phòng (Guest). */
        public int GuestId { get; set; }

        /** Thuộc tính điều hướng liên kết thông tin chi tiết của hồ sơ khách hàng tương ứng. */
        public Guest? Guest { get; set; }

        /** Mốc thời gian dự kiến khách hàng nhận phòng (Check-in), được chuẩn hóa theo múi giờ UTC. */
        public DateTime CheckInDate { get; set; }

        /** 
         * Mốc thời gian dự kiến khách hàng trả phòng (Check-out). 
         * Ràng buộc nghiệp vụ ngày trả phòng bắt buộc phải xảy ra sau ngày nhận phòng được kiểm soát chặt chẽ ở tầng Service.
         */
        public DateTime CheckOutDate { get; set; }

        /** 
         * Trạng thái vận hành hiện tại của đơn đặt phòng (Enum: PENDING, CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED).
         * Giá trị mặc định khi tiếp tân khởi tạo đơn đặt phòng mới trên giao diện hệ thống là PENDING.
         */
        public ReservationStatus Status { get; set; } = ReservationStatus.PENDING;
    }
}