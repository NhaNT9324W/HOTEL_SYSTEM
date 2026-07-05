using Hotel_System.DTOs;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_System.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) => _authService = authService;

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

        // POST /api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout() =>
            Ok(new { message = "Logged out successfully" });

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