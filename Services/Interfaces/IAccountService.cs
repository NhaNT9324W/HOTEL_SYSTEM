using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<AccountDto>> GetAllAsync();
        Task<AccountDto?> GetByIdAsync(int id);
        Task<IEnumerable<AccountDto>> SearchAsync(string keyword);
        Task CreateAsync(CreateAccountDto dto);
        Task UpdateAsync(UpdateAccountDto dto);
        Task DeleteAsync(int id);
    }
}