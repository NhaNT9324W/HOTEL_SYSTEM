using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repo;

        public TaskService(ITaskRepository repo) => _repo = repo;

        public async Task<IEnumerable<TaskDto>> GetAllAsync()
        {
            var tasks = await _repo.GetAllAsync();
            return tasks.Select(ToDto);
        }

        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id);
            return task == null ? null : ToDto(task);
        }

        public async Task<IEnumerable<TaskDto>> SearchAsync(string keyword)
        {
            var tasks = await _repo.SearchAsync(keyword);
            return tasks.Select(ToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetByStaffIdAsync(int staffId)
        {
            var tasks = await _repo.GetByStaffIdAsync(staffId);
            return tasks.Select(ToDto);
        }

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
                Status = HousekeepingTaskStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(task);
        }

        public async Task UpdateAsync(UpdateTaskDto dto)
        {
            var task = await _repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Task not found");

            if (task.Status != HousekeepingTaskStatus.Pending)
                throw new Exception("Only pending tasks can be edited");

            task.RoomId = dto.RoomId;
            task.AssignedToId = dto.AssignedToId;
            task.TaskType = dto.TaskType;
            task.Priority = dto.Priority;
            task.Description = dto.Description;
            task.DueDate = dto.DueDate;

            await _repo.UpdateAsync(task);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Task not found");

            if (task.Status != HousekeepingTaskStatus.Pending)
                throw new Exception("Only pending tasks can be deleted");

            await _repo.DeleteAsync(id);
        }

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
    }
}