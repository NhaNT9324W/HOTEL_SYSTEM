using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.7 HotelInfoService Implementation]
     * Lớp xử lý nghiệp vụ cấu hình hồ sơ và thông tin cơ bản của khách sạn.
     * Phục vụ trực tiếp chức năng Quản lý thông tin khách sạn (UC06) và trích xuất thông tin in Hóa đơn (UC18).
     */
    public class HotelInfoService : IHotelInfoService
    {
        private readonly IHotelInfoRepository _repo;

        /** Inject Repository thông qua Constructor để tương tác với tầng dữ liệu cấu hình. */
        public HotelInfoService(IHotelInfoRepository repo) => _repo = repo;

        /** Truy xuất thông tin cấu hình duy nhất của khách sạn dưới dạng DTO để hiển thị lên hệ thống (UC06.1). */
        public async Task<HotelInfoDto?> GetAsync()
        {
            var info = await _repo.GetAsync();
            if (info == null) return null;

            return new HotelInfoDto
            {
                Id = info.Id,
                HotelName = info.HotelName,
                Address = info.Address,
                Phone = info.Phone,
                Email = info.Email,
                Website = info.Website,
                Description = info.Description,
                UpdatedAt = info.UpdatedAt
            };
        }

        /** Hiệu chỉnh thông tin hồ sơ, liên hệ hoặc biểu mẫu của khách sạn (UC06.2), tự động ghi vết thời gian sửa đổi. */
        public async Task UpdateAsync(UpdateHotelInfoDto dto)
        {
            var info = await _repo.GetAsync()
                ?? throw new Exception("Hotel information not found");

            info.HotelName = dto.HotelName;
            info.Address = dto.Address;
            info.Phone = dto.Phone;
            info.Email = dto.Email;
            info.Website = dto.Website;
            info.Description = dto.Description;

            await _repo.UpdateAsync(info);
        }
    }
}