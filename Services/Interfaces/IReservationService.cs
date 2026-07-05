using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationListDto>> GetAllAsync();
        Task<ReservationDetailDto?> GetByIdAsync(int id);
        Task<IEnumerable<ReservationListDto>> SearchAsync(string keyword);
        Task<IEnumerable<object>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
        Task CreateAsync(CreateReservationDto dto);
        Task UpdateAsync(int id, UpdateReservationDto dto);
        Task CancelAsync(int id);
    }
}