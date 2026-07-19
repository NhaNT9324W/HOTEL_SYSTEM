using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.3.I ICheckOutService Interface]
     * Giao diện quy định các phương thức quản lý nghiệp vụ tiêu dùng tiện ích và quyết toán trả phòng.
     * Làm hợp đồng cho CheckOutService thực thi, phục vụ phân hệ Ghi nhận dịch vụ (UC17) và quy trình Trả phòng xuất hóa đơn (UC18).
     */
    public interface ICheckOutService
    {
        /** Lấy danh sách chi tiết các dịch vụ mà khách hàng đã tiêu dùng trong thời gian lưu trú (UC17.1). */
        Task<IEnumerable<ServiceUsageDto>> GetServiceUsagesAsync(int reservationId);

        /** Ghi nhận một lượt tiêu dùng dịch vụ/mini-bar tại phòng hoặc tại quầy kèm kiểm tra trạng thái đơn đặt phòng (UC17.2). */
        Task AddServiceUsageAsync(AddServiceUsageDto dto);

        /** Hủy ghi nhận dịch vụ đã sử dụng trong trường hợp Tiếp tân nhập sai thông tin trước khi chốt hóa đơn. */
        Task RemoveServiceUsageAsync(int serviceUsageId);

        /** Cho phép Tiếp tân hiển thị hoặc in biểu mẫu xem trước tổng chi phí bill để khách đối chiếu trước khi thanh toán (UC18.1). */
        Task<InvoiceDto> PreviewInvoiceAsync(int reservationId);

        /** Quy trình quyết toán trả phòng chính thức, giải phóng phòng vật lý và tự động cập nhật trạng thái dọn dẹp (UC18.2). */
        Task<InvoiceDto> CheckOutAsync(int reservationId);
    }
}