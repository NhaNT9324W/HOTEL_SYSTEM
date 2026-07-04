namespace Hotel_System.DTOs
{
    // Dùng cho danh sách (không hiện full info) - hiển thị bảng
    public class ReservationListDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public string GuestName { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } = null!;
    }

    // Dùng khi bấm vào card để xem full - UC13.2
    public class ReservationDetailDto : ReservationListDto
    {
        public string GuestPhone { get; set; } = null!;
        public string? GuestEmail { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReservationDto
    {
        public int RoomId { get; set; }
        public string GuestFullName { get; set; } = null!;
        public string GuestPhone { get; set; } = null!;
        public string? GuestEmail { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}