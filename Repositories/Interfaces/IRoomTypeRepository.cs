using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IRoomTypeRepository
    {
        Task<List<RoomType>> GetAllAsync();
        Task<RoomType?> GetByIdAsync(int id);
        Task AddAsync(RoomType entity);
        void Update(RoomType entity);
        void Delete(RoomType entity);
        Task<bool> SaveChangesAsync();
    }
}