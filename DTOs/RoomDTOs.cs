using Hotel_System.Entities.Enums;

namespace Hotel_System.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = null!;
        public int Floor { get; set; }
        public int RoomTypeId { get; set; }
        public string RoomTypeName { get; set; } = null!;
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; } // MỚI - Sức chứa, lấy từ RoomType
        public RoomBookingStatus BookingStatus { get; set; }
        public RoomHousekeepingStatus HousekeepingStatus { get; set; }
    }

    public class CreateRoomDto
    {
        public string RoomNumber { get; set; } = null!;
        public int Floor { get; set; }
        public int RoomTypeId { get; set; }
    }

    public class UpdateRoomDto
    {
        public string RoomNumber { get; set; } = null!;
        public int Floor { get; set; }
        public int RoomTypeId { get; set; }
        public RoomBookingStatus BookingStatus { get; set; }
        public RoomHousekeepingStatus HousekeepingStatus { get; set; }
    }
}