namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.6 MaintenanceIssue Entity]
     * Thực thể đại diện cho bảng dữ liệu 'MaintenanceIssues' trong cơ sở dữ liệu.
     * Quản lý thông tin hồ sơ, phân loại và tiến độ xử lý các sự cố hỏng hóc vật tư, trang thiết bị phát sinh tại các phòng vật lý trong khách sạn.
     * Thực thể này tham gia trực tiếp vào chức năng Báo cáo sự cố bảo trì buồng phòng (UC19.4) nhằm phối hợp thông tin giữa bộ phận buồng phòng và kỹ thuật.
     */
    public class MaintenanceIssue : BaseEntity
    {
        /** Mã định danh ngoại khóa liên kết trực tiếp tới phòng vật lý đang xảy ra sự cố kỹ thuật (Room). */
        public int RoomId { get; set; }

        /** Thuộc tính điều hướng liên kết chi tiết thông tin của phòng vật lý tương ứng. */
        public Room? Room { get; set; }

        /** Mã định danh ngoại khóa lưu vết tài khoản nhân viên (thường là Room Staff hoặc Tiếp tân) đã phát hiện và ghi nhận sự cố lên hệ thống. */
        public int ReportedById { get; set; }

        /** Thuộc tính điều hướng liên kết hồ sơ tài khoản nhân viên thực hiện lập phiếu báo cáo sự cố. */
        public Account? ReportedBy { get; set; }

        /** Phân loại nhóm hạng mục sự cố kỹ thuật (ví dụ: Điện, Điện lạnh, Nước, Đồ gỗ, Thiết bị gia dụng) phục vụ mục đích phân phối công việc cho thợ sửa chữa. */
        public string IssueType { get; set; } = null!;

        /** 
         * Nội dung mô tả chi tiết về hiện trạng hư hại hoặc dấu hiệu hỏng hóc của trang thiết bị.
         * Áp dụng nghiêm ngặt bộ quy tắc nghiệp vụ Business Rule BR-64: Trường dữ liệu bắt buộc, tuyệt đối không được phép bỏ trống trong quy trình tạo phiếu báo cáo sự cố.
         */
        public string Description { get; set; } = null!;

        /** 
         * Trạng thái xử lý và khắc phục sự cố hỏng hóc của phòng (Mặc định khi khởi tạo là "PENDING").
         * Nhận các giá trị kiểm soát vận hành bao gồm ba trạng thái cốt lõi: PENDING, IN_PROGRESS, RESOLVED.
         */
        public string Status { get; set; } = "PENDING";
    }
}