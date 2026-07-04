using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class GuestRepository : IGuestRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GuestRepository> _logger;

        public GuestRepository(AppDbContext context, ILogger<GuestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Guest>> GetAllAsync(string? search)
        {
            // Root Cause Gate: mặc định không hiện khách đã soft-delete
            var query = _context.Guests.Where(g => !g.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(g =>
                    g.FullName.ToLower().Contains(search) ||
                    g.Phone.Contains(search) ||
                    (g.Email != null && g.Email.ToLower().Contains(search)));
            }

            return await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
        }

        public void SoftDelete(Guest guest)
        {
            guest.IsDeleted = true;
            _context.Guests.Update(guest);
            _logger.LogInformation("Guest soft-deleted: Id={Id}, Phone={Phone}", guest.Id, guest.Phone);
        }

        public async Task<Guest?> GetByIdAsync(int id) =>
            await _context.Guests.FindAsync(id);

        public async Task<Guest?> GetByPhoneAsync(string phone, int? excludeId = null)
        {
            var query = _context.Guests.Where(g => g.Phone == phone);
            if (excludeId.HasValue)
                query = query.Where(g => g.Id != excludeId.Value);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<int> CountReservationsAsync(int guestId) =>
            await _context.Reservations.CountAsync(r => r.GuestId == guestId);

        public async Task AddAsync(Guest guest)
        {
            await _context.Guests.AddAsync(guest);
            _logger.LogInformation("Guest queued for creation: Phone={Phone}", guest.Phone);
        }

        public void Update(Guest guest)
        {
            _context.Guests.Update(guest);
            _logger.LogInformation("Guest queued for update: Id={Id}", guest.Id);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}