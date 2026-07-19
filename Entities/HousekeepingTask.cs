using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    /**
     * [IV.5.2.4 HousekeepingTask Entity]
     * Thực thể đại diện cho bảng dữ liệu 'HousekeepingTasks' trong cơ sở dữ liệu.
     * Quản lý vòng đời và tiến độ của các tác vụ buồng phòng (như dọn dẹp thường nhật, tổng vệ sinh, kiểm tra phòng).
     * Thực thể này đóng vai trò trung tâm trong mô-đun Quản lý tác vụ buồng phòng (UC19), 
     * hỗ trợ phân phối công việc cho Room Staff (UC19.1) và cập nhật tiến độ vận hành thời gian thực (UC19.3).
     */
    public class HousekeepingTask : BaseEntity
    {
        /** 
         * Mã định danh ngoại khóa liên kết với phòng vật lý cần xử lý (Room).
         * Ràng buộc OnDelete Restrict tại Fluent API bảo vệ không cho phép xóa phòng nếu tồn tại lịch sử tác vụ.
         */
        public int RoomId { get; set; }

        /** 
         * Mã ngoại khóa liên kết tới tài khoản nhân viên buồng phòng (Room Staff) được giao trách nhiệm thực thi tác vụ.
         * Phục vụ kiểm tra điều kiện Task Ownership Constraint nhằm đảm bảo nhân viên chỉ có quyền thao tác trên nhiệm vụ của mình.
         */
        public int AssignedToId { get; set; }

        /** Mã ngoại khóa lưu trữ danh tính người khởi tạo tác vụ (thường thuộc nhóm quyền Admin hoặc Hotel Manager). */
        public int CreatedById { get; set; }

        /** Phân loại hình thức tác vụ buồng phòng (Enum: Cleaning, Maintenance, Inspection). */
        public TaskType TaskType { get; set; }

        /** Mức độ ưu tiên và khẩn cấp của công việc (Enum: Low, Medium, High, Critical), giúp nhân viên sắp xếp thứ tự thực hiện. */
        public TaskPriority Priority { get; set; }

        /** 
         * Trạng thái tiến độ hiện tại của công việc (Enum: Pending, InProgress, Completed).
         * Mặc định khi khởi tạo là Pending. Quá trình cập nhật trạng thái phải tuân thủ nghiêm ngặt 
         * luồng logic chuyển đổi tuần tự: PENDING -> IN_PROGRESS -> COMPLETED.
         */
        public HousekeepingTaskStatus Status { get; set; } = HousekeepingTaskStatus.Pending;

        /** Mô tả chi tiết yêu cầu công việc hoặc các ghi chú đặc biệt do cấp quản lý chỉ định (ví dụ: "Thay thêm ga giường", "Khách yêu cầu thêm khăn"). */
        public string Description { get; set; } = string.Empty;

        /** Mốc thời gian giới hạn bắt buộc phải hoàn thành tác vụ dọn dẹp/kiểm tra (có thể để trống). */
        public DateTime? DueDate { get; set; }

        /** Mốc thời gian ghi nhận tự động bởi hệ thống ngay khi nhân viên chuyển đổi trạng thái tác vụ thành 'Completed'. */
        public DateTime? CompletedAt { get; set; }

        // Navigation Properties

        /** Thuộc tính điều hướng liên kết chi tiết thông tin của phòng vật lý tương ứng. */
        public Room Room { get; set; } = null!;

        /** Thuộc tính điều hướng liên kết hồ sơ thông tin nhân viên được phân công thực hiện nhiệm vụ. */
        public Account AssignedTo { get; set; } = null!;

        /** Thuộc tính điều hướng liên kết hồ sơ của người có thẩm quyền đã ra lệnh khởi tạo tác vụ. */
        public Account CreatedBy { get; set; } = null!;
    }
}