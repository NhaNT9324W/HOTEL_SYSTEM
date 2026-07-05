using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<IEnumerable<Reservation>> SearchAsync(string keyword);
        Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeId = null);
        Task AddAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
        Task UpdateStatusAsync(int id, Entities.Enums.ReservationStatus status);
    }
}