using Microsoft.EntityFrameworkCore;
using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;

namespace Hotel_System.Repositories.Implementations
{
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly AppDbContext _context;
        public RoomTypeRepository(AppDbContext context) => _context = context;

        public async Task<List<RoomType>> GetAllAsync() =>
            await _context.RoomTypes.OrderByDescending(r => r.Id).ToListAsync();

        public async Task<RoomType?> GetByIdAsync(int id) =>
            await _context.RoomTypes.FindAsync(id);

        public async Task AddAsync(RoomType entity) =>
            await _context.RoomTypes.AddAsync(entity);

        public void Update(RoomType entity) =>
            _context.RoomTypes.Update(entity);

        public void Delete(RoomType entity) =>
            _context.RoomTypes.Remove(entity);

        public async Task<bool> SaveChangesAsync() =>
            (await _context.SaveChangesAsync()) > 0;
    }
}