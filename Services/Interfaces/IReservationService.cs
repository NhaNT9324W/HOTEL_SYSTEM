using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationListDto>> GetAllAsync(string? search);
        Task<ReservationDetailDto?> GetDetailAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(CreateReservationDto dto);
    }
}