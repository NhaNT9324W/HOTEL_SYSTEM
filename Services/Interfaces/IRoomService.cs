using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.10.I IRoomService Interface]
     * Giao diện quy định các phương thức quản lý danh mục và trạng thái kinh doanh của phòng vật lý.
     * Làm hợp đồng cho RoomService thực thi, phục vụ phân hệ Quản lý phòng (UC07) và đồng bộ sơ đồ lưới Room Matrix thời gian thực.
     */
    public interface IRoomService
    {
        /** Lấy toàn bộ danh sách phòng kèm chi tiết hạng phòng dưới dạng DTO để hiển thị trực quan lên sơ đồ trạng thái (UC07.1). */
        Task<List<RoomDto>> GetAllAsync();

        /** Tra cứu cấu trúc chi tiết của một phòng vật lý cụ thể dựa trên mã định danh Primary Key. */
        Task<RoomDto?> GetByIdAsync(int id);

        /** 
         * Khởi tạo phòng vật lý mới vào danh mục hệ thống (UC07.3).
         * Áp dụng các bộ lọc ràng buộc nghiệp vụ (BR) kiểm tra trùng lặp số phòng và tính hợp lệ của hạng phòng liên kết.
         */
        Task<(bool Success, string? Error, RoomDto? Data)> CreateAsync(CreateRoomDto dto);

        /** 
         * Hiệu chỉnh thông tin cấu hình phòng hoặc cập nhật trạng thái kinh doanh/buồng phòng (UC07.2).
         * Tiến hành đối chiếu loại trừ trùng số phòng với các phòng khác và tự động cập nhật mốc thời gian sửa đổi.
         */
        Task<(bool Success, string? Error)> UpdateAsync(int id, UpdateRoomDto dto);

        /** 
         * Xóa vật lý bản ghi phòng ra khỏi hệ thống cơ cơ sở dữ liệu.
         * Hệ thống sẽ chặn hành động này nếu phòng đã phát sinh dữ liệu lịch sử đặt phòng (Reservation) hoặc hóa đơn nhằm bảo toàn toàn vẹn dữ liệu.
         */
        Task<bool> DeleteAsync(int id);
    }
}