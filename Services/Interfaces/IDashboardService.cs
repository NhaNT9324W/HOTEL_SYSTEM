using Hotel_System.DTOs;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.4.I IDashboardService Interface]
     * Giao diện quy định các phương thức tổng hợp dữ liệu thống kê (Dashboard) thời gian thực.
     * Khai báo các hợp đồng dịch vụ nhằm cung cấp chỉ số vận hành và kinh doanh cốt lõi được cá nhân hóa theo từng vai trò (Role).
     */
    public interface IDashboardService
    {
        /** Tổng hợp số liệu hệ thống dành cho Quản trị viên phục vụ giám sát tài khoản nhân sự và tài nguyên hệ thống (UC05). */
        Task<AdminDashboardDto> GetAdminDashboardAsync();

        /** Tổng hợp dữ liệu kinh doanh cho bộ phận Quản lý gồm công suất phòng, doanh thu và thống kê tồn đọng (UC21). */
        Task<ManagerDashboardDto> GetManagerDashboardAsync();

        /** Tổng hợp số liệu vận hành Tiền sảnh trong ngày phục vụ Lễ tân điều phối luồng nhận/trả phòng (UC13/UC14). */
        Task<ReceptionistDashboardDto> GetReceptionistDashboardAsync();

        /** Tổng hợp danh sách và tiến độ công việc được phân gán riêng cho từng nhân viên buồng phòng thực địa (UC19.1). */
        Task<RoomStaffDashboardDto> GetRoomStaffDashboardAsync(int staffId);
    }
}