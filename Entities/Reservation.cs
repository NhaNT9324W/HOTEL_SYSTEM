using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    /// <summary>Đặt phòng - UC13. Ràng buộc trạng thái phòng kiểm tra ở tầng Service, không đặt trong Entity.</summary>
    public class Reservation : BaseEntity
    {
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        public int GuestId { get; set; }
        public Guest? Guest { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.PENDING;
    }
}