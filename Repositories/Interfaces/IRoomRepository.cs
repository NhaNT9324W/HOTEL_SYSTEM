using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(int id);
        Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null);
        Task AddAsync(Room entity);
        void Update(Room entity);
        void Delete(Room entity);
        Task<bool> SaveChangesAsync();
    }
}