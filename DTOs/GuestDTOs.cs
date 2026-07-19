namespace Hotel_System.DTOs
{
    /**
     * [UC15 – Manage Guest Profiles]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) mô tả cấu trúc rút gọn của hồ sơ khách hàng.
     */
    public class GuestListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }

    /**
     * [UC15.2 – View Guest Profile]
     * Bổ sung đầy đủ thông tin chi tiết bao gồm cả số định danh cá nhân khi bấm xem chi tiết.
     */
    public class GuestDetailDto : GuestListDto
    {
        public string IdNumber { get; set; } = null!; // 🔥 BỔ SUNG: Hiển thị CCCD/Passport khi xem chi tiết
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReservations { get; set; }
    }

    /**
     * [UC15.1 – Create Guest Information]
     * Tiếp nhận dữ liệu đầu vào phục vụ khởi tạo hồ sơ khách hàng mới.
     */
    public class CreateGuestDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string IdNumber { get; set; } = null!; // 🔥 BỔ SUNG: Tiếp nhận trường CCCD từ giao diện gửi lên
        public string? Email { get; set; }
    }

    /**
     * [UC15.3 – Edit Guest Profile]
     * Tiếp nhận dữ liệu cập nhật hồ sơ khách hàng hiện tại.
     */
    public class UpdateGuestDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string IdNumber { get; set; } = null!; // 🔥 BỔ SUNG: Cho phép cập nhật lại số CCCD/Passport mới
        public string? Email { get; set; }
    }
}