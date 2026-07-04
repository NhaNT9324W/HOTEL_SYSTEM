using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _repo;
        public RoomTypeService(IRoomTypeRepository repo) => _repo = repo;

        public async Task<List<RoomTypeDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<RoomTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto)
        {
            var entity = new RoomType
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                MaxOccupancy = dto.MaxOccupancy,
                IsActive = true
            };
            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            return MapToDto(entity);
        }

        public async Task<bool> UpdateAsync(int id, UpdateRoomTypeDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.BasePrice = dto.BasePrice;
            entity.MaxOccupancy = dto.MaxOccupancy;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.Now;

            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsActive = false; // soft delete
            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        private static RoomTypeDto MapToDto(RoomType r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            BasePrice = r.BasePrice,
            MaxOccupancy = r.MaxOccupancy,
            IsActive = r.IsActive
        };
    }
}