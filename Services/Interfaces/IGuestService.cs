using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IGuestService
    {
        Task<List<GuestListDto>> GetAllAsync(string? search);
        Task<GuestDetailDto?> GetDetailAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(CreateGuestDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateGuestDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}