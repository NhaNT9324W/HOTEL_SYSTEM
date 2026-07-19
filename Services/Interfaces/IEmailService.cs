using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.5.I IEmailService Interface]
     * Giao diện quy định các phương thức dịch vụ gửi thư điện tử tự động của hệ thống.
     * Làm hợp đồng cho EmailService thực thi, phục vụ công tác truyền thông tin bảo mật và luồng Quên mật khẩu (UC03).
     */
    public interface IEmailService
    {
        /** 
         * Khởi tạo và phát hành email chứa liên kết đặt lại mật khẩu an toàn cho nhân sự (UC03).
         * Giao diện phôi thư sẽ được cấu hình dạng HTML, đính kèm kèm mã xác thực Reset Token có thời hạn.
         */
        Task SendResetPasswordEmailAsync(string toEmail, string toName, string resetLink);
    }
}