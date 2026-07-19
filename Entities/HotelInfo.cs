namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.3 HotelInfo Entity]
     * Thực thể đại diện cho bảng dữ liệu 'HotelInfos' trong cơ sở dữ liệu.
     * Thiết kế theo mô hình Singleton ở tầng dữ liệu nhằm lưu trữ thông tin định danh và cấu hình nền tảng duy nhất của khách sạn.
     * Dữ liệu từ thực thể này được sử dụng xuyên suốt hệ thống, đặc biệt là trong quy trình Cập nhật thông tin khách sạn (UC06) và kết xuất phôi hóa đơn tài chính chính thức cho khách hàng (UC18).
     */
    public class HotelInfo : BaseEntity
    {
        /** 
         * Tên thương mại chính thức của khách sạn.
         * Áp dụng Business Rule BR-18: Trường dữ liệu bắt buộc điền, cấu hình độ dài tối đa 200 ký tự tại tầng Fluent API.
         */
        public string HotelName { get; set; } = string.Empty;

        /** 
         * Địa chỉ vật lý/trụ sở hoạt động của khách sạn.
         * Áp dụng Business Rule BR-18: Thông tin bắt buộc điền phục vụ hiển thị trên phôi hóa đơn GTGT, giới hạn tối đa 500 ký tự.
         */
        public string Address { get; set; } = string.Empty;

        /** 
         * Số điện thoại hotline/Tổng đài chăm sóc khách hàng chính.
         * Áp dụng Business Rule BR-18: Trường bắt buộc, giới hạn tối đa 20 ký tự để đảm bảo tính nhất quán định dạng.
         */
        public string Phone { get; set; } = string.Empty;

        /** Hộp thư điện tử liên hệ và tiếp nhận phản hồi từ khách hàng, hỗ trợ độ dài dữ liệu tối đa 100 ký tự. */
        public string Email { get; set; } = string.Empty;

        /** Đường dẫn trang web chính thức của khách sạn (ví dụ: tên miền tra cứu thông tin dịch vụ hoặc đặt phòng trực tuyến). */
        public string Website { get; set; } = string.Empty;

        /** Đoạn văn bản giới thiệu tổng quan về lịch sử, quy mô phòng hoặc các dịch vụ tiện ích đặc trưng của khách sạn, tối đa 1000 ký tự. */
        public string Description { get; set; } = string.Empty;
    }
}