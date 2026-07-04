using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    public class HousekeepingTask : BaseEntity
    {
        public int RoomId { get; set; }
        public int AssignedToId { get; set; }
        public int CreatedById { get; set; }
        public TaskType TaskType { get; set; }
        public TaskPriority Priority { get; set; }
        public HousekeepingTaskStatus Status { get; set; } = HousekeepingTaskStatus.Pending;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public Room Room { get; set; } = null!;
        public Account AssignedTo { get; set; } = null!;
        public Account CreatedBy { get; set; } = null!;
    }
}