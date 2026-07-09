namespace Hotel_System.Entities
{
    public class Invoice : BaseEntity
    {
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public int Nights { get; set; }
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}