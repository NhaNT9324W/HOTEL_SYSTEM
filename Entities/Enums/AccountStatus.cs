namespace Hotel_System.Entities.Enums
{
    /**
     * [UC05 - Manage Account] / [UC05.2 - Edit Account]
     * Danh mục liệt kê tất cả các trạng thái vận hành và kiểm soát an ninh của tài khoản nhân viên.
     * Chỉ số này tham gia trực tiếp vào bộ lọc xác thực của quy trình Đăng nhập (UC01), quyết định quyền truy cập của nhân sự vào hệ thống khách sạn.
     */
    public enum AccountStatus
    {
        /** Tài khoản đang hoạt động bình thường, cho phép đăng nhập và thực thi các chức năng theo phân bổ vai trò (Role). */
        Active,

        /** Tài khoản đã bị vô hiệu hóa (ví dụ: nhân viên đã nghỉ việc), hệ thống sẽ chặn quyền truy cập tuyệt đối nhưng vẫn bảo toàn dữ liệu lịch sử tác vụ liên quan. */
        Inactive,

        /** 
         * Tài khoản bị hệ thống tự động khóa để bảo vệ an ninh.
         * Trạng thái này được kích hoạt khi tài khoản vi phạm Business Rule BR-03 (ví dụ: thực hiện đăng nhập sai mật khẩu vượt quá 5 lần liên tiếp) và yêu cầu Quản trị viên (Admin) phê duyệt mở khóa thủ công.
         */
        Locked
    }
}