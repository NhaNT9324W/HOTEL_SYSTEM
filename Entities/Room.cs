using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    public class Room : BaseEntity
    {
        public string RoomNumber { get; set; } = null!;
        public int RoomTypeId { get; set; }
        public RoomType? RoomType { get; set; }
        public RoomBookingStatus BookingStatus { get; set; } = RoomBookingStatus.AVAILABLE;
        public RoomHousekeepingStatus HousekeepingStatus { get; set; } = RoomHousekeepingStatus.CLEAN;
        public int Floor { get; set; }
    }
}