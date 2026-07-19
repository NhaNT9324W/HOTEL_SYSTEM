using Hotel_System.DTOs;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.2.I IAuthService Interface]
     * Giao diện quy định các phương thức xử lý nghiệp vụ xác thực, phân quyền và bảo mật tài khoản.
     * Khai báo các hợp đồng dịch vụ phục vụ Đăng nhập (UC01), Đổi mật khẩu (UC02), Quên mật khẩu (UC03) và quản lý hồ sơ cá nhân.
     */
    public interface IAuthService
    {
        /** Xác thực tài khoản đăng nhập (UC01), đối chiếu mật khẩu và cấp phát JWT Token định danh nếu hợp lệ. */
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        /** Khởi tạo quy trình khôi phục mật khẩu khi quên (UC03), sinh mã Token an toàn và phát hành link qua Email. */
        Task<bool> ForgotPasswordAsync(string email, string baseUrl);

        /** Xác thực Reset Token và tiến hành cập nhật mật khẩu mới cho người dùng sau khi xác minh thời hạn (UC03). */
        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        /** Cho phép nhân sự chủ động thực hiện Đổi mật khẩu định kỳ (UC02) sau khi đã đăng nhập thành công. */
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);

        /** Cho phép nhân sự tự chỉnh sửa thông tin hồ sơ cá nhân (Họ tên, Email, Số điện thoại) trên hệ thống. */
        Task<bool> EditProfileAsync(int userId, EditProfileDto dto);

        /** Quyền hạn của Admin: Cưỡng bức đặt lại mật khẩu mới cho một tài khoản nhân viên cụ thể (UC05). */
        Task<bool> ResetPasswordByAdminAsync(int userId, ResetPasswordByAdminDto dto);

        /** Tiện ích mã hóa một chiều dữ liệu mật khẩu thô đầu vào sang chuỗi băm bảo mật. */
        string HashPassword(string password);

        /** Tiện ích đối chiếu so sánh mật khẩu người dùng vừa nhập với chuỗi hash lưu trữ trong database. */
        bool VerifyPassword(string password, string hash);
    }
}