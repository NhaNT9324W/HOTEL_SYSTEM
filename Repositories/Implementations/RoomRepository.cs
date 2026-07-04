using Microsoft.EntityFrameworkCore;
using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;

namespace Hotel_System.Repositories.Implementations
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;
        public RoomRepository(AppDbContext context) => _context = context;

        public async Task<List<Room>> GetAllAsync() =>
            await _context.Rooms
                .Include(r => r.RoomType)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

        public async Task<Room?> GetByIdAsync(int id) =>
            await _context.Rooms.Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null) =>
            await _context.Rooms.AnyAsync(r =>
                r.RoomNumber == roomNumber && (excludeId == null || r.Id != excludeId));

        public async Task AddAsync(Room entity) =>
            await _context.Rooms.AddAsync(entity);

        public void Update(Room entity) =>
            _context.Rooms.Update(entity);

        public void Delete(Room entity) =>
            _context.Rooms.Remove(entity);

        public async Task<bool> SaveChangesAsync() =>
            (await _context.SaveChangesAsync()) > 0;
    }
}