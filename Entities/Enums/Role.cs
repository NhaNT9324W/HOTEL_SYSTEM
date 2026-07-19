namespace Hotel_System.Entities.Enums
{
    /**
     * [UC05 – Manage Account] / [Core Security - Authorization]
     * Danh mục liệt kê các phân quyền vai trò (Role-based Access Control - RBAC) của đội ngũ nhân sự trong hệ thống khách sạn.
     * Chỉ số này tham gia trực tiếp vào cơ chế xác thực và phân quyền tại tầng API (thông qua bộ lọc [Authorize(Roles = "...")]), 
     * quyết định phạm vi truy cập dữ liệu và giới hạn quyền thực thi chức năng tương ứng với vị trí công việc của từng tài khoản.
     */
    public enum Role
    {
        /** 
         * Quản trị viên hệ thống (Super User).
         * Nắm giữ đặc quyền cao nhất, toàn quyền truy cập và kiểm soát mọi phân hệ.
         * Chịu trách nhiệm thiết lập cấu hình nền tảng (UC06, UC09) và quản trị vòng đời, phân quyền hồ sơ tài khoản nhân sự (UC05).
         */
        Admin,

        /** 
         * Quản lý khách sạn (Giám đốc/Trưởng bộ phận vận hành).
         * Quyền hạn tập trung vào công tác giám sát kinh doanh và điều phối nguồn lực.
         * Có quyền truy cập sâu vào các báo cáo thống kê doanh thu (UC12), quản lý danh mục dịch vụ (UC10) và khởi tạo, theo dõi các tác vụ vận hành tổng thể (UC19).
         */
        HotelManager,

        /** 
         * Nhân viên Lễ tân (Tiếp tân - Front Office).
         * Chịu trách nhiệm xử lý trực tiếp các nghiệp vụ tiền sảnh và tương tác với khách lưu trú.
         * Giới hạn quyền truy cập xoay quanh các mô-đun: Quản lý khách hàng (UC15), Đặt phòng (UC13), Check-in/Check-out (UC14, UC18) và Ghi nhận tiêu dùng dịch vụ (UC17). 
         * Không có quyền can thiệp vào cấu hình lõi hệ thống hay dữ liệu tài khoản nội bộ.
         */
        Receptionist,

        /** 
         * Nhân viên Buồng phòng / Kỹ thuật bảo trì (Housekeeping/Maintenance).
         * Giới hạn phạm vi quyền truy cập hẹp nhất, chỉ tập trung vào phân hệ thao tác nghiệp vụ thực địa.
         * Được phép xem danh sách tác vụ được phân gán (UC19.1), cập nhật trạng thái vệ sinh của phòng vật lý (UC19.2) và lập phiếu báo cáo sự cố hỏng hóc (UC19.4).
         * Tuyệt đối không được phép tiếp cận vào cơ sở dữ liệu hồ sơ khách hàng, luồng tiền tài chính hay thông tin đơn đặt phòng.
         */
        RoomStaff
    }
}