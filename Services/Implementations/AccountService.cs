using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.1 AccountService Implementation]
     * Lớp xử lý nghiệp vụ (Business Logic Layer) cho hệ thống tài khoản.
     * Phục vụ điều phối các luồng xác thực và quản lý thông tin nhân sự (UC01, UC02, UC05).
     */
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        /** Inject Repository thông qua Constructor để tương tác với tầng dữ liệu. */
        public AccountService(IAccountRepository repo) => _repo = repo;

        /** Lấy toàn bộ danh sách tài khoản hệ thống dưới dạng DTO để hiển thị trực quan (UC05.1). */
        public async Task<IEnumerable<AccountDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(ToDto);
        }

        /** Tìm kiếm tài khoản theo ID, trả về dữ liệu DTO hoặc null nếu không tồn tại. */
        public async Task<AccountDto?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user == null ? null : ToDto(user);
        }

        /** Tra cứu nhanh danh sách nhân sự theo từ khóa linh hoạt tại bộ lọc giao diện. */
        public async Task<IEnumerable<AccountDto>> SearchAsync(string keyword)
        {
            var users = await _repo.SearchAsync(keyword);
            return users.Select(ToDto);
        }

        /** 
         * Khởi tạo tài khoản nhân sự mới (UC05.3).
         * Ràng buộc nghiệp vụ: Kiểm tra trùng lặp Username/Email và mã hóa mật khẩu đầu vào.
         */
        public async Task CreateAsync(CreateAccountDto dto)
        {
            if (await _repo.IsUsernameExistsAsync(dto.Username))
                throw new Exception("Username already exists");

            if (await _repo.IsEmailExistsAsync(dto.Email))
                throw new Exception("Email already exists");

            var user = new Account
            {
                FullName = dto.FullName,
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password), // Mã hóa một chiều trước khi lưu
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);
        }

        /** Hiệu chỉnh thông tin hồ sơ, phân quyền hoặc trạng thái hoạt động của nhân sự (UC05.2). */
        public async Task UpdateAsync(UpdateAccountDto dto)
        {
            var user = await _repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Account not found");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Role = dto.Role;
            user.Status = dto.Status;

            await _repo.UpdateAsync(user);
        }

        /** Xóa vật lý tài khoản nhân sự ra khỏi hệ thống sau khi xác minh sự tồn tại. */
        public async Task DeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Account not found");

            await _repo.DeleteAsync(id);
        }

        // ===== Private Helpers =====

        /** Hàm tiện ích ánh xạ dữ liệu (Mapping) từ Entity sang DTO bảo mật dữ liệu lớp nền. */
        private static AccountDto ToDto(Account u) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Username = u.Username,
            Email = u.Email,
            Phone = u.Phone,
            Role = u.Role,
            Status = u.Status,
            CreatedAt = u.CreatedAt
        };

        /** Thực hiện cơ chế băm mật khẩu bằng thuật toán mã hóa SHA256 an toàn. */
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}