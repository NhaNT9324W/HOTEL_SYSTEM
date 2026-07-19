using System;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2 Base Architecture - Entity Layer]
     * Lớp trừu tượng cơ sở (Abstract Base Class) nền tảng cho tất cả các thực thể (Entities) trong hệ thống.
     * Đóng vai trò chuẩn hóa cấu trúc và tự động hóa việc quản lý siêu dữ liệu lịch sử (Auditing Metadata), 
     * giúp theo dõi sát sao vòng đời dữ liệu của toàn bộ các bảng trong cơ sở dữ liệu khách sạn.
     */
    public abstract class BaseEntity
    {
        /** Mã định danh duy nhất (Primary Key - Khóa chính) dạng số nguyên tự động tăng (Identity) của từng thực thể lưu trữ trong hệ thống. */
        public int Id { get; set; }

        /** 
         * Mốc thời gian bản ghi được khởi tạo lần đầu tiên vào cơ sở dữ liệu. 
         * Hệ thống thiết lập mặc định theo chuẩn múi giờ quốc tế UTC (DateTime.UtcNow) để bảo đảm tính nhất quán dữ liệu lịch sử, 
         * tránh xung đột múi giờ khi kết xuất báo cáo thống kê.
         */
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /** 
         * Mốc thời gian ghi nhận lượt hiệu chỉnh hoặc cập nhật thông tin gần đây nhất của thực thể. 
         * Được tự động làm mới ở tầng lưu trữ dữ liệu (Repository/Intercepting) mỗi khi có hành vi chỉnh sửa cấu trúc trường thông tin xảy ra trên bản ghi.
         */
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}