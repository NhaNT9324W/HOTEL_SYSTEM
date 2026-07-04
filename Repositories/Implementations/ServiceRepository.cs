using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        public ServiceRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Service>> GetAllAsync() =>
            await _context.Services.ToListAsync();

        public async Task<Service?> GetByIdAsync(int id) =>
            await _context.Services.FindAsync(id);

        public async Task<IEnumerable<Service>> SearchAsync(string keyword) =>
            await _context.Services
                .Where(s => s.ServiceName.Contains(keyword) ||
                            s.Description.Contains(keyword))
                .ToListAsync();

        public async Task<bool> IsNameExistsAsync(string serviceName) =>
            await _context.Services
                .AnyAsync(s => s.ServiceName == serviceName);

        public async Task<int> CountUsageAsync(int serviceId) =>
            // TODO: đếm khi có ServiceUsage table
            await Task.FromResult(0);

        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Service service)
        {
            service.UpdatedAt = DateTime.UtcNow;
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}