namespace Hotel_System.Entities
{
    public class MaintenanceIssue : BaseEntity
    {
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public int ReportedById { get; set; }
        public Account? ReportedBy { get; set; }
        public string IssueType { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = "PENDING";
    }
}