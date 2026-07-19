using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.2.I IGuestRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể Guest (Khách lưu trú).
     * Phục vụ phân hệ Quản lý hồ sơ khách hàng (UC15) và quy trình Đặt phòng tiền sảnh (UC13).
     */
    public interface IGuestRepository
    {
        /** Lấy danh sách khách hàng chưa xóa mềm, hỗ trợ bộ lọc tìm kiếm tương đối (UC15). */
        Task<List<Guest>> GetAllAsync(string? search);

        /** Tìm kiếm thông tin chi tiết một khách hàng theo mã định danh Primary Key. */
        Task<Guest?> GetByIdAsync(int id);

        /** Tìm khách hàng theo số điện thoại, hỗ trợ kiểm tra ràng buộc duy nhất BR-22 khi tạo/sửa. */
        Task<Guest?> GetByPhoneAsync(string phone, int? excludeId = null);

        /** Đếm tổng số đơn đặt phòng của khách để thống kê hoặc kiểm tra điều kiện ràng buộc. */
        Task<int> CountReservationsAsync(int guestId);

        /** Thêm mới hồ sơ khách hàng vào ngữ cảnh theo dõi (Change Tracker). */
        Task AddAsync(Guest guest);

        /** Đánh dấu thực thể khách hàng đã thay đổi thông tin để chuẩn bị cập nhật. */
        void Update(Guest guest);

        /** Thực hiện xóa mềm hồ sơ, bảo toàn tính toàn vẹn dữ liệu cho lịch sử hóa đơn. */
        void SoftDelete(Guest guest);

        /** Xác nhận và chính thức đồng bộ toàn bộ thay đổi xuống cơ sở dữ liệu vật lý. */
        Task SaveChangesAsync();
    }
}