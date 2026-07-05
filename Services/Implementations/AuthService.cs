using Hotel_System.DTOs;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_System.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _repo;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(
            IAccountRepository repo,
            IConfiguration config,
            IEmailService emailService)
        {
            _repo = repo;
            _config = config;
            _emailService = emailService;
        }

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

        public async Task<bool> ForgotPasswordAsync(string email, string baseUrl)
        {
            var account = await _repo.GetByEmailAsync(email);
            if (account == null) return false;

            // Tạo reset token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace("+", "-").Replace("/", "_").Replace("=", "");

            account.ResetToken = token;
            account.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _repo.UpdateAsync(account);

            // Gửi email
            var resetLink = $"{baseUrl}/ResetPassword?token={token}";
            await _emailService.SendResetPasswordEmailAsync(
                account.Email, account.FullName, resetLink);

            return true;
        }

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

        public async Task<bool> EditProfileAsync(int userId, EditProfileDto dto)
        {
            var account = await _repo.GetByIdAsync(userId);
            if (account == null) return false;

            // Kiểm tra email trùng với người khác
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null && existing.Id != userId) return false;

            account.FullName = dto.FullName;
            account.Email = dto.Email;
            account.Phone = dto.Phone;
            await _repo.UpdateAsync(account);

            return true;
        }

        public async Task<bool> ResetPasswordByAdminAsync(int userId, ResetPasswordByAdminDto dto)
        {
            var account = await _repo.GetByIdAsync(userId);
            if (account == null) return false;

            account.PasswordHash = HashPassword(dto.NewPassword);
            await _repo.UpdateAsync(account);

            return true;
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool VerifyPassword(string password, string hash) =>
            HashPassword(password) == hash;

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
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}