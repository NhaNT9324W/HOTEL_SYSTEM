using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByUsernameAsync(string username);
        Task<Account?> GetByEmailAsync(string email);
        Task<Account?> GetByResetTokenAsync(string token);
        Task<IEnumerable<Account>> SearchAsync(string keyword);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int id);
    }
}