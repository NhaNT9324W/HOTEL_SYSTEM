using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.10 Service Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Services' trong cơ sở dữ liệu.
     * Lưu trữ toàn bộ danh mục cấu hình thông tin các dịch vụ tiện ích bổ sung mà khách sạn cung cấp (như ăn uống, giặt ủi, spa, xe đưa đón).
     * Thực thể này tham gia trực tiếp vào mô-đun Quản lý dịch vụ khách sạn (UC10) và làm cơ sở dữ liệu gốc để ghi nhận tiêu dùng tại quầy tiếp tân (UC17).
     */
    public class Service : BaseEntity
    {
        /** Tên gọi chính thức của dịch vụ khách sạn, cấu hình bắt buộc nhập và giới hạn tối đa 200 ký tự tại tầng dữ liệu. */
        public string ServiceName { get; set; } = string.Empty;

        /** Đoạn văn bản mô tả chi tiết nội dung, quy trình cung cấp hoặc lưu ý đi kèm của dịch vụ, hỗ trợ tối đa 1000 ký tự. */
        public string Description { get; set; } = string.Empty;

        /** 
         * Biểu phí niêm yết áp dụng cho một đơn vị tiêu dùng của dịch vụ này.
         * Sử dụng kiểu dữ liệu decimal(18,2) nhằm đảm bảo độ chính xác tuyệt đối trong tính toán tài chính và tuân thủ bộ quy tắc nghiệp vụ Business Rule BR-35 (giá trị bắt buộc lớn hơn 0).
         */
        public decimal Price { get; set; }

        /** 
         * Trạng thái hoạt động và áp dụng kinh doanh hiện tại của dịch vụ (Enum: Active, Inactive).
         * Mặc định khi khởi tạo là Active. Khi chuyển thành Inactive, hệ thống sẽ chặn và ẩn dịch vụ khỏi danh mục lựa chọn khả dụng khi Tiếp tân tiến hành ghi nhận sử dụng dịch vụ cho khách lưu trú (UC17).
         */
        public ServiceStatus Status { get; set; } = ServiceStatus.Active;
    }
}