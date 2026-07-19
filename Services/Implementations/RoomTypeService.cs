using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.11 RoomTypeService Implementation]
     * Lớp xử lý nghiệp vụ cấu hình danh mục và chính sách hạng phòng khách sạn.
     * Phục vụ trực tiếp phân hệ Quản lý hạng phòng (UC09), làm cơ sở thiết lập biểu giá sàn và sức chứa cho phòng vật lý.
     */
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _repo;

        /** Inject Repository quản lý thông tin hạng phòng thông qua Constructor. */
        public RoomTypeService(IRoomTypeRepository repo) => _repo = repo;

        /** Lấy toàn bộ danh sách hạng phòng hệ thống dưới dạng DTO để hiển thị lên bảng danh mục quản trị (UC09.1). */
        public async Task<List<RoomTypeDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        /** Tìm thông tin chi tiết một hạng phòng theo ID phục vụ xem hoặc chuẩn bị hiệu chỉnh cấu hình (UC09.2). */
        public async Task<RoomTypeDto?> GetByIdAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            return entity == null ? null : MapToDto(entity);
        }

        /** Khởi tạo cấu hình hạng phòng mới vào hệ thống (UC09.3), mặc định đặt trạng thái kích hoạt hoạt động (IsActive = true). */
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

        /** Hiệu chỉnh các thông số hạng phòng (tên, mô tả, giá niêm yết, sức chứa) và tự động ghi vết thời gian sửa đổi UpdateAt. */
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

        /** Áp dụng cơ chế ngắt kích hoạt/xóa mềm (Soft Delete) hạng phòng để bảo toàn tính toàn vẹn cho các phòng vật lý đang liên kết. */
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            entity.IsActive = false; // Đánh dấu ngắt hoạt động thay vì xóa cứng khỏi cơ sở dữ liệu
            _repo.Update(entity);
            return await _repo.SaveChangesAsync();
        }

        // ===== PRIVATE HELPERS =====
        /** Hàm tiện ích ánh xạ (Mapping) dữ liệu phẳng từ thực thể RoomType sang cấu trúc RoomTypeDto an toàn. */
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