using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Account>> GetAllAsync() =>
            await _context.Accounts.ToListAsync();

        public async Task<Account?> GetByIdAsync(int id) =>
            await _context.Accounts.FindAsync(id);

        public async Task<Account?> GetByUsernameAsync(string username) =>
            await _context.Accounts
                .FirstOrDefaultAsync(u => u.Username == username);

        public async Task<IEnumerable<Account>> SearchAsync(string keyword) =>
            await _context.Accounts
                .Where(u => u.FullName.Contains(keyword) ||
                            u.Username.Contains(keyword) ||
                            u.Email.Contains(keyword))
                .ToListAsync();

        public async Task<bool> IsUsernameExistsAsync(string username) =>
            await _context.Accounts.AnyAsync(u => u.Username == username);

        public async Task<bool> IsEmailExistsAsync(string email) =>
            await _context.Accounts.AnyAsync(u => u.Email == email);

        public async Task AddAsync(Account user)
        {
            await _context.Accounts.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Accounts.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Accounts.FindAsync(id);
            if (user != null)
            {
                _context.Accounts.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}