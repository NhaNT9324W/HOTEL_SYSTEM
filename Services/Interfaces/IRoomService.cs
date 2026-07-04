using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IRoomService
    {
        Task<List<RoomDto>> GetAllAsync();
        Task<RoomDto?> GetByIdAsync(int id);
        Task<(bool Success, string? Error, RoomDto? Data)> CreateAsync(CreateRoomDto dto);
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRoomDto dto);
        Task<bool> DeleteAsync(int id);
    }
}