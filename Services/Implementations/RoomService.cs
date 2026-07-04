using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repo;
        private readonly IRoomTypeRepository _roomTypeRepo;

        public RoomService(IRoomRepository repo, IRoomTypeRepository roomTypeRepo)
        {
            _repo = repo;
            _roomTypeRepo = roomTypeRepo;
        }

        public async Task<List<RoomDto>> GetAllAsync()
        {
            var rooms = await _repo.GetAllAsync();
            return rooms.Select(MapToDto).ToList();
        }

        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var room = await _repo.GetByIdAsync(id);
            return room == null ? null : MapToDto(room);
        }

        public async Task<(bool, string?, RoomDto?)> CreateAsync(CreateRoomDto dto)
        {
            if (await _repo.RoomNumberExistsAsync(dto.RoomNumber))
                return (false, "Số phòng đã tồn tại.", null);

            var roomType = await _roomTypeRepo.GetByIdAsync(dto.RoomTypeId);
            if (roomType == null)
                return (false, "Loại phòng không tồn tại.", null);

            var entity = new Room
            {
                RoomNumber = dto.RoomNumber,
                Floor = dto.Floor,
                RoomTypeId = dto.RoomTypeId
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            entity.RoomType = roomType;
            return (true, null, MapToDto(entity));
        }

        public async Task<(bool, string?)> UpdateAsync(int id, UpdateRoomDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return (false, "Không tìm thấy phòng.");

            if (await _repo.RoomNumberExistsAsync(dto.RoomNumber, id))
                return (false, "Số phòng đã tồn tại.");

            var roomType = await _roomTypeRepo.GetByIdAsync(dto.RoomTypeId);
            if (roomType == null) return (false, "Loại phòng không tồn tại.");

            entity.RoomNumber = dto.RoomNumber;
            entity.Floor = dto.Floor;
            entity.RoomTypeId = dto.RoomTypeId;
            entity.BookingStatus = dto.BookingStatus;
            entity.HousekeepingStatus = dto.HousekeepingStatus;
            entity.UpdatedAt = DateTime.Now;

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            _repo.Delete(entity); // Room không có IsActive nên xóa cứng; có thể đổi sang soft-delete nếu cần
            return await _repo.SaveChangesAsync();
        }

        private static RoomDto MapToDto(Room r) => new()
        {
            Id = r.Id,
            RoomNumber = r.RoomNumber,
            Floor = r.Floor,
            RoomTypeId = r.RoomTypeId,
            RoomTypeName = r.RoomType?.Name ?? "",
            BasePrice = r.RoomType?.BasePrice ?? 0,
            MaxOccupancy = r.RoomType?.MaxOccupancy ?? 0,
            BookingStatus = r.BookingStatus,
            HousekeepingStatus = r.HousekeepingStatus
        };
    }
}