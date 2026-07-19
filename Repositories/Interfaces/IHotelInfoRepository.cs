using Hotel_System.Entities;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.3.I IHotelInfoRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cấu hình khách sạn.
     * Phục vụ trực tiếp cho chức năng Quản lý thông tin khách sạn (UC06) và trích xuất thông tin in Hóa đơn (UC18).
     */
    public interface IHotelInfoRepository
    {
        /** Truy xuất bản ghi thông tin cấu hình duy nhất của khách sạn trên hệ thống. */
        Task<HotelInfo?> GetAsync();

        /** Cập nhật thông tin hồ sơ, liên hệ hoặc biểu mẫu của khách sạn, tự động ghi vết UpdatedAt. */
        Task UpdateAsync(HotelInfo hotelInfo);
    }
}