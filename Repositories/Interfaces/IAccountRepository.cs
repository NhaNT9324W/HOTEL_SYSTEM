using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAsync();
        Task<Account?> GetByIdAsync(int id);
        Task<Account?> GetByUsernameAsync(string username);
        Task<IEnumerable<Account>> SearchAsync(string keyword);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> IsEmailExistsAsync(string email);
        Task AddAsync(Account user);
        Task UpdateAsync(Account user);
        Task DeleteAsync(int id);
    }
}