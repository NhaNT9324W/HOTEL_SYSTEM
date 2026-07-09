using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Enums = Hotel_System.Entities.Enums;

namespace Hotel_System.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repo;
        private readonly AppDbContext _context;

        public TaskService(ITaskRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // ===== GET ALL =====
        public async Task<IEnumerable<TaskDto>> GetAllAsync()
        {
            var tasks = await _repo.GetAllAsync();
            return tasks.Select(ToDto);
        }

        // ===== GET BY ID =====
        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id);
            return task == null ? null : ToDto(task);
        }

        // ===== SEARCH =====
        public async Task<IEnumerable<TaskDto>> SearchAsync(string keyword)
        {
            var tasks = await _repo.SearchAsync(keyword);
            return tasks.Select(ToDto);
        }

        // ===== GET BY STAFF ID =====
        public async Task<IEnumerable<TaskDto>> GetByStaffIdAsync(int staffId)
        {
            var tasks = await _repo.GetByStaffIdAsync(staffId);
            return tasks.Select(ToDto);
        }

        // ===== CREATE =====
        public async Task CreateAsync(CreateTaskDto dto)
        {
            var task = new HousekeepingTask
            {
                RoomId = dto.RoomId,
                AssignedToId = dto.AssignedToId,
                CreatedById = dto.CreatedById,
                TaskType = dto.TaskType,
                Priority = dto.Priority,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = Enums.HousekeepingTaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(task);
        }

        // ===== UPDATE =====
        public async Task UpdateAsync(UpdateTaskDto dto)
        {
            var task = await _repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Task not found");

            if (task.Status != Enums.HousekeepingTaskStatus.Pending)
                throw new Exception("Only pending tasks can be edited");

            task.RoomId = dto.RoomId;
            task.AssignedToId = dto.AssignedToId;
            task.TaskType = dto.TaskType;
            task.Priority = dto.Priority;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _repo.UpdateAsync(task);
        }

        // ===== DELETE =====
        public async Task DeleteAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Task not found");

            if (task.Status != Enums.HousekeepingTaskStatus.Pending)
                throw new Exception("Only pending tasks can be deleted");

            await _repo.DeleteAsync(id);
        }

        // ===== UPDATE TASK STATUS =====
        public async Task UpdateTaskStatusAsync(int taskId, string status)
        {
            var task = await _repo.GetByIdAsync(taskId)
                ?? throw new Exception("Task not found");

            // Validate transition
            var validTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "InProgress" } },
                { "InProgress", new List<string> { "Completed" } },
                { "Completed", new List<string>() }
            };

            var currentStatus = task.Status.ToString();
            if (!validTransitions[currentStatus].Contains(status))
                throw new Exception($"Cannot transition from {currentStatus} to {status}");

            task.Status = Enum.Parse<Enums.HousekeepingTaskStatus>(status);
            if (status == "Completed")
                task.CompletedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(task);
        }

        // ===== UPDATE ROOM STATUS =====
        public async Task UpdateRoomStatusAsync(int roomId, string housekeepingStatus)
        {
            var room = await _context.Rooms.FindAsync(roomId)
                ?? throw new Exception("Room not found");

            room.HousekeepingStatus = Enum.Parse<RoomHousekeepingStatus>(housekeepingStatus);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        // ===== REPORT MAINTENANCE =====
        public async Task<bool> ReportMaintenanceAsync(ReportMaintenanceDto dto)
        {
            var issue = new MaintenanceIssue
            {
                RoomId = dto.RoomId,
                ReportedById = dto.ReportedById,
                IssueType = dto.IssueType,
                Description = dto.Description,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow
            };

            await _context.MaintenanceIssues.AddAsync(issue);
            await _context.SaveChangesAsync();
            return true;
        }

        // ===== GET MAINTENANCE ISSUES BY STAFF =====
        public async Task<IEnumerable<MaintenanceIssueDto>> GetMaintenanceIssuesByStaffAsync(
            int staffId)
        {
            return await _context.MaintenanceIssues
                .Include(m => m.Room)
                .Include(m => m.ReportedBy)
                .Where(m => m.ReportedById == staffId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MaintenanceIssueDto
                {
                    Id = m.Id,
                    RoomNumber = m.Room!.RoomNumber,
                    ReportedByName = m.ReportedBy!.FullName,
                    IssueType = m.IssueType,
                    Description = m.Description,
                    Status = m.Status,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        // ===== MAPPER =====
        private static TaskDto ToDto(HousekeepingTask t) => new()
        {
            Id = t.Id,
            RoomId = t.RoomId,
            RoomNumber = t.Room?.RoomNumber ?? "",
            AssignedToId = t.AssignedToId,
            AssignedToName = t.AssignedTo?.FullName ?? "",
            CreatedByName = t.CreatedBy?.FullName ?? "",
            TaskType = t.TaskType,
            Priority = t.Priority,
            Status = t.Status,
            Description = t.Description,
            DueDate = t.DueDate,
            CompletedAt = t.CompletedAt,
            CreatedAt = t.CreatedAt
        };

        public async Task<IEnumerable<MaintenanceIssueDto>> GetAllMaintenanceIssuesAsync()
        {
            return await _context.MaintenanceIssues
                .Include(m => m.Room)
                .Include(m => m.ReportedBy)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MaintenanceIssueDto
                {
                    Id = m.Id,
                    RoomNumber = m.Room!.RoomNumber,
                    ReportedByName = m.ReportedBy!.FullName,
                    IssueType = m.IssueType,
                    Description = m.Description,
                    Status = m.Status,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        public async Task UpdateMaintenanceStatusAsync(int id, string status)
        {
            var issue = await _context.MaintenanceIssues.FindAsync(id)
                ?? throw new Exception("Maintenance issue not found");

            issue.Status = status;
            issue.UpdatedAt = DateTime.UtcNow;
            _context.MaintenanceIssues.Update(issue);
            await _context.SaveChangesAsync();
        }
    }
}