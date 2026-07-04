using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IHotelInfoService
    {
        Task<HotelInfoDto?> GetAsync();
        Task UpdateAsync(UpdateHotelInfoDto dto);
    }
}