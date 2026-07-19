using Hotel_System.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC19 - Manage Housekeeping Tasks] / [UC19.1 - View Assigned Tasks]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) đại diện cho thông tin chi tiết đầy đủ của một tác vụ buồng phòng[cite: 1].
     * Phục vụ hiển thị danh sách tác vụ chung hoặc phân gán công việc chi tiết cho nhân viên Room Staff[cite: 1].
     */
    public class TaskDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int AssignedToId { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public TaskType TaskType { get; set; }
        public TaskPriority Priority { get; set; }
        public HousekeepingTaskStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /**
     * [UC19 - Manage Housekeeping Tasks]
     * DTO tiếp nhận dữ liệu đầu vào khi Quản lý (Hotel Manager) hoặc Admin khởi tạo một tác vụ dọn dẹp hoặc bảo trì mới[cite: 1].
     * Áp dụng thuộc tính Data Annotations để ràng buộc bắt buộc thông tin phòng, nhân viên thực hiện và loại tác vụ[cite: 1].
     */
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Staff is required")]
        public int AssignedToId { get; set; }

        [Required(ErrorMessage = "Task type is required")]
        public TaskType TaskType { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public TaskPriority Priority { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        public int CreatedById { get; set; }
    }

    /**
     * [UC19 - Manage Housekeeping Tasks]
     * DTO tiếp nhận dữ liệu yêu cầu chỉnh sửa, hiệu chỉnh thông tin phân gán của một tác vụ đang tồn tại trên hệ thống[cite: 1].
     */
    public class UpdateTaskDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Staff is required")]
        public int AssignedToId { get; set; }

        [Required(ErrorMessage = "Task type is required")]
        public TaskType TaskType { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public TaskPriority Priority { get; set; }

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }
    }

    /**
     * [UC19.3 - Update Task Status]
     * DTO tiếp nhận dữ liệu yêu cầu điều chỉnh trạng thái tiến độ của một tác vụ dọn dẹp buồng phòng từ Room Staff[cite: 1].
     * Tuân thủ quy trình chuyển đổi trạng thái Task Status Transition Logic: PENDING -> IN_PROGRESS -> COMPLETED[cite: 1].
     */
    public class UpdateTaskStatusDto
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = null!; // Pending, InProgress, Completed
    }

    /**
     * [UC19.2 - Update Room Status]
     * DTO truyền tải dữ liệu khi Nhân viên buồng phòng thực hiện cập nhật nhanh trạng thái vệ sinh vật lý của phòng khách sạn[cite: 1].
     * Cho phép chuyển đổi linh hoạt qua giao diện đơn giản giữa các trạng thái: DIRTY, CLEAN, READY[cite: 1].
     */
    public class UpdateRoomStatusDto
    {
        public int RoomId { get; set; }
        public string HousekeepingStatus { get; set; } = null!; // DIRTY, CLEAN, READY
    }

    /**
     * [UC19.4 - Report Room Maintenance Issues]
     * DTO thu thập thông tin khi Nhân viên buồng phòng phát hiện và lập phiếu báo cáo sự cố hỏng hóc vật tư thiết bị trong phòng[cite: 1].
     * Trường thông tin mô tả chi tiết (Description) cấu hình bắt buộc không được để trống theo quy tắc BR-64[cite: 1].
     */
    public class ReportMaintenanceDto
    {
        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Issue type is required")]
        public string IssueType { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = null!;

        public int ReportedById { get; set; }
    }

    /**
     * [UC19.4 - Report Room Maintenance Issues]
     * DTO phục vụ cho mục đích kết xuất hiển thị thông tin chi tiết đầy đủ của một phiếu báo cáo sự cố bảo trì buồng phòng lên hệ thống[cite: 1].
     */
    public class MaintenanceIssueDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string ReportedByName { get; set; } = null!;
        public string IssueType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    /**
     * [UC19.3 - Update Task Status] (Đối với tác vụ sửa chữa)
     * DTO tiếp nhận lệnh điều chỉnh trạng thái xử lý phiếu sự cố kỹ thuật của phòng (PENDING, IN_PROGRESS, RESOLVED)[cite: 1].
     */
    public class UpdateMaintenanceStatusDto
    {
        public string Status { get; set; } = null!; // PENDING, IN_PROGRESS, RESOLVED
    }
}