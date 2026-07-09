namespace Hotel_System.Entities
{
    public class ServiceUsage : BaseEntity
    {
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public int ServiceId { get; set; }
        public Service? Service { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}