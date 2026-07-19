using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.1.I IAccountService Interface]
     * Giao diện quy định các phương thức quản lý nghiệp vụ tài khoản và nhân sự.
     * Làm hợp đồng cho AccountService thực thi, phục vụ các luồng xác thực và phân hệ quản trị nhân sự (UC01, UC02, UC05).
     */
    public interface IAccountService
    {
        /** Lấy toàn bộ danh sách tài khoản hệ thống dưới dạng DTO để hiển thị trực quan (UC05.1). */
        Task<IEnumerable<AccountDto>> GetAllAsync();

        /** Tra cứu chi tiết thông tin một tài khoản dựa trên ID. */
        Task<AccountDto?> GetByIdAsync(int id);

        /** Tìm kiếm nhanh tài khoản theo từ khóa linh hoạt (Tên nhân viên, Username, Email). */
        Task<IEnumerable<AccountDto>> SearchAsync(string keyword);

        /** Khởi tạo tài khoản mới cho nhân sự kèm các ràng buộc chống trùng lặp dữ liệu hệ thống (UC05.3). */
        Task CreateAsync(CreateAccountDto dto);

        /** Hiệu chỉnh thông tin tài khoản nhân sự, phân lại vai trò hoặc khóa/mở khóa tài khoản (UC05.2). */
        Task UpdateAsync(UpdateAccountDto dto);

        /** Xóa vật lý tài khoản nhân sự ra khỏi hệ thống cơ sở dữ liệu. */
        Task DeleteAsync(int id);
    }
}