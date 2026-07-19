using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.11.I IRoomTypeService Interface]
     * Giao diện quy định các phương thức cấu hình danh mục và chính sách hạng phòng khách sạn.
     * Làm hợp đồng cho RoomTypeService thực thi, phục vụ trực tiếp phân hệ Quản lý hạng phòng (UC09), làm cơ sở thiết lập biểu giá sàn và sức chứa cho phòng vật lý.
     */
    public interface IRoomTypeService
    {
        /** Lấy toàn bộ danh sách hạng phòng hệ thống dưới dạng DTO để hiển thị lên bảng danh mục quản trị (UC09.1). */
        Task<List<RoomTypeDto>> GetAllAsync();

        /** Tìm thông tin chi tiết một hạng phòng theo ID phục vụ xem hoặc chuẩn bị hiệu chỉnh cấu hình (UC09.2). */
        Task<RoomTypeDto?> GetByIdAsync(int id);

        /** Khởi tạo cấu hình hạng phòng mới vào hệ thống (UC09.3), thiết lập các thuộc tính cơ bản như biểu giá sàn và sức chứa tối đa. */
        Task<RoomTypeDto> CreateAsync(CreateRoomTypeDto dto);

        /** Hiệu chỉnh các thông số hạng phòng (tên, mô tả, giá niêm yết, sức chứa) và cập nhật trạng thái hoạt động trên hệ thống. */
        Task<bool> UpdateAsync(int id, UpdateRoomTypeDto dto);

        /** Áp dụng cơ chế ngắt kích hoạt/xóa mềm (Soft Delete) hạng phòng để bảo toàn tính toàn vẹn cho các phòng vật lý đang liên kết. */
        Task<bool> SoftDeleteAsync(int id);
    }
}