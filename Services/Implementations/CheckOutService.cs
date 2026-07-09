using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Services.Implementations
{
    public class CheckOutService : ICheckOutService
    {
        private readonly AppDbContext _context;

        public CheckOutService(AppDbContext context) => _context = context;

        // ===== GET SERVICE USAGES =====
        public async Task<IEnumerable<ServiceUsageDto>> GetServiceUsagesAsync(int reservationId)
        {
            return await _context.ServiceUsages
                .Include(s => s.Service)
                .Where(s => s.ReservationId == reservationId)
                .Select(s => new ServiceUsageDto
                {
                    Id = s.Id,
                    ReservationId = s.ReservationId,
                    ServiceName = s.Service!.ServiceName,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice,
                    UsedAt = s.UsedAt
                })
                .ToListAsync();
        }

        // ===== ADD SERVICE USAGE =====
        public async Task AddServiceUsageAsync(AddServiceUsageDto dto)
        {
            var reservation = await _context.Reservations.FindAsync(dto.ReservationId)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status != ReservationStatus.CHECKED_IN)
                throw new Exception("Can only add services to checked-in reservations");

            var service = await _context.Services.FindAsync(dto.ServiceId)
                ?? throw new Exception("Service not found");

            var totalPrice = service.Price * dto.Quantity;

            var usage = new ServiceUsage
            {
                ReservationId = dto.ReservationId,
                ServiceId = dto.ServiceId,
                Quantity = dto.Quantity,
                UnitPrice = service.Price,
                TotalPrice = totalPrice,
                UsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ServiceUsages.AddAsync(usage);
            await _context.SaveChangesAsync();
        }

        // ===== REMOVE SERVICE USAGE =====
        public async Task RemoveServiceUsageAsync(int serviceUsageId)
        {
            var usage = await _context.ServiceUsages
                .Include(s => s.Reservation)
                .FirstOrDefaultAsync(s => s.Id == serviceUsageId)
                ?? throw new Exception("Service usage not found");

            if (usage.Reservation!.Status != ReservationStatus.CHECKED_IN)
                throw new Exception("Cannot remove service from non-checked-in reservation");

            _context.ServiceUsages.Remove(usage);
            await _context.SaveChangesAsync();
        }

        // ===== PREVIEW INVOICE =====
        public async Task<InvoiceDto> PreviewInvoiceAsync(int reservationId)
        {
            return await BuildInvoiceDtoAsync(reservationId);
        }

        // ===== CHECK OUT =====
        public async Task<InvoiceDto> CheckOutAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(r => r.Id == reservationId)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status != ReservationStatus.CHECKED_IN)
                throw new Exception("Only checked-in reservations can be checked out");

            // Tính invoice
            var invoiceDto = await BuildInvoiceDtoAsync(reservationId);

            // Lưu Invoice vào DB
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.ReservationId == reservationId);

            if (existingInvoice == null)
            {
                var invoice = new Invoice
                {
                    ReservationId = reservationId,
                    RoomCharge = invoiceDto.RoomCharge,
                    ServiceCharge = invoiceDto.ServiceCharge,
                    TotalAmount = invoiceDto.TotalAmount,
                    Nights = invoiceDto.Nights,
                    IssuedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Invoices.AddAsync(invoice);
            }

            // Cập nhật Reservation status
            reservation.Status = ReservationStatus.CHECKED_OUT;
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);

            // Cập nhật Room status
            var room = reservation.Room!;
            room.BookingStatus = RoomBookingStatus.AVAILABLE;
            room.HousekeepingStatus = RoomHousekeepingStatus.DIRTY;
            _context.Rooms.Update(room);

            await _context.SaveChangesAsync();

            return invoiceDto;
        }

        // ===== BUILD INVOICE DTO =====
        private async Task<InvoiceDto> BuildInvoiceDtoAsync(int reservationId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(r => r.Id == reservationId)
                ?? throw new Exception("Reservation not found");

            var serviceUsages = await _context.ServiceUsages
                .Include(s => s.Service)
                .Where(s => s.ReservationId == reservationId)
                .ToListAsync();

            var nights = (int)(reservation.CheckOutDate - reservation.CheckInDate).TotalDays;
            var roomCharge = nights * (reservation.Room?.RoomType?.BasePrice ?? 0);
            var serviceCharge = serviceUsages.Sum(s => s.TotalPrice);
            var totalAmount = roomCharge + serviceCharge;

            return new InvoiceDto
            {
                ReservationId = reservationId,
                GuestName = reservation.Guest?.FullName ?? "",
                RoomNumber = reservation.Room?.RoomNumber ?? "",
                RoomTypeName = reservation.Room?.RoomType?.Name ?? "",
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                Nights = nights,
                RoomCharge = roomCharge,
                ServiceCharge = serviceCharge,
                TotalAmount = totalAmount,
                Services = serviceUsages.Select(s => new ServiceUsageDto
                {
                    Id = s.Id,
                    ReservationId = s.ReservationId,
                    ServiceName = s.Service?.ServiceName ?? "",
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    TotalPrice = s.TotalPrice,
                    UsedAt = s.UsedAt
                }).ToList(),
                IssuedAt = DateTime.UtcNow
            };
        }
    }
}