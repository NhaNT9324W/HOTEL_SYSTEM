using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationRepository> _logger;

        public ReservationRepository(AppDbContext context, ILogger<ReservationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Reservation>> GetAllAsync(string? search)
        {
            var query = _context.Reservations
                .Include(r => r.Room).ThenInclude(rm => rm!.RoomType)
                .Include(r => r.Guest)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(r =>
                    r.Room!.RoomNumber.ToLower().Contains(search) ||
                    r.Guest!.FullName.ToLower().Contains(search) ||
                    r.Guest!.Phone.Contains(search));
            }

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(int id) =>
            await _context.Reservations
                .Include(r => r.Room).ThenInclude(rm => rm!.RoomType)
                .Include(r => r.Guest)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<Room?> GetRoomWithStatusAsync(int roomId) =>
            await _context.Rooms.FindAsync(roomId);

        public async Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Root Cause note: overlap khi (start1 < end2) && (start2 < end1)
            return await _context.Reservations.AnyAsync(r =>
                r.RoomId == roomId &&
                r.Status != Entities.Enums.ReservationStatus.CANCELED &&
                r.CheckInDate < checkOut &&
                checkIn < r.CheckOutDate);
        }

        public async Task<Guest?> FindGuestByPhoneAsync(string phone) =>
            await _context.Guests.FirstOrDefaultAsync(g => g.Phone == phone);

        public async Task AddGuestAsync(Guest guest) => await _context.Guests.AddAsync(guest);

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            _logger.LogInformation("Reservation queued for RoomId={RoomId}, GuestId={GuestId}", reservation.RoomId, reservation.GuestId);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}