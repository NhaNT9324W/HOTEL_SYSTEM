namespace Hotel_System.Entities
{
    public class Guest : BaseEntity
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string IdNumber { get; set; } = null!; // CCCD
        public string? Email { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}