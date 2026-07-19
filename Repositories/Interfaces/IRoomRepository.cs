using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.5.I IRoomRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể Room (Phòng vật lý).
     * Phục vụ trực tiếp sơ đồ phòng thời gian thực, phân hệ Quản lý phòng (UC07), Check-in (UC14) và Buồng phòng (UC19).
     */
    public interface IRoomRepository
    {
        /** Lấy toàn bộ danh sách phòng vật lý kèm hạng phòng để sắp xếp lên sơ đồ Room Matrix. */
        Task<List<Room>> GetAllAsync();

        /** Tìm thông tin chi tiết một phòng vật lý theo mã định danh Primary Key. */
        Task<Room?> GetByIdAsync(int id);

        /** Kiểm tra trùng lặp số phòng (BR-05) khi thêm mới hoặc hiệu chỉnh cấu hình phòng. */
        Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null);

        /** Thêm mới một phòng vật lý vào ngữ cảnh theo dõi (Change Tracker) của hệ thống (UC07). */
        Task AddAsync(Room entity);

        /** Đánh dấu thực thể phòng vật lý đã thay đổi thông tin để chuẩn bị cập nhật. */
        void Update(Room entity);

        /** Xóa vật lý bản ghi phòng khỏi hệ thống nếu không vướng ràng buộc khóa ngoại lịch sử. */
        void Delete(Room entity);

        /** Xác nhận và chính thức đồng bộ toàn bộ thay đổi (Thêm, Sửa, Xóa) xuống cơ sở dữ liệu. */
        Task<bool> SaveChangesAsync();
    }
}