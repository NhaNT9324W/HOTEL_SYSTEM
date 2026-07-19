using Hotel_System.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.12.I IServiceManager Interface]
     * Giao diện quy định các phương thức quản lý nghiệp vụ danh mục dịch vụ tiện ích của khách sạn.
     * Làm hợp đồng cho ServiceManager thực thi, phục vụ phân hệ Quản lý danh mục dịch vụ (UC10) và tra cứu khi ghi nhận tiêu dùng tại quầy (UC17).
     */
    public interface IServiceManager
    {
        /** Lấy toàn bộ danh sách tất cả dịch vụ dưới dạng DTO để hiển thị lên bảng danh mục quản trị (UC10.1). */
        Task<IEnumerable<ServiceDto>> GetAllAsync();

        /** Tìm chi tiết dịch vụ theo ID phục vụ xem thông tin hoặc chuẩn bị hiệu chỉnh cấu hình (UC10.2). */
        Task<ServiceDto?> GetByIdAsync(int id);

        /** Tra cứu nhanh danh sách dịch vụ theo từ khóa linh hoạt tại bộ lọc giao diện. */
        Task<IEnumerable<ServiceDto>> SearchAsync(string keyword);

        /**
         * Khởi tạo cấu hình dịch vụ tiện ích mới vào hệ thống (UC10.3).
         * Áp dụng ràng buộc nghiệp vụ kiểm tra trùng tên dịch vụ trên hệ thống nhằm ngăn chặn dữ liệu rác.
         */
        Task CreateAsync(CreateServiceDto dto);

        /** 
         * Hiệu chỉnh thông tin dịch vụ (tên, mô tả, giá niêm yết, trạng thái).
         * Thực hiện kiểm tra đối chiếu trùng tên ngoại trừ mã định danh của chính dịch vụ hiện tại.
         */
        Task UpdateAsync(UpdateServiceDto dto);

        /** 
         * Xóa vật lý bản ghi dịch vụ ra khỏi hệ thống danh mục.
         * Ràng buộc nghiệp vụ: Chặn xóa nếu dịch vụ đã phát sinh lịch sử tiêu dùng (Folio) để bảo toàn dữ liệu kế toán.
         */
        Task DeleteAsync(int id);
    }
}