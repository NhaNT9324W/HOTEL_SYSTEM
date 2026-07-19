using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    /**
     * [UC17 – Record Service Usage]
     * Đối tượng chuyển đổi dữ liệu (Data Transfer Object) đại diện cho thông tin chi tiết của một bản ghi sử dụng dịch vụ bổ sung từ khách hàng[cite: 1].
     * Được sử dụng để hiển thị danh sách các khoản chi tiêu dịch vụ trên giao diện quản lý hoặc folio hóa đơn thanh toán[cite: 1].
     */
    public class ServiceUsageDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string ServiceName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime UsedAt { get; set; }
    }

    /**
     * [UC17 – Record Service Usage]
     * DTO tiếp nhận dữ liệu từ Client gửi lên Request Body khi Tiếp tân ghi nhận một lần tiêu dùng dịch vụ mới của khách hàng[cite: 1].
     * Sử dụng thuộc tính Data Annotations để ràng buộc mã đơn đặt phòng, mã dịch vụ và giới hạn phạm vi số lượng (Quantity) strictly từ 1 đến 100[cite: 1].
     */
    public class AddServiceUsageDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
        public int Quantity { get; set; }
    }

    /**
     * [UC18.1 – Generate Invoice] / [UC18.2 – Calculate Charges]
     * Đối tượng chuyển đổi dữ liệu tổng hợp toàn bộ thông tin chi phí tài chính của khách lưu trú phục vụ quy trình lập và xem trước hóa đơn (Invoice Preview)[cite: 1].
     * Chứa đầy đủ thông số tạm tính bao gồm tiền phòng (RoomCharge) dựa trên số đêm (Nights), tổng tiền dịch vụ (ServiceCharge) kèm danh sách ServiceUsageDto chi tiết và tổng số tiền cuối cùng cần thanh toán (TotalAmount)[cite: 1].
     */
    public class InvoiceDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string GuestName { get; set; } = null!;
        public string RoomNumber { get; set; } = null!;
        public string RoomTypeName { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal RoomCharge { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public List<ServiceUsageDto> Services { get; set; } = new();
        public DateTime IssuedAt { get; set; }
    }
}