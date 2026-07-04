using Hotel_System.Entities;

namespace Hotel_System.Repositories.Interfaces
{
    public interface IHotelInfoRepository
    {
        Task<HotelInfo?> GetAsync();
        Task UpdateAsync(HotelInfo hotelInfo);
    }
}