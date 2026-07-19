using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Enums = Hotel_System.Entities.Enums;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.13 TaskService Implementation]
     * Lớp xử lý nghiệp vụ quản lý tác vụ buồng phòng và điều phối sự cố bảo trì vật chất.
     * Phục vụ trực tiếp cho phân hệ Phân công tác vụ (UC19), Xem việc được giao (UC19.1) và Nghiệm thu chất lượng phòng (UC20).
     */
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _repo;
        private readonly AppDbContext _context;

        /** Inject Repository tác vụ buồng phòng và DbContext xử lý sự cố kỹ thuật thông qua Constructor. */
        public TaskService(ITaskRepository repo, AppDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // ===== GET ALL =====
        /** Lấy toàn bộ danh sách tác vụ dọn dẹp hệ thống kèm thông tin nhân sự và số phòng vật lý (UC19). */
        public async Task<IEnumerable<TaskDto>> GetAllAsync()
        {
            var tasks = await _repo.GetAllAsync();
            return tasks.Select(ToDto);
        }

        // ===== GET BY ID =====
        /** Truy xuất chi tiết một tác vụ buồng phòng cụ thể dựa trên ID. */
        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id);
            return task == null ? null : ToDto(task);
        }

        // ===== SEARCH =====
        /** Tra cứu nâng cao danh sách tác vụ theo từ khóa linh hoạt (Số phòng, Tên nhân viên, Nội dung yêu cầu). */
        public async Task<IEnumerable<TaskDto>> SearchAsync(string keyword)
        {
            var tasks = await _repo.SearchAsync(keyword);
            return tasks.Select(ToDto);
        }

        // ===== GET BY STAFF ID =====
        /** Lấy danh sách tác vụ được phân công riêng cho một nhân viên phục vụ phòng thực địa (UC19.1). */
        public async Task<IEnumerable<TaskDto>> GetByStaffIdAsync(int staffId)
        {
            var tasks = await _repo.GetByStaffIdAsync(staffId);
            return tasks.Select(ToDto);
        }

        // ===== CREATE =====
        /** Khởi tạo một tác vụ dọn dẹp hoặc sửa chữa buồng phòng mới, mặc định gán trạng thái ban đầu là `Pending`. */
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
        /** Hiệu chỉnh nội dung phân công tác vụ buồng phòng, ràng buộc nghiệp vụ: Chỉ cho phép sửa khi trạng thái là `Pending`. */
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
        /** Xóa vật lý bản ghi tác vụ dọn dẹp, ràng buộc nghiệp vụ: Chỉ cho phép loại bỏ các công việc chưa được tiến hành (`Pending`). */
        public async Task DeleteAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Task not found");

            if (task.Status != Enums.HousekeepingTaskStatus.Pending)
                throw new Exception("Only pending tasks can be deleted");

            await _repo.DeleteAsync(id);
        }

        // ===== UPDATE TASK STATUS =====
        /** 
         * Quản lý máy trạng thái (State Machine) tiến độ hoàn thành công việc của nhân viên buồng phòng.
         * Ràng buộc chặt chẽ luồng vòng đời tác vụ: Pending -> InProgress -> Completed. Tự động cập nhật `CompletedAt`.
         */
        public async Task UpdateTaskStatusAsync(int taskId, string status)
        {
            var task = await _repo.GetByIdAsync(taskId)
                ?? throw new Exception("Task not found");

            // Định nghĩa các bước chuyển trạng thái hợp lệ trên hệ thống
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
        /** Cập nhật thủ công hoặc tự động trạng thái buồng phòng vật lý (READY, DIRTY, CLEANING) phục vụ vận hành tiền sảnh. */
        public async Task UpdateRoomStatusAsync(int roomId, string housekeepingStatus)
        {
            var room = await _context.Rooms.FindAsync(roomId)
                ?? throw new Exception("Room not found");

            room.HousekeepingStatus = Enum.Parse<RoomHousekeepingStatus>(housekeepingStatus);
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        // ===== REPORT MAINTENANCE =====
        /** Ghi nhận báo cáo sự cố kỹ thuật hỏng hóc vật chất (Điện, nước, điều hòa) phát hiện trong quá trình dọn phòng (UC20). */
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
        /** Lấy lịch sử danh sách các sự cố kỹ thuật vật chất do một nhân viên cụ thể phát hiện và khai báo báo cáo. */
        public async Task<IEnumerable<MaintenanceIssueDto>> GetMaintenanceIssuesByStaffAsync(int staffId)
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

        // ===== GET ALL MAINTENANCE ISSUES =====
        /** Lấy toàn bộ danh sách sự cố hỏng hóc thiết bị buồng phòng phục vụ điều phối kỹ sư sửa chữa (UC20.1). */
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

        // ===== UPDATE MAINTENANCE STATUS =====
        /** Cập nhật tiến độ xử lý sửa chữa thiết bị (PENDING, IN_PROGRESS, RESOLVED) kèm ghi vết thời gian đồng bộ `UpdatedAt`. */
        public async Task UpdateMaintenanceStatusAsync(int id, string status)
        {
            var issue = await _context.MaintenanceIssues.FindAsync(id)
                ?? throw new Exception("Maintenance issue not found");

            issue.Status = status;
            issue.UpdatedAt = DateTime.UtcNow;
            _context.MaintenanceIssues.Update(issue);
            await _context.SaveChangesAsync();
        }

        // ===== MAPPER =====
        /** Hàm tiện ích nội bộ ánh xạ dữ liệu từ mô hình HousekeepingTask sang cấu trúc TaskDto an toàn bảo mật lớp nền. */
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