namespace Hotel_System.DTOs
{
    // Danh sách - không hiện full info
    public class GuestListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }

    // Chi tiết - hiện khi bấm vào card, UC15.2
    public class GuestDetailDto : GuestListDto
    {
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalReservations { get; set; }
    }

    public class CreateGuestDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
    }

    public class UpdateGuestDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
    }
}