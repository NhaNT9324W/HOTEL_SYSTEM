using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.10 RoomService Implementation]
     * Lớp xử lý nghiệp vụ quản lý danh mục và trạng thái phòng vật lý.
     * Phục vụ trực tiếp phân hệ Quản lý phòng (UC07), hỗ trợ hiển thị sơ đồ Room Matrix thời gian thực và đồng bộ trạng thái lưu trú (UC14, UC18).
     */
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repo;
        private readonly IRoomTypeRepository _roomTypeRepo;

        /** Inject các Repository quản lý thông tin phòng và hạng phòng thông qua Constructor. */
        public RoomService(IRoomRepository repo, IRoomTypeRepository roomTypeRepo)
        {
            _repo = repo;
            _roomTypeRepo = roomTypeRepo;
        }

        /** Lấy toàn bộ danh sách phòng kèm thông tin chi tiết hạng phòng dưới dạng DTO để hiển thị lên sơ đồ lưới (UC07.1). */
        public async Task<List<RoomDto>> GetAllAsync()
        {
            var rooms = await _repo.GetAllAsync();
            return rooms.Select(MapToDto).ToList();
        }

        /** Tìm thông tin chi tiết một phòng vật lý theo mã định danh Primary Key để phục vụ xem hoặc hiệu chỉnh. */
        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var room = await _repo.GetByIdAsync(id);
            return room == null ? null : MapToDto(room);
        }

        /** 
         * Khởi tạo phòng vật lý mới vào hệ thống danh mục (UC07.3).
         * Ràng buộc nghiệp vụ (BR-05): Kiểm tra trùng lặp số phòng và bảo đảm hạng phòng liên kết phải tồn tại hợp lệ.
         */
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

        /** 
         * Hiệu chỉnh thông tin cấu hình phòng hoặc cập nhật thủ công trạng thái kinh doanh/buồng phòng (UC07.2).
         * Thực hiện kiểm tra trùng số phòng ngoại trừ ID hiện tại và tự động ghi vết thời gian đồng bộ `UpdatedAt`.
         */
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
            entity.UpdatedAt = DateTime.UtcNow; // Đồng bộ sử dụng giờ chuẩn UTC toàn hệ thống

            _repo.Update(entity);
            await _repo.SaveChangesAsync();
            return (true, null);
        }

        /** 
         * Xóa vật lý bản ghi phòng ra khỏi hệ thống cơ sở dữ liệu.
         * Chỉ thực hiện thành công nếu phòng chưa phát sinh lịch sử giao dịch khóa ngoại (Folio/Reservation) để bảo toàn tính toàn vẹn DB.
         */
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            _repo.Delete(entity);
            return await _repo.SaveChangesAsync();
        }

        // ===== PRIVATE HELPERS =====
        /** Hàm tiện ích ánh xạ (Mapping) dữ liệu phẳng từ thực thể Room sang cấu trúc RoomDto an toàn. */
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