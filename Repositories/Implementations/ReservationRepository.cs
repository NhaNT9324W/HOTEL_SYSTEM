using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Repositories.Implementations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Reservation>> GetAllAsync() =>
            await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<Reservation?> GetByIdAsync(int id) =>
            await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<IEnumerable<Reservation>> SearchAsync(string keyword) =>
            await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .Where(r => r.Guest!.FullName.Contains(keyword) ||
                            r.Guest.Phone.Contains(keyword) ||
                            r.Room!.RoomNumber.Contains(keyword))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<bool> HasOverlappingReservationAsync(
            int roomId, DateTime checkIn, DateTime checkOut, int? excludeId = null) =>
            await _context.Reservations
                .Where(r => r.RoomId == roomId
                    && r.Id != (excludeId ?? 0)
                    && r.Status != ReservationStatus.CANCELED
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn)
                .AnyAsync();

        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, ReservationStatus status)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Status = status;
                reservation.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}