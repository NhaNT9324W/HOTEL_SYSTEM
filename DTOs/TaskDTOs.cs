using Hotel_System.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
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
}