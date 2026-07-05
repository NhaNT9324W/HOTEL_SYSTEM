using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        Task<bool> ForgotPasswordAsync(string email, string baseUrl);
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<bool> EditProfileAsync(int userId, EditProfileDto dto);
        Task<bool> ResetPasswordByAdminAsync(int userId, ResetPasswordByAdminDto dto);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}