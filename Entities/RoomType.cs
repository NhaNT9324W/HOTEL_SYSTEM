using System.Collections.Generic;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.9 RoomType Entity]
     * Thực thể đại diện cho bảng dữ liệu 'RoomTypes' trong cơ sở dữ liệu.
     * Lưu trữ toàn bộ thông tin cấu hình nền tảng, định mức biểu phí cơ bản, sức chứa tối đa và trạng thái hoạt động của từng phân loại hạng phòng.
     * Thực thể này tham gia trực tiếp vào mô-đun Quản lý hạng phòng (UC09), cấu hình phòng vật lý (UC07), 
     * và làm cơ sở dữ liệu gốc để tính toán tiền phòng trong quy trình đặt phòng (UC13) cũng như kết xuất báo cáo tài chính (UC12.3).
     */
    public class RoomType : BaseEntity
    {
        /** 
         * Tên gọi chính thức của hạng phòng (ví dụ: "Standard", "Deluxe Double", "Executive Suite").
         * Áp dụng Business Rule BR-09: Trường dữ liệu bắt buộc và phải là duy nhất (Unique Index) trên toàn hệ thống khách sạn nhằm ngăn chặn trùng lặp cấu hình phân loại.
         */
        public string Name { get; set; } = null!;

        /** Đoạn văn bản mô tả chi tiết về đặc điểm, tiện nghi đi kèm hoặc diện tích của hạng phòng (có thể để trống). */
        public string? Description { get; set; }

        /** 
         * Giá thuê phòng cơ bản áp dụng cho một đêm lưu trú của hạng phòng này.
         * Sử dụng kiểu dữ liệu decimal(18,2) nhằm đảm bảo độ chính xác tuyệt đối trong tính toán tài chính và tuân thủ bộ quy tắc biểu phí hệ thống.
         */
        public decimal BasePrice { get; set; }

        /** 
         * Sức chứa/Số lượng khách lưu trú tối đa được phép ở trong phòng thuộc phân loại này.
         * Dữ liệu này phục vụ cho việc kiểm tra ràng buộc Over-occupancy Constraint tại tầng nghiệp vụ khi Tiếp tân thực hiện lập đơn đặt phòng mới (UC13).
         */
        public int MaxOccupancy { get; set; }

        /** 
         * Cờ trạng thái cho biết hạng phòng này có còn được áp dụng kinh doanh hay không.
         * Mặc định là true. Khi chuyển thành false, hệ thống sẽ chặn không cho phép tạo phòng vật lý mới thuộc loại này (UC07) hoặc khởi tạo đơn đặt phòng mới (UC13).
         */
        public bool IsActive { get; set; } = true;

        // Navigation Properties

        /** 
         * Bộ sưu tập danh sách toàn bộ các phòng vật lý (Rooms) thuộc về phân loại hạng phòng này.
         * Thể hiện mối quan hệ Một-Nhiều (One-to-Many) điều hướng thuận từ RoomType sang thực thể Room.
         */
        public ICollection<Room>? Rooms { get; set; }
    }
}