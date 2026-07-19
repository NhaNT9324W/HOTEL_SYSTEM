using Hotel_System.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC05 - Manage Account]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) dùng để hiển thị và truyền tải thông tin cấu trúc tài khoản người dùng giữa các tầng hệ thống mà không làm lộ dữ liệu nhạy cảm (như mật khẩu)[cite: 1].
     */
    public class AccountDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Role Role { get; set; }
        public AccountStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /**
     * [UC5.1 - Create Account]
     * DTO tiếp nhận và ràng buộc các điều kiện dữ liệu đầu vào khi Admin thực hiện tạo mới một tài khoản nhân viên[cite: 1].
     * Sử dụng thuộc tính thuộc gói Data Annotations giúp tự động kiểm tra định dạng dữ liệu, độ dài và thuộc tính bắt buộc ngay tại form level[cite: 1].
     */
    public class CreateAccountDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }
    }

    /**
     * [UC5.2 - Edit Account]
     * DTO tiếp nhận dữ liệu cập nhật khi Admin thực hiện hiệu chỉnh thông tin hồ sơ của một tài khoản nhân viên đang tồn tại trên hệ thống khách sạn[cite: 1].
     */
    public class UpdateAccountDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public Role Role { get; set; }

        [Required]
        public AccountStatus Status { get; set; }
    }
}