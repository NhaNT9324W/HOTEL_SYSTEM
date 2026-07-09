using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetAllAsync();
        Task<TaskDto?> GetByIdAsync(int id);
        Task<IEnumerable<TaskDto>> SearchAsync(string keyword);
        Task<IEnumerable<TaskDto>> GetByStaffIdAsync(int staffId);
        Task CreateAsync(CreateTaskDto dto);
        Task UpdateAsync(UpdateTaskDto dto);
        Task DeleteAsync(int id);
        Task UpdateTaskStatusAsync(int taskId, string status);
        Task UpdateRoomStatusAsync(int roomId, string housekeepingStatus);
        Task<bool> ReportMaintenanceAsync(ReportMaintenanceDto dto);
        Task<IEnumerable<MaintenanceIssueDto>> GetMaintenanceIssuesByStaffAsync(int staffId);
        Task<IEnumerable<MaintenanceIssueDto>> GetAllMaintenanceIssuesAsync();
        Task UpdateMaintenanceStatusAsync(int id, string status);
    }
}