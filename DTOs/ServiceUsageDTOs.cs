using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    public class ServiceUsageDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string ServiceName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime UsedAt { get; set; }
    }

    public class AddServiceUsageDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public string RoomTypeName { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ServiceUsageDto> Services { get; set; } = new();
        public DateTime IssuedAt { get; set; }
    }
}