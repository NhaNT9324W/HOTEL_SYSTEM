using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.6.I IRoomTypeRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể RoomType (Hạng phòng).
     * Phục vụ trực tiếp cho phân hệ Quản lý hạng phòng (UC09) và làm cơ sở tham chiếu cho Đặt phòng (UC13).
     */
    public interface IRoomTypeRepository
    {
        /** Lấy toàn bộ danh sách hạng phòng hiện có, sắp xếp mới nhất lên đầu (UC09.1). */
        Task<List<RoomType>> GetAllAsync();

        /** Tìm thông tin chi tiết hạng phòng theo ID để hiển thị hoặc chỉnh sửa (UC09.2). */
        Task<RoomType?> GetByIdAsync(int id);

        /** Thêm mới một cấu hình hạng phòng vào ngữ cảnh theo dõi của hệ thống (UC09.3). */
        Task AddAsync(RoomType entity);

        /** Đánh dấu thực thể hạng phòng đã thay đổi thông tin để chuẩn bị cập nhật. */
        void Update(RoomType entity);

        /** Xóa vật lý bản ghi hạng phòng nếu không vướng ràng buộc khóa ngoại với phòng vật lý. */
        void Delete(RoomType entity);

        /** Xác nhận và chính thức đồng bộ toàn bộ thay đổi (Thêm, Sửa, Xóa) xuống cơ sở dữ liệu. */
        Task<bool> SaveChangesAsync();
    }
}