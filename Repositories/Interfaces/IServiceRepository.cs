using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<IEnumerable<Service>> GetAllAsync();
        Task<Service?> GetByIdAsync(int id);
        Task<IEnumerable<Service>> SearchAsync(string keyword);
        Task<bool> IsNameExistsAsync(string serviceName);
        Task<int> CountUsageAsync(int serviceId);
        Task AddAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(int id);
    }
}