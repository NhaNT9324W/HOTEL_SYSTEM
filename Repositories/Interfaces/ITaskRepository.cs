using Hotel_System.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.8.I ITaskRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho HousekeepingTask (Tác vụ buồng phòng/bảo trì).
     * Phục vụ phân hệ Phân công tác vụ (UC19), Xem việc được giao (UC19.1) và Nghiệm thu chất lượng phòng (UC20).
     */
    public interface ITaskRepository
    {
        /** Lấy toàn bộ danh sách tác vụ buồng phòng kèm thông tin Room và Staff để quản lý (UC19). */
        Task<IEnumerable<HousekeepingTask>> GetAllAsync();

        /** Tìm chi tiết tác vụ theo ID để phục vụ hiển thị chi tiết hoặc xử lý tiến độ công việc. */
        Task<HousekeepingTask?> GetByIdAsync(int id);

        /** Tìm kiếm nâng cao danh sách tác vụ theo từ khóa linh hoạt (Số phòng, Tên nhân viên, Mô tả). */
        Task<IEnumerable<HousekeepingTask>> SearchAsync(string keyword);

        /** Lấy danh sách tác vụ được phân gán riêng cho một nhân viên buồng phòng thực địa (UC19.1). */
        Task<IEnumerable<HousekeepingTask>> GetByStaffIdAsync(int staffId);

        /** Thêm mới một tác vụ dọn dẹp hoặc bảo trì phòng và lưu trực tiếp xuống cơ sở dữ liệu. */
        Task AddAsync(HousekeepingTask task);

        /** Cập nhật thông tin tác vụ (thay đổi nhân sự, tiến độ, độ ưu tiên), tự động ghi vết UpdatedAt. */
        Task UpdateAsync(HousekeepingTask task);

        /** Xóa vật lý bản ghi tác vụ khỏi hệ thống dựa trên ID. */
        Task DeleteAsync(int id);
    }
}