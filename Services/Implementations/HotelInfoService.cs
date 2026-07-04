using Hotel_System.DTOs;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class HotelInfoService : IHotelInfoService
    {
        private readonly IHotelInfoRepository _repo;

        public HotelInfoService(IHotelInfoRepository repo) => _repo = repo;

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