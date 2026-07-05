using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    public class ReservationListDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string GuestName { get; set; } = null!;
        public string GuestPhone { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = null!;
    }

    public class ReservationDetailDto : ReservationListDto
    {
        public string GuestIdNumber { get; set; } = null!;
        public string? GuestEmail { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public int Floor { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReservationDto
    {
        [Required(ErrorMessage = "Guest name is required")]
        public string GuestFullName { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        public string GuestPhone { get; set; } = null!;

        [Required(ErrorMessage = "ID Number (CCCD) is required")]
        public string GuestIdNumber { get; set; } = null!;

        public string? GuestEmail { get; set; }

        [Required(ErrorMessage = "Room is required")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Check-in date is required")]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required")]
        public DateTime CheckOutDate { get; set; }
    }

    public class UpdateReservationDto
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}