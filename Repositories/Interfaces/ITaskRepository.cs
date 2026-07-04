using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<HousekeepingTask>> GetAllAsync();
        Task<HousekeepingTask?> GetByIdAsync(int id);
        Task<IEnumerable<HousekeepingTask>> SearchAsync(string keyword);
        Task<IEnumerable<HousekeepingTask>> GetByStaffIdAsync(int staffId);
        Task AddAsync(HousekeepingTask task);
        Task UpdateAsync(HousekeepingTask task);
        Task DeleteAsync(int id);
    }
}