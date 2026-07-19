using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.6.I IGuestService Interface]
     * Giao diện quy định các phương thức quản lý nghiệp vụ hồ sơ và lịch sử khách lưu trú.
     * Làm hợp đồng cho GuestService thực thi, phục vụ trực tiếp phân hệ Quản lý hồ sơ khách hàng (UC15) và hỗ trợ thông tin gốc cho quy trình Đặt phòng (UC13).
     */
    public interface IGuestService
    {
        /** Lấy danh sách khách hàng gọn nhẹ chưa xóa mềm, hỗ trợ bộ lọc tra cứu nhanh tại giao diện (UC15). */
        Task<List<GuestListDto>> GetAllAsync(string? search);

        /** Xem thông tin hồ sơ chi tiết của khách kèm theo tổng số lượt đặt phòng lịch sử để đánh giá độ thân thiết. */
        Task<GuestDetailDto?> GetDetailAsync(int id);

        /** Khởi tạo hồ sơ khách hàng mới kèm ràng buộc nghiệp vụ kiểm tra trùng lặp Số điện thoại toàn hệ thống (UC15.1). */
        Task<(bool Success, string Message)> CreateAsync(CreateGuestDto dto);

        /** Hiệu chỉnh thông tin hồ sơ khách hàng, thực hiện đối chiếu trùng SĐT ngoại trừ mã định danh của chính khách hàng hiện tại. */
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateGuestDto dto);

        /** Áp dụng cơ chế Xóa mềm (Soft Delete) để ẩn hồ sơ khách khỏi danh mục hiển thị nhưng vẫn bảo toàn lịch sử giao dịch/đặt phòng (UC15). */
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }
}