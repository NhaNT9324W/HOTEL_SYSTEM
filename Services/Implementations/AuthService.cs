using Hotel_System.DTOs;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.2 AuthService Implementation]
     * Lớp xử lý nghiệp vụ xác thực, phân quyền và bảo mật tài khoản.
     * Điều phối toàn bộ các luồng: Đăng nhập (UC01), Đổi mật khẩu (UC02), Quên mật khẩu (UC03) và cập nhật Hồ sơ cá nhân.
     */
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        /** Inject các dịch vụ phụ thuộc về tài khoản, cấu hình hệ thống và gửi email qua Constructor. */
        public AuthService(
            IAccountRepository repo,
            IConfiguration config,
            IEmailService emailService)
        {
            _repo = repo;
            _config = config;
            _emailService = emailService;
        }

        /** 
         * Xác thực tài khoản đăng nhập (UC01).
         * Kiểm tra sự tồn tại của Username, đối chiếu mã băm mật khẩu và xác minh tài khoản đang ở trạng thái kích hoạt (Active).
         */
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var account = await _repo.GetByUsernameAsync(dto.Username);
            if (account == null) return null;
            if (!VerifyPassword(dto.Password, account.PasswordHash)) return null;
            if (account.Status != Entities.Enums.AccountStatus.Active) return null;

            return new LoginResponseDto
            {
                Token = GenerateJwtToken(account),
                FullName = account.FullName,
                Role = account.Role.ToString(),
                UserId = account.Id
            };
        }

        /** 
         * Khởi tạo quy trình khôi phục mật khẩu khi quên (UC03).
         * Sinh mã Token ngẫu nhiên có độ mã hóa cao, lưu thời gian hết hạn (30 phút) và gửi link xác thực qua Email.
         */
        public async Task<bool> ForgotPasswordAsync(string email, string baseUrl)
        {
            var account = await _repo.GetByEmailAsync(email);
            if (account == null) return false;

            // Tạo mã token an toàn ngẫu nhiên và loại bỏ ký tự đặc biệt của URL
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace("+", "-").Replace("/", "_").Replace("=", "");

            account.ResetToken = token;
            account.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _repo.UpdateAsync(account);

            var resetLink = $"{baseUrl}/ResetPassword?token={token}";
            await _emailService.SendResetPasswordEmailAsync(
                account.Email, account.FullName, resetLink);

            return true;
        }

        /** Xác thực Reset Token hợp lệ và tiến hành cập nhật mật khẩu mới cho người dùng (UC03). */
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var account = await _repo.GetByResetTokenAsync(dto.Token);
            if (account == null) return false;
            if (account.ResetTokenExpiry < DateTime.UtcNow) return false;

            account.PasswordHash = HashPassword(dto.NewPassword);
            account.ResetToken = null;
            account.ResetTokenExpiry = null;
            await _repo.UpdateAsync(account);

            return true;
        }

        /** Cho phép người dùng chủ động thực hiện Đổi mật khẩu định kỳ bảo mật (UC02) sau khi đã đăng nhập thành công. */
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword) return false;

            var account = await _repo.GetByIdAsync(userId);
            if (account == null) return false;
            if (!VerifyPassword(dto.CurrentPassword, account.PasswordHash)) return false;

            account.PasswordHash = HashPassword(dto.NewPassword);
            await _repo.UpdateAsync(account);

            return true;
        }

        /** Cho phép nhân sự tự cập nhật thông tin hồ sơ cá nhân, bảo đảm không bị trùng lặp Email với nhân sự khác. */
        public async Task<bool> EditProfileAsync(int userId, EditProfileDto dto)
        {
            var account = await _repo.GetByIdAsync(userId);
            if (account == null) return false;

            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null && existing.Id != userId) return false;

            account.FullName = dto.FullName;
            account.Email = dto.Email;
            account.Phone = dto.Phone;
            await _repo.UpdateAsync(account);

            return true;
        }

        /** Quyền của Quản trị viên (Admin): Cưỡng bức đặt lại mật khẩu mới cho một nhân viên cụ thể (UC05). */
        public async Task<bool> ResetPasswordByAdminAsync(int userId, ResetPasswordByAdminDto dto)
        {
            var account = await _repo.GetByIdAsync(userId);
            if (account == null) return false;

            account.PasswordHash = HashPassword(dto.NewPassword);
            await _repo.UpdateAsync(account);

            return true;
        }

        /** Tiện ích băm một chiều dữ liệu mật khẩu thô đầu vào sang chuẩn mã hóa SHA256 an toàn. */
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /** Đối chiếu so sánh mật khẩu người dùng vừa nhập với chuỗi hash lưu trữ trong database. */
        public bool VerifyPassword(string password, string hash) =>
            HashPassword(password) == hash;

        /** Khởi tạo chuỗi JWT Token định danh State-less chứa danh sách Claims phân quyền cá nhân, thời hạn hiệu lực 8 tiếng. */
        private string GenerateJwtToken(Entities.Account account)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Username),
                new Claim(ClaimTypes.GivenName, account.FullName),
                new Claim(ClaimTypes.Role, account.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),  // DUY TRÌ ĐĂNG NHẬP 8 TIẾNG TRƯỚC KHI HẾT HẠN TOKEN
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}