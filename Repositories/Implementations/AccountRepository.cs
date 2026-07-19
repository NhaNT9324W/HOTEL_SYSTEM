using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.1 AccountRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể Account.
     * Áp dụng mẫu kiến trúc Repository Pattern nhằm cô lập tầng Data Access Layer (DAL) khỏi Business Logic Layer (BLL).
     * Lớp này đóng vai trò trung tâm xử lý dữ liệu phục vụ Quy trình Đăng nhập (UC01), Đổi mật khẩu (UC02) và Quản lý tài khoản nhân sự (UC05).
     */
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public AccountRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các tài khoản nhân viên hiện có trong hệ thống.
         * Thường được sử dụng trong màn hình quản trị tổng quan của Admin (UC05.1).
         */
        public async Task<IEnumerable<Account>> GetAllAsync() =>
            await _context.Accounts.ToListAsync();

        /** 
         * Tìm kiếm một tài khoản cụ thể dựa trên mã định danh số nguyên (Primary Key).
         * Hỗ trợ đắc lực cho các tác vụ xem chi tiết hồ sơ hoặc chuẩn bị dữ liệu cho quy trình Sửa tài khoản (UC05.2).
         */
        public async Task<Account?> GetByIdAsync(int id) =>
            await _context.Accounts.FindAsync(id);

        /** 
         * Tìm kiếm tài khoản dựa trên tên đăng nhập (Username).
         * Đây là hàm xử lý cốt lõi phục vụ Business Rule BR-01 trong quy trình Xác thực Đăng nhập (UC01) để đối chiếu thông tin tài khoản.
         */
        public async Task<Account?> GetByUsernameAsync(string username) =>
            await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == username);

        /** 
         * Tìm kiếm nâng cao và lọc danh sách tài khoản theo từ khóa linh hoạt.
         * Hệ thống biên dịch biểu thức Lambda thành câu lệnh SQL LIKE để tìm kiếm tương đối trên cả 3 trường thông tin: FullName, Username, và Email.
         */
        public async Task<IEnumerable<Account>> SearchAsync(string keyword) =>
            await _context.Accounts
                .Where(u => u.FullName.Contains(keyword) ||
                            u.Username.Contains(keyword) ||
                            u.Email.Contains(keyword))
                .ToListAsync();

        /** Kiểm tra sự tồn tại của Username trên hệ thống nhằm phục vụ kiểm tra ràng buộc duy nhất (Unique Constraint) trước khi khởi tạo tài khoản mới. */
        public async Task<bool> IsUsernameExistsAsync(string username) =>
            await _context.Accounts.AnyAsync(u => u.Username == username);

        /** Kiểm tra sự tồn tại của Email trên hệ thống, đảm bảo không có hai nhân sự sử dụng chung một hộp thư điện tử trong phân hệ quản lý. */
        public async Task<bool> IsEmailExistsAsync(string email) =>
            await _context.Accounts.AnyAsync(u => u.Email == email);

        /** 
         * Thực hiện thêm mới một bản ghi tài khoản vào cơ sở dữ liệu.
         * Phương thức thực thi bất đồng bộ (Async) và gọi trực tiếp SaveChangesAsync để xác nhận giao dịch xuống hệ quản trị CSDL.
         */
        public async Task AddAsync(Account user)
        {
            await _context.Accounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        /** 
         * Cập nhật thông tin sửa đổi của tài khoản nhân viên.
         * Trước khi thực hiện đồng bộ, hệ thống tự động cập nhật thuộc tính hệ thống `UpdatedAt` theo chuẩn thời gian UTC 
         * nhằm phục vụ mục đích kiểm toán lịch sử dữ liệu (Data Auditing).
         */
        public async Task UpdateAsync(Account user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();
        }

        /** 
         * Xóa vật lý bản ghi tài khoản khỏi hệ thống dựa vào ID.
         * Hàm thực hiện tìm kiếm thực thể trước, nếu tồn tại bản ghi hợp lệ mới tiến hành lệnh Remove và lưu thay đổi xuống DB.
         */
        public async Task DeleteAsync(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user != null)
            {
                _context.Accounts.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        /** Tìm kiếm tài khoản thông qua Email chính thức, phục vụ cho luồng logic xác minh danh tính và khởi tạo quy trình Quên mật khẩu (UC03). */
        public async Task<Account?> GetByEmailAsync(string email) =>
            await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == email);

        /** Tìm kiếm tài khoản dựa trên mã Token khôi phục mật khẩu nhằm xác thực tính hợp lệ của liên kết đặt lại mật khẩu mà người dùng nhấn vào. */
        public async Task<Account?> GetByResetTokenAsync(string token) =>
            await _context.Accounts
                .FirstOrDefaultAsync(a => a.ResetToken == token);
    }
}