using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
        Task<ManagerDashboardDto> GetManagerDashboardAsync();
        Task<ReceptionistDashboardDto> GetReceptionistDashboardAsync();
        Task<RoomStaffDashboardDto> GetRoomStaffDashboardAsync(int staffId);
    }
}