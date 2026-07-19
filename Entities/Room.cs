using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.8 Room Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Rooms' trong cơ sở dữ liệu.
     * Lưu trữ và quản lý thông tin chi tiết của từng phòng vật lý trong cấu trúc tòa nhà khách sạn, bao gồm số phòng, vị trí tầng, liên kết phân loại hạng phòng cùng hai chỉ số trạng thái vận hành cốt lõi.
     * Thực thể này đóng vai trò mắt xích trung tâm, tham gia trực tiếp vào các quy trình nghiệp vụ chính: Quản lý phòng (UC07), Đặt phòng (UC13) và Điều phối tác vụ buồng phòng (UC19).
     */
    public class Room : BaseEntity
    {
        /** 
         * Số hiệu nhận diện của phòng vật lý (ví dụ: "101", "A205", "P403").
         * Áp dụng Business Rule BR-05: Trường dữ liệu bắt buộc và phải là duy nhất (Unique Index) trên toàn bộ hệ thống khách sạn nhằm ngăn chặn tuyệt đối lỗi trùng lặp phòng khi cấu hình.
         */
        public string RoomNumber { get; set; } = null!;

        /** Mã định danh ngoại khóa liên kết trực tiếp tới cấu hình phân loại hạng phòng (RoomType) để xác định định mức biểu phí và sức chứa tối đa. */
        public int RoomTypeId { get; set; }

        /** Thuộc tính điều hướng liên kết thông tin chi tiết về cấu hình, mô tả và chính sách giá của hạng phòng (RoomType) liên quan. */
        public RoomType? RoomType { get; set; }

        /** 
         * Trạng thái kinh doanh/đặt phòng hiện tại của phòng vật lý (Enum: AVAILABLE, OCCUPIED, RESERVED, MAINTENANCE).
         * Mặc định khi thiết lập hệ thống là AVAILABLE. Giá trị này được biến đổi tự động thông qua các hành vi xác thực Check-in/Check-out của Tiếp tân tại tầng nghiệp vụ.
         */
        public RoomBookingStatus BookingStatus { get; set; } = RoomBookingStatus.AVAILABLE;

        /** 
         * Trạng thái dọn dẹp vệ sinh hiện tại của phòng (Enum: CLEAN, DIRTY, READY, INSPECTION).
         * Mặc định ban đầu là CLEAN. Chỉ số này sẽ tự động chuyển sang DIRTY khi khách thực hiện trả phòng và thay đổi linh hoạt thông qua màn hình điều hành cập nhật của Nhân viên buồng phòng (UC19.2).
         */
        public RoomHousekeepingStatus HousekeepingStatus { get; set; } = RoomHousekeepingStatus.CLEAN;

        /** Vị trí số tầng của phòng trong sơ đồ tòa nhà khách sạn, hỗ trợ Tiếp tân điều phối lọc phòng nhanh và giúp Quản lý phân ban nhiệm vụ buồng phòng theo khu vực địa lý. */
        public int Floor { get; set; }
    }
}