using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.7.I IServiceRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể Service (Dịch vụ khách sạn).
     * Phục vụ phân hệ Quản lý danh mục dịch vụ (UC10) và tra cứu khi ghi nhận tiêu dùng tại quầy (UC17).
     */
    public interface IServiceRepository
    {
        /** Lấy toàn bộ danh sách tất cả dịch vụ phục vụ màn hình quản trị danh mục (UC10.1). */
        Task<IEnumerable<Service>> GetAllAsync();

        /** Tìm chi tiết dịch vụ theo ID để phục vụ hiển thị hoặc chuẩn bị chỉnh sửa (UC10.2). */
        Task<Service?> GetByIdAsync(int id);

        /** Tìm kiếm nâng cao danh sách dịch vụ theo từ khóa linh hoạt (Tên dịch vụ, Mô tả). */
        Task<IEnumerable<Service>> SearchAsync(string keyword);

        /** Kiểm tra trùng tên dịch vụ (BR-35) trước khi thêm mới danh mục hệ thống. */
        Task<bool> IsNameExistsAsync(string serviceName);

        /** Đếm số lượt tiêu dùng lịch sử để ràng buộc điều kiện an toàn, chống xóa nhầm dữ liệu kế toán. */
        Task<int> CountUsageAsync(int serviceId);

        /** Thêm mới một bản ghi dịch vụ và lưu trực tiếp xuống cơ sở dữ liệu (UC10.3). */
        Task AddAsync(Service service);

        /** Cập nhật thông tin dịch vụ (giá niêm yết, trạng thái), tự động ghi vết UpdatedAt. */
        Task UpdateAsync(Service service);

        /** Xóa vật lý bản ghi dịch vụ khỏi hệ thống dựa trên ID. */
        Task DeleteAsync(int id);
    }
}