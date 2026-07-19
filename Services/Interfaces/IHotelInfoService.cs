using Hotel_System.DTOs;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.7.I IHotelInfoService Interface]
     * Giao diện quy định các phương thức cấu hình hồ sơ và thông tin cơ bản của khách sạn.
     * Làm hợp đồng cho HotelInfoService thực thi, phục vụ trực tiếp chức năng Quản lý thông tin khách sạn (UC06) và trích xuất thông tin in Hóa đơn (UC18).
     */
    public interface IHotelInfoService
    {
        /** Truy xuất thông tin cấu hình duy nhất của khách sạn dưới dạng DTO để hiển thị lên hệ thống hoặc phục vụ nhúng dữ liệu in ấn folio/hóa đơn (UC06.1). */
        Task<HotelInfoDto?> GetAsync();

        /** Hiệu chỉnh thông tin hồ sơ, địa chỉ liên hệ hoặc thông tin biểu mẫu của khách sạn (UC06.2). */
        Task UpdateAsync(UpdateHotelInfoDto dto);
    }
}