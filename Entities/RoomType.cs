namespace Hotel_System.Entities
{
    public class RoomType : BaseEntity
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Room>? Rooms { get; set; }
    }
}