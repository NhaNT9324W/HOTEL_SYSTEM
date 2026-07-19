using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.1.I IAccountRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể Account.
     * Phục vụ các nghiệp vụ: Đăng nhập (UC01), Đổi/Quên mật khẩu (UC02, UC03) và Quản lý tài khoản (UC05).
     */
    public interface IAccountRepository
    {
        /** Lấy toàn bộ danh sách tài khoản nhân sự phục vụ màn hình quản trị (UC05.1). */
        Task<IEnumerable<Account>> GetAllAsync();

        /** Tìm tài khoản theo ID phục vụ xem chi tiết hoặc chuẩn bị cập nhật (UC05.2). */
        Task<Account?> GetByIdAsync(int id);

        /** Tìm tài khoản theo Username phục vụ kiểm tra xác thực khi Đăng nhập (UC01). */
        Task<Account?> GetByUsernameAsync(string username);

        /** Tìm tài khoản theo Email phục vụ luồng xác minh danh tính khi Quên mật khẩu (UC03). */
        Task<Account?> GetByEmailAsync(string email);

        /** Tìm tài khoản theo Reset Token để xác thực liên kết đặt lại mật khẩu (UC03). */
        Task<Account?> GetByResetTokenAsync(string token);

        /** Tìm kiếm tương đối tài khoản theo từ khóa (FullName, Username, Email) tại bộ lọc quản trị. */
        Task<IEnumerable<Account>> SearchAsync(string keyword);

        /** Kiểm tra trùng lặp Username trước khi tạo tài khoản mới (Ràng buộc duy nhất). */
        Task<bool> IsUsernameExistsAsync(string username);

        /** Kiểm tra trùng lặp Email trên toàn hệ thống để bảo đảm an toàn định danh. */
        Task<bool> IsEmailExistsAsync(string email);

        /** Thêm mới một bản ghi tài khoản và lưu trực tiếp xuống cơ sở dữ liệu. */
        Task AddAsync(Account account);

        /** Cập nhật thông tin tài khoản, tự động ghi vết thời gian hiệu chỉnh (UpdatedAt). */
        Task UpdateAsync(Account account);

        /** Xóa vật lý bản ghi tài khoản khỏi hệ thống dựa trên ID. */
        Task DeleteAsync(int id);
    }
}