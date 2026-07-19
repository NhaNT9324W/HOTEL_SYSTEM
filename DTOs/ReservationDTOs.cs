using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC13.1 - View List Reservations] / [UC13.4 - Search Reservation]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) mô tả cấu trúc rút gọn của một đơn đặt phòng khách sạn[cite: 1].
     * Được sử dụng để hiển thị thông tin tổng quan theo dạng danh sách hoặc kết quả tìm kiếm nhanh trên giao diện tiếp tân[cite: 1].
     */
    public class ReservationListDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string GuestName { get; set; } = null!;
        public string GuestPhone { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = null!;
    }

    /**
     * [UC13.2 - View Detail Reservation]
     * Đối tượng chuyển đổi dữ liệu kế thừa từ ReservationListDto, bổ sung đầy đủ thông tin chi tiết của một đơn đặt phòng[cite: 1].
     * Hiển thị chi tiết toàn bộ hồ sơ thông tin cá nhân khách lưu trú, phân loại hạng phòng, số tầng và thời gian khởi tạo đơn đặt phòng khi bấm xem chi tiết[cite: 1].
     */
    public class ReservationDetailDto : ReservationListDto
    {
        public string GuestIdNumber { get; set; } = null!;
        public string? GuestEmail { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public int Floor { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /**
     * [UC13.3 - Create Reservation]
     * DTO tiếp nhận và kiểm tra ràng buộc dữ liệu đầu vào khi bộ phận Tiếp tân thực hiện khởi tạo một đơn đặt phòng mới[cite: 1].
     * Sử dụng các thuộc tính Data Annotations nhằm đảm bảo tính bắt buộc của thông tin định danh khách hàng, phòng lựa chọn và mốc thời gian lưu trú[cite: 1].
     */
    public class CreateReservationDto
    {
        [Required(ErrorMessage = "Guest name is required")]
        public string GuestFullName { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        public string GuestPhone { get; set; } = null!;

        [Required(ErrorMessage = "ID Number (CCCD) is required")]
        public string GuestIdNumber { get; set; } = null!;

        public string? GuestEmail { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        public DateTime CheckOutDate { get; set; }
    }

    /**
     * [UC13.5 - Edit Reservation]
     * DTO tiếp nhận thông tin cập nhật điều chỉnh mới liên quan đến số phòng chọn, ngày nhận phòng hoặc ngày trả phòng cho một đơn đặt phòng đang tồn tại[cite: 1].
     */
    public class UpdateReservationDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}