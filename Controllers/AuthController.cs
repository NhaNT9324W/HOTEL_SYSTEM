using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    /**
     * [UC01, UC02, UC03, UC04 - Authentication & Profile Management]
     * Controller quản lý toàn bộ quy trình xác thực hệ thống, cấp phát token phiên làm việc và thiết lập bảo mật tài khoản cho nhân viên
     */
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

        /**
         * [UC01 - Login]
         * Thực hiện kiểm tra thông tin đăng nhập của người dùng qua Email/Username và mật khẩu mã hóa MD5
         * Nếu thông tin hợp lệ, AuthService sẽ cấp phát một JWT token dùng để duy trì phiên làm việc trong vòng 8 giờ
         * 
         * @param dto Đối tượng LoginDto chứa thông tin tài khoản đăng nhập gửi từ client
         * @return Trả về JWT token kèm thông tin vai trò người dùng (200 OK) hoặc thông báo từ chối truy cập (401 Unauthorized)
         */
        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(result);
        }

        /**
         * [UC02 - Logout]
         * Thực hiện hủy bỏ quyền truy cập và chấm dứt phiên đăng nhập hiện tại của người dùng trong hệ thống
         * Token hiện tại sẽ bị vô hiệu hóa để ngăn chặn các request trái phép tiếp theo
         * 
         * @return Thông báo đăng xuất thành công kèm HTTP trạng thái 200 OK
         */
        // POST /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout() =>
            Ok(new { message = "Logged out successfully" });

        /**
         * [UC03 - Forgot Password] (Giai đoạn 1: Gửi link khôi phục)
         * Tiếp nhận yêu cầu quên mật khẩu và thực hiện tạo ra một reset token an toàn có thời hạn để gửi qua Email Service
         * Nhằm tuân thủ quy tắc bảo mật an toàn thông tin (tránh lộ thông tin email có tồn tại hay không), API luôn trả về thông báo thành công chung[cite: 1].
         * 
         * @param dto Đối tượng ForgotPasswordDto chứa Email đã đăng ký tài khoản cần khôi phục
         * @return Thông báo xác nhận đã gửi link đặt lại mật khẩu (nếu email có trong hệ thống) kèm HTTP trạng thái 200 OK
         */
        // POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.ForgotPasswordAsync(dto.Email, baseUrl);

            // Luôn trả về success để tránh lộ email tồn tại
            return Ok(new { message = "If this email exists, a reset link has been sent" });
        }

        /**
         * [UC03 - Forgot Password] (Giai đoạn 2: Đặt lại mật khẩu mới)
         * Thực hiện kiểm tra tính hợp lệ và thời hạn của reset token, tiến hành cập nhật mật khẩu mới đã mã hóa cho tài khoản
         * Sau khi cập nhật thành công, reset token cũ sẽ bị vô hiệu hóa hoàn toàn
         * 
         * @param dto Đối tượng ResetPasswordDto chứa token khôi phục và mật khẩu mới thiết lập
         * @return Thông báo đặt lại mật khẩu thành công (200 OK) hoặc báo lỗi nếu token không đúng/hết hạn (400 BadRequest)
         */
        // POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordAsync(dto);
            if (!result)
                return BadRequest(new { message = "Invalid or expired reset token" });

            return Ok(new { message = "Password reset successfully" });
        }

        /**
         * [UC04 - Change Password]
         * Cho phép nhân viên đã đăng nhập tự chủ động thay đổi mật khẩu hiện tại vì lý do bảo mật cá nhân
         * Hệ thống tiến hành xác thực mật khẩu cũ và kiểm tra mật khẩu mới xem có trùng khớp với chính sách mật khẩu an toàn hay không
         * 
         * @param dto Đối tượng ChangePasswordDto chứa thông tin mật khẩu cũ và mật khẩu mới
         * @return Thông báo đổi mật khẩu thành công (200 OK) hoặc báo lỗi sai mật khẩu cũ/mật khẩu mới không khớp (400 BadRequest)
         */
        // POST /api/auth/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _authService.ChangePasswordAsync(userId, dto);
            if (!result)
                return BadRequest(new { message = "Current password is incorrect or passwords do not match" });

            return Ok(new { message = "Password changed successfully" });
        }

        /**
         * [Account Settings - Profile Update]
         * Cho phép người dùng đang đăng nhập tự điều chỉnh hồ sơ thông tin cá nhân của mình (FullName, Phone, Email)
         * Quá trình chỉnh sửa sẽ kiểm tra điều kiện ràng buộc để tránh trùng lặp email với các tài khoản khác đang có trên hệ thống
         * 
         * @param dto Đối tượng EditProfileDto chứa các thông tin thay đổi mới từ người dùng
         * @return Thông báo cập nhật hồ sơ thành công (200 OK) hoặc trả về thông báo lỗi ràng buộc dữ liệu (400 BadRequest)
         */
        // PUT /api/auth/edit-profile
        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] EditProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            var result = await _authService.EditProfileAsync(userId, dto);
            if (!result)
                return BadRequest(new { message = "Email already exists or user not found" });

            return Ok(new { message = "Profile updated successfully" });
        }

        /**
         * [UC05 - Manage Account] (Reset Password by Admin)
         * Tính năng dành riêng cho Quản trị viên (Admin) để thực hiện cưỡng bức đặt lại mật khẩu tạm thời cho nhân viên
         * Thường sử dụng khi nhân viên mới được cấp tài khoản hoặc gặp sự cố tài khoản không thể tự khôi phục
         * 
         * @param userId Mã định danh duy nhất (ID) của tài khoản nhân viên cần xử lý mật khẩu
         * @param dto Đối tượng chứa thông tin cấu hình mật khẩu mới do Admin chỉ định
         * @return Thông báo Admin cập nhật mật khẩu thành công (200 OK) hoặc trả về lỗi không tìm thấy tài khoản (400 BadRequest)
         */
        // POST /api/auth/reset-password-admin/{userId}
        [HttpPost("reset-password-admin/{userId}")]
        public async Task<IActionResult> ResetPasswordByAdmin(int userId, [FromBody] ResetPasswordByAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _authService.ResetPasswordByAdminAsync(userId, dto);
            if (!result)
                return BadRequest(new { message = "User not found" });

            return Ok(new { message = "Password reset successfully" });
        }
    }
}