using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class HotelInfoRepository : IHotelInfoRepository
    {
        private readonly AppDbContext _context;

        public HotelInfoRepository(AppDbContext context) => _context = context;

        public async Task<HotelInfo?> GetAsync() =>
            await _context.HotelInfos.FirstOrDefaultAsync();

        public async Task UpdateAsync(HotelInfo hotelInfo)
        {
            hotelInfo.UpdatedAt = DateTime.UtcNow;
            _context.HotelInfos.Update(hotelInfo);
            await _context.SaveChangesAsync();
        }
    }
}