using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.1 Account Entity]
     * Thực thể đại diện cho bảng dữ liệu 'Accounts' trong cơ sở dữ liệu.
     * Lưu trữ toàn bộ thông tin định danh hồ sơ, thông tin xác thực mật mã và phân bổ phân quyền chức năng của đội ngũ nhân sự vận hành khách sạn.
     * Thực thể này tham gia trực tiếp vào các quy trình Core Security: Đăng nhập (UC01), Đổi mật khẩu (UC04) và Quản lý tài khoản (UC05).
     */
    public class Account : BaseEntity
    {
        /** Tên đầy đủ của nhân viên, giới hạn hiển thị tối đa 100 ký tự trên phôi hệ thống. */
        public string FullName { get; set; } = string.Empty;

        /** 
         * Tên tài khoản dùng để đăng nhập hệ thống.
         * Áp dụng Business Rule BR-02: Trường dữ liệu bắt buộc và phải là duy nhất (Unique Index) trên toàn hệ thống.
         */
        public string Username { get; set; } = string.Empty;

        /** 
         * Chuỗi ký tự mật khẩu đã được mã hóa bảo mật.
         * Tuân thủ nghiêm ngặt bộ quy chế mã hóa mật mã BR-01: Tuyệt đối không lưu văn bản thô (plain text), 
         * mật khẩu bắt buộc phải đi qua thuật toán băm SHA256 và chuyển đổi sang chuỗi Base64 trước khi ghi nhận xuống DB.
         */
        public string PasswordHash { get; set; } = string.Empty;

        /** 
         * Địa chỉ thư điện tử cá nhân/công việc của nhân viên.
         * Áp dụng Unique Index ràng buộc hệ thống: Mỗi tài khoản chỉ liên kết với một Email duy nhất để phục vụ tính năng khôi phục mật khẩu (UC03).
         */
        public string Email { get; set; } = string.Empty;

        /** Số điện thoại liên hệ trực tiếp của nhân viên, hỗ trợ định dạng tối đa 20 ký tự. */
        public string Phone { get; set; } = string.Empty;

        /** 
         * Phân quyền vai trò chức năng trong hệ thống khách sạn (Enum: Admin, HotelManager, Receptionist, RoomStaff).
         * Quyết định phạm vi quyền hạn khi đi qua các bộ lọc xác thực '[Authorize(Roles = "...")]' tại tầng API Controllers.
         */
        public Role Role { get; set; }

        /** 
         * Trạng thái hoạt động hiện tại của tài khoản nhân viên (Enum: Active, Inactive).
         * Mặc định khi khởi tạo là Active. Tài khoản ở trạng thái 'Inactive' sẽ bị chặn truy cập tuyệt đối ở bước xác thực đăng nhập (UC01).
         */
        public AccountStatus Status { get; set; } = AccountStatus.Active;

        /** 
         * Chuỗi mã thông báo bảo mật (Token) sinh ra ngẫu nhiên khi người dùng yêu cầu khôi phục mật khẩu (UC03).
         * Được gửi kèm vào đường dẫn liên kết xác kết danh tính qua email của nhân viên.
         */
        public string? ResetToken { get; set; }

        /** 
         * Mốc m thời gian giới hạn hiệu lực của mã thông báo khôi phục mật khẩu (ResetToken).
         * Vượt quá mốc thời gian này, chuỗi ResetToken sẽ bị coi là vô hiệu hóa và hệ thống từ chối xử lý lệnh đặt lại mật khẩu.
         */
        public DateTime? ResetTokenExpiry { get; set; }
    }
}