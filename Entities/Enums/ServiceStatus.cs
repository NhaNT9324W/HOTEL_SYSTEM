namespace Hotel_System.Entities.Enums
{
    /**
     * [UC10 – Manage Hotel Services] / [UC17 – Record Service Usage]
     * Danh mục liệt kê các trạng thái kinh doanh và tính khả dụng thương mại của các dịch vụ tiện ích bổ sung trong khách sạn.
     * Chỉ số này quyết định trực tiếp xem một dịch vụ có được phép hiển thị và áp dụng để ghi nhận tiêu dùng cho khách lưu trú tại quầy lễ tân hay không.
     */
    public enum ServiceStatus
    {
        /** 
         * Dịch vụ đang hoạt động bình thường và sẵn sàng cung cấp cho khách lưu trú.
         * Tiếp tân có thể tìm kiếm, lựa chọn và gán dịch vụ này vào danh sách tiêu dùng (Folio) của khách hàng khi thực hiện quy trình ghi nhận sử dụng dịch vụ (UC17).
         */
        Active,

        /** 
         * Dịch vụ đã tạm thời ngưng cung cấp hoặc áp dụng ngừng kinh doanh vĩnh viễn trong khách sạn.
         * Hệ thống sẽ chặn và ẩn dịch vụ này khỏi danh mục hiển thị khả dụng của Tiếp tân nhằm tránh sai sót vận hành, 
         * nhưng giữ lại bản ghi cấu hình trong database để bảo toàn tính toàn vẹn dữ liệu đối chiếu cho các hóa đơn lịch sử (UC18).
         */
        Inactive
    }
}