using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<List<Reservation>> GetAllAsync(string? search);
        Task<Reservation?> GetByIdAsync(int id);
        Task<Room?> GetRoomWithStatusAsync(int roomId);
        Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut);
        Task AddAsync(Reservation reservation);
        Task<Guest?> FindGuestByPhoneAsync(string phone);
        Task AddGuestAsync(Guest guest);
        Task SaveChangesAsync();
    }
}