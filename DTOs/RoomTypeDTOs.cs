namespace Hotel_System.DTOs
{
    /**
     * [UC9 - Manage Room Type]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) chứa thông tin cấu hình đầy đủ của một loại phòng/hạng phòng trong hệ thống khách sạn[cite: 1].
     * Phục vụ trực tiếp cho tác vụ hiển thị danh sách hoặc xem thông tin chi tiết của hạng phòng trên giao diện điều hành[cite: 1].
     */
    public class RoomTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }
        public bool IsActive { get; set; }
    }

    /**
     * [UC9.1 - Create Room Type]
     * DTO tiếp nhận dữ liệu đầu vào từ phía Client khi Quản trị viên (Admin) hoặc Quản lý (Hotel Manager) thực hiện thêm mới một loại phòng vào hệ thống[cite: 1].
     * Quá trình xử lý tại tầng nghiệp vụ sẽ áp dụng quy tắc ràng buộc thiết yếu: Tên loại phòng (Name) nhập vào phải là duy nhất, không được trùng lặp[cite: 1].
     */
    public class CreateRoomTypeDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }
    }

    /**
     * [UC9.2 - Edit Room Type]
     * DTO chứa dữ liệu yêu cầu cập nhật khi người dùng có thẩm quyền thực hiện hiệu chỉnh thông tin cấu hình của một loại phòng đang tồn tại[cite: 1].
     * Cho phép điều chỉnh linh hoạt các thông số như tên gọi, mô tả, giá cơ bản (BasePrice), sức chứa tối đa (MaxOccupancy) và trạng thái kích hoạt (IsActive) của hạng phòng[cite: 1].
     */
    public class UpdateRoomTypeDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxOccupancy { get; set; }
        public bool IsActive { get; set; }
    }
}