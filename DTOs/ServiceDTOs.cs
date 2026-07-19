using Hotel_System.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC10 – Manage Hotel Services]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) đại diện cho cấu trúc thông tin đầy đủ của một dịch vụ khách sạn[cite: 1].
     * Phục vụ hiển thị danh sách (UC10.1) hoặc thông tin chi tiết (UC10.2) trên hệ thống giao diện điều hành của Admin và Quản lý[cite: 1].
     */
    public class ServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /**
     * [UC10.3 – Create Service]
     * DTO tiếp nhận dữ liệu từ Client gửi lên Request Body khi lập danh mục dịch vụ mới cho khách sạn[cite: 1].
     * Sử dụng thuộc tính Data Annotations để kiểm tra định dạng và thực thi nghiêm ngặt quy tắc nghiệp vụ Business Rule BR-35: 
     * Giá dịch vụ (Price) bắt buộc phải nhập và phải là một giá trị số thực dương strictly lớn hơn 0[cite: 1].
     */
    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }

    /**
     * [UC10.4 – Edit Service]
     * DTO tiếp nhận dữ liệu từ giao diện khi người dùng có thẩm quyền thực hiện hiệu chỉnh thông tin cấu hình của một dịch vụ đang tồn tại[cite: 1].
     * Cho phép cập nhật lại tên gọi dịch vụ (ServiceName), mô tả công việc (Description), biểu phí (Price) tuân thủ BR-35 và trạng thái hoạt động (Status)[cite: 1].
     */
    public class UpdateServiceDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public ServiceStatus Status { get; set; }
    }
}