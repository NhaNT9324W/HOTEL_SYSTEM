using Hotel_System.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.13.I ITaskService Interface]
     * Giao diện quy định các phương thức quản lý tác vụ buồng phòng và điều phối xử lý sự cố bảo trì vật chất.
     * Làm hợp đồng cho TaskService thực thi, phục vụ trực tiếp cho phân hệ Phân công tác vụ (UC19), 
     * Xem việc được giao (UC19.1) và Nghiệm thu chất lượng phòng / Khai báo sự cố kỹ thuật (UC20).
     */
    public interface ITaskService
    {
        /** Lấy toàn bộ danh sách tác vụ dọn dẹp hệ thống kèm thông tin nhân sự và số phòng vật lý (UC19). */
        Task<IEnumerable<TaskDto>> GetAllAsync();

        /** Truy xuất thông tin cấu trúc chi tiết của một tác vụ buồng phòng cụ thể dựa trên ID. */
        Task<TaskDto?> GetByIdAsync(int id);

        /** Tra cứu nâng cao danh sách tác vụ theo từ khóa linh hoạt (Số phòng, Tên nhân viên, Nội dung yêu cầu). */
        Task<IEnumerable<TaskDto>> SearchAsync(string keyword);

        /** Lấy danh sách tác vụ được phân công riêng cho một nhân viên phục vụ phòng thực địa (UC19.1). */
        Task<IEnumerable<TaskDto>> GetByStaffIdAsync(int staffId);

        /** Khởi tạo một tác vụ dọn dẹp hoặc sửa chữa buồng phòng mới với trạng thái mặc định ban đầu là Pending (UC19). */
        Task CreateAsync(CreateTaskDto dto);

        /** Hiệu chỉnh nội dung phân công tác vụ buồng phòng, ràng buộc nghiệp vụ chỉ cho phép sửa khi tác vụ ở trạng thái Pending. */
        Task UpdateAsync(UpdateTaskDto dto);

        /** Xóa vật lý bản ghi tác vụ dọn dẹp, ràng buộc nghiệp vụ chỉ cho phép loại bỏ các công việc chưa được tiến hành (Pending). */
        Task DeleteAsync(int id);

        /** Quản lý máy trạng thái (State Machine) cập nhật tiến độ hoàn thành công việc của nhân viên buồng phòng (Pending -> InProgress -> Completed). */
        Task UpdateTaskStatusAsync(int taskId, string status);

        /** Cập nhật trạng thái buồng phòng vật lý (READY, DIRTY, CLEANING) để đồng bộ dữ liệu vận hành cho bộ phận Tiền sảnh. */
        Task UpdateRoomStatusAsync(int roomId, string housekeepingStatus);

        /** Ghi nhận khai báo báo cáo sự cố kỹ thuật hỏng hóc vật chất (Điện, nước, thiết bị) phát hiện trong kỳ dọn phòng (UC20). */
        Task<bool> ReportMaintenanceAsync(ReportMaintenanceDto dto);

        /** Lấy lịch sử danh sách các sự cố kỹ thuật vật chất do một nhân viên cụ thể phát hiện và khai báo báo cáo lên hệ thống. */
        Task<IEnumerable<MaintenanceIssueDto>> GetMaintenanceIssuesByStaffAsync(int staffId);

        /** Lấy toàn bộ danh sách sự cố hỏng hóc thiết bị buồng phòng toàn hệ thống phục vụ công tác điều phối kỹ sư sửa chữa (UC20.1). */
        Task<IEnumerable<MaintenanceIssueDto>> GetAllMaintenanceIssuesAsync();

        /** Cập nhật trạng thái tiến độ xử lý sửa chữa thiết bị kỹ thuật vật chất (PENDING, IN_PROGRESS, RESOLVED). */
        Task UpdateMaintenanceStatusAsync(int id, string status);
    }
}