using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Hotel_System.Data;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_System.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _repo;

        public AccountService(IAccountRepository repo) => _repo = repo;

        public async Task<IEnumerable<AccountDto>> GetAllAsync()
        {
            var users = await _repo.GetAllAsync();
            return users.Select(ToDto);
        }

        public async Task<AccountDto?> GetByIdAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id);
            return user == null ? null : ToDto(user);
        }

        public async Task<IEnumerable<AccountDto>> SearchAsync(string keyword)
        {
            var users = await _repo.SearchAsync(keyword);
            return users.Select(ToDto);
        }

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
                PasswordHash = HashPassword(dto.Password),
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(user);
        }

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

        public async Task DeleteAsync(int id)
        {
            var user = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Account not found");

            await _repo.DeleteAsync(id);
        }

        // ===== Private Helpers =====
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

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}