using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC01 - Login]
     * DTO tiếp nhận dữ liệu yêu cầu đăng nhập từ phía người dùng bao gồm tài khoản và mật khẩu văn bản thô[cite: 1].
     */
    public class LoginDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }

    /**
     * [UC01 - Login]
     * DTO phản hồi dữ liệu khi người dùng xác thực thông tin đăng nhập thành công vào hệ thống khách sạn[cite: 1].
     * Chứa chuỗi mã hóa JWT token để duy trì phiên làm việc cùng thông tin phân quyền vai trò phục vụ điều hướng giao diện[cite: 1].
     */
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    /**
     * [UC03 - Forgot Password]
     * DTO tiếp nhận địa chỉ email đăng ký tài khoản từ giao diện yêu cầu quên mật khẩu để xác minh danh tính[cite: 1].
     */
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    /**
     * [UC03 - Forgot Password]
     * DTO chứa mã xác thực khôi phục và mật khẩu mới phục vụ cho quy trình đặt lại mật khẩu từ liên kết email[cite: 1].
     */
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    /**
     * [UC04 - Change Password]
     * DTO thu thập thông tin thay đổi mật khẩu chủ động bảo mật của nhân viên khi đang ở trong phiên đăng nhập[cite: 1].
     * Ràng buộc kiểm tra tính hợp lệ của mật khẩu hiện tại, độ dài mật khẩu mới và xác nhận chuỗi nhập lại trùng khớp[cite: 1].
     */
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /**
     * [Account Settings - Profile Update]
     * DTO truyền tải thông tin cập nhật dữ liệu hồ sơ cá nhân tự quản lý của mỗi nhân viên khách sạn[cite: 1].
     */
    public class EditProfileDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;
    }

    /**
     * [UC05 - Manage Account]
     * DTO chứa dữ liệu thiết lập lại mật khẩu mới do Admin chỉ định trực tiếp khi ép buộc khôi phục tài khoản cho nhân viên[cite: 1].
     */
    public class ResetPasswordByAdminDto
    {
        [Required(ErrorMessage = "New password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }
}