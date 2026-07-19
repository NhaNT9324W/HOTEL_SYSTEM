using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC06 - Edit Hotel Information]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) dùng để chứa và hiển thị thông tin cấu hình tổng thể hiện tại của khách sạn ra ngoài giao diện (UI)[cite: 1].
     */
    public class HotelInfoDto
    {
        public int Id { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    /**
     * [UC06 - Edit Hotel Information]
     * DTO tiếp nhận dữ liệu từ phía Client gửi lên qua Request Body khi Admin thực hiện cập nhật điều chỉnh thông tin hệ thống khách sạn[cite: 1].
     * Sử dụng các thuộc tính ràng buộc dữ liệu (Data Annotations) nhằm thực thi nghiêm ngặt bộ quy tắc Business Rule BR-18: 
     * Các trường Tên khách sạn (Hotel Name), Số điện thoại liên hệ chính (Main Contact Number) và Địa chỉ vật lý (Physical Address) bắt buộc phải điền, tuyệt đối không được phép để trống trong quá trình cập nhật[cite: 1].
     */
    public class UpdateHotelInfoDto
    {
        [Required(ErrorMessage = "Hotel name is required")]
        [MaxLength(200)]
        public string HotelName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Url(ErrorMessage = "Invalid website URL")]
        public string Website { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
    }
}