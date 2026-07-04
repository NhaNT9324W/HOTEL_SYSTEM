using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IRoomTypeService
    {
        Task<List<RoomTypeDto>> GetAllAsync();
        Task<RoomTypeDto?> GetByIdAsync(int id);
        Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto);
        Task<bool> UpdateAsync(int id, UpdateRoomTypeDto dto);
        Task<bool> SoftDeleteAsync(int id);
    }
}