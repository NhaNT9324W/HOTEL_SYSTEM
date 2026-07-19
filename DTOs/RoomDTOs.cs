using Hotel_System.Entities.Enums;

namespace Hotel_System.DTOs
{
    /**
     * [UC07 - Manage Room] / [UC7.3 / UC7.5 - Room Data Representation]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) chứa thông tin toàn diện về một phòng vật lý trong hệ thống khách sạn[cite: 1].
     * Phục vụ trực tiếp cho tác vụ hiển thị danh sách phòng, tra cứu trạng thái hoặc hiển thị thông tin chi tiết phòng trên màn hình điều hành[cite: 1].
     */
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

    /**
     * [UC7.1 - Create Room]
     * DTO tiếp nhận dữ liệu từ phía Client khi người dùng có thẩm quyền (Admin hoặc Hotel Manager) thực hiện khởi tạo và thêm mới một phòng vật lý[cite: 1].
     * Quá trình xử lý sẽ kiểm tra ràng buộc số phòng duy nhất và lấy các thông tin bổ sung dựa trên mã hạng phòng RoomTypeId[cite: 1].
     */
    public class CreateRoomDto
    {
        public string RoomNumber { get; set; } = null!;
        public int Floor { get; set; }
        public int RoomTypeId { get; set; }
    }

    /**
     * [UC7.2 - Edit Room]
     * DTO tiếp nhận thông tin yêu cầu cập nhật, hiệu chỉnh cấu hình hoặc thay đổi trạng thái của một phòng đang tồn tại trong database[cite: 1].
     * Cho phép điều chỉnh linh hoạt số phòng, số tầng, phân loại hạng phòng mới cũng như cập nhật các trạng thái đặt phòng hay buồng phòng liên quan[cite: 1].
     */
    public class UpdateRoomDto
    {
        public string RoomNumber { get; set; } = null!;
        public int Floor { get; set; }
        public int RoomTypeId { get; set; }
        public RoomBookingStatus BookingStatus { get; set; }
        public RoomHousekeepingStatus HousekeepingStatus { get; set; }
    }
}