namespace Hotel_System.Entities.Enums
{
    /**
     * [UC19 - Manage Housekeeping Tasks] / [UC19.3 - Update Task Status]
     * Danh mục liệt kê các trạng thái tiến độ trong vòng đời vận hành của một tác vụ buồng phòng hoặc sửa chữa bảo trì.
     * Chỉ số này kiểm soát trực tiếp luồng logic chuyển đổi trạng thái công việc (Task Status Transition Logic) 
     * của nhân viên Room Staff và là cơ sở để hệ thống tự động hóa các báo cáo hiệu suất làm việc.
     */
    public enum HousekeepingTaskStatus
    {
        /** 
         * Tác vụ đã được Quản lý khởi tạo và phân gán thành công cho nhân viên buồng phòng nhưng chưa bắt đầu thực hiện.
         * Đây là trạng thái mặc định ban đầu của mọi nhiệm vụ dọn dẹp hoặc bảo trì khi vừa ghi nhận vào cơ sở dữ liệu.
         */
        Pending,

        /** 
         * Nhân viên buồng phòng đã tiếp nhận và đang trực tiếp xử lý công việc thực tế tại phòng vật lý tương ứng.
         * Khi chuyển sang trạng thái này, hệ thống sẽ ghi nhận tiến độ thời gian thực và hạn chế thay đổi thông tin phân gán để tránh xung đột vận hành.
         */
        InProgress,

        /** 
         * Nhân viên đã hoàn thành triệt để các yêu cầu dọn dẹp hoặc bảo trì được giao.
         * Sự kiện chuyển đổi sang trạng thái này sẽ kích hoạt hệ thống tự động cập nhật mốc thời gian hoàn tất (CompletedAt) 
         * và mở khóa điều kiện để cập nhật trạng thái vệ sinh vật lý của phòng (UC19.2).
         */
        Completed
    }
}