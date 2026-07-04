using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IGuestRepository
    {
        Task<List<Guest>> GetAllAsync(string? search);
        Task<Guest?> GetByIdAsync(int id);
        Task<Guest?> GetByPhoneAsync(string phone, int? excludeId = null);
        Task<int> CountReservationsAsync(int guestId);
        Task AddAsync(Guest guest);
        void Update(Guest guest);
        void SoftDelete(Guest guest); // MỚI
        Task SaveChangesAsync();
    }
}