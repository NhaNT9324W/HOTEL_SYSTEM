using System;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.11 ServiceUsage Entity]
     * Thực thể đại diện cho bảng dữ liệu 'ServiceUsages' trong cơ sở dữ liệu.
     * Ghi nhận và lưu vết chi tiết lịch sử từng lượt tiêu dùng dịch vụ tiện ích phát sinh của khách lưu trú gắn liền với một đơn đặt phòng cụ thể.
     * Thực thể này tham gia trực tiếp vào quy trình Ghi nhận sử dụng dịch vụ (UC17) và cung cấp dữ liệu gốc để đối chiếu, tính toán tổng chi phí dịch vụ (ServiceCharge) khi lập hóa đơn thanh toán (UC18).
     */
    public class ServiceUsage : BaseEntity
    {
        /** Mã định danh ngoại khóa liên kết trực tiếp tới đơn đặt phòng của khách hàng (Reservation). */
        public int ReservationId { get; set; }

        /** Thuộc tính điều hướng liên kết thông tin chi tiết của hồ sơ đơn đặt phòng liên quan. */
        public Reservation? Reservation { get; set; }

        /** Mã định danh ngoại khóa liên kết tới danh mục dịch vụ nền tảng (Service) được lựa chọn sử dụng. */
        public int ServiceId { get; set; }

        /** Thuộc tính điều hướng liên kết thông tin cấu hình và định danh của dịch vụ khách sạn tương ứng. */
        public Service? Service { get; set; }

        /** 
         * Số lượng đơn vị dịch vụ mà khách hàng đã tiêu dùng trong lượt này.
         * Áp dụng ràng buộc kiểm tra dữ liệu đầu vào đầu cuối, giới hạn định mức tiêu dùng trong phạm vi từ 1 đến 100 đơn vị cho mỗi lần Tiếp tân ghi nhận thành công.
         */
        public int Quantity { get; set; }

        /** 
         * Đơn giá thực tế của một đơn vị dịch vụ được áp dụng ngay tại thời điểm khách hàng tiêu dùng.
         * Việc lưu trữ bản sao đơn giá tại thời điểm này nhằm bảo đảm tính toàn vẹn dữ liệu kế toán tài chính, tránh việc thay đổi biểu phí gốc trong danh mục (Service.Price) làm ảnh hưởng sai lệch tới hóa đơn lịch sử.
         */
        public decimal UnitPrice { get; set; }

        /** 
         * Tổng chi phí tài chính của toàn bộ lượt tiêu dùng dịch vụ này.
         * Cấu hình kiểu dữ liệu tiền tệ decimal(18,2) tuân thủ nghiêm ngặt Business Rule BR-35, giá trị được tính toán tự động dựa trên công thức nghiệp vụ: TotalPrice = Quantity * UnitPrice.
         */
        public decimal TotalPrice { get; set; }

        /** Mốc thời gian hệ thống tự động ghi nhận thời điểm phát sinh giao dịch tiêu dùng dịch vụ, chuẩn hóa mặc định theo múi giờ quốc tế UTC (DateTime.UtcNow). */
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}