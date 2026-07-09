using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IGuestRepository _guestRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly AppDbContext _context;

        public ReservationService(
            IReservationRepository repo,
            IGuestRepository guestRepo,
            IRoomRepository roomRepo,
            AppDbContext context)
        {
            _repo = repo;
            _guestRepo = guestRepo;
            _roomRepo = roomRepo;
            _context = context;
        }

        public async Task<IEnumerable<ReservationListDto>> GetAllAsync()
        {
            var reservations = await _repo.GetAllAsync();
            return reservations.Select(ToListDto);
        }

        public async Task<ReservationDetailDto?> GetByIdAsync(int id)
        {
            var reservation = await _repo.GetByIdAsync(id);
            return reservation == null ? null : ToDetailDto(reservation);
        }

        public async Task<IEnumerable<ReservationListDto>> SearchAsync(string keyword)
        {
            var reservations = await _repo.SearchAsync(keyword);
            return reservations.Select(ToListDto);
        }

        public async Task<IEnumerable<object>> GetAvailableRoomsAsync(
    DateTime checkIn, DateTime checkOut, int? roomTypeId = null)
        {
            var allRooms = await _roomRepo.GetAllAsync();

            if (roomTypeId.HasValue)
                allRooms = allRooms.Where(r => r.RoomTypeId == roomTypeId.Value).ToList();

            // Lấy danh sách phòng đang có maintenance issue
            var maintenanceRoomIds = await _context.MaintenanceIssues
                .Where(m => m.Status == "PENDING" || m.Status == "IN_PROGRESS")
                .Select(m => m.RoomId)
                .Distinct()
                .ToListAsync();

            var availableRooms = new List<object>();
            foreach (var room in allRooms)
            {
                // Kiểm tra overlap reservation
                var hasOverlap = await _repo.HasOverlappingReservationAsync(
                    room.Id, checkIn, checkOut);

                // Kiểm tra tất cả điều kiện
                if (!hasOverlap
                    && room.BookingStatus == RoomBookingStatus.AVAILABLE
                    && room.HousekeepingStatus == RoomHousekeepingStatus.READY
                    && !maintenanceRoomIds.Contains(room.Id))
                {
                    availableRooms.Add(new
                    {
                        room.Id,
                        room.RoomNumber,
                        room.Floor,
                        RoomTypeName = room.RoomType?.Name ?? "",
                        Price = room.RoomType?.BasePrice ?? 0
                    });
                }
            }

            return availableRooms;
        }

        public async Task CreateAsync(CreateReservationDto dto)
        {
            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new Exception("Check-out date must be after check-in date");

            if (dto.CheckInDate < DateTime.Today)
                throw new Exception("Check-in date cannot be in the past");

            // Kiểm tra phòng có trống không
            var hasOverlap = await _repo.HasOverlappingReservationAsync(
                dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
            if (hasOverlap)
                throw new Exception("Room is not available for the selected dates");

            // Kiểm tra HousekeepingStatus
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == dto.RoomId)
                ?? throw new Exception("Room not found");

            if (room.HousekeepingStatus != RoomHousekeepingStatus.READY)
                throw new Exception("Room is not ready for check-in " +
                    $"(Current status: {room.HousekeepingStatus})");

            // Kiểm tra Maintenance Issue
            var hasMaintenance = await _context.MaintenanceIssues
                .AnyAsync(m => m.RoomId == dto.RoomId
                    && (m.Status == "PENDING" || m.Status == "IN_PROGRESS"));
            if (hasMaintenance)
                throw new Exception("Room has pending maintenance issues");

            // Tìm hoặc tạo Guest theo Phone
            var guest = await _guestRepo.GetByPhoneAsync(dto.GuestPhone);
            if (guest == null)
            {
                guest = new Guest
                {
                    FullName = dto.GuestFullName,
                    Phone = dto.GuestPhone,
                    IdNumber = dto.GuestIdNumber,
                    Email = dto.GuestEmail,
                    CreatedAt = DateTime.UtcNow
                };
                await _guestRepo.AddAsync(guest);
                await _guestRepo.SaveChangesAsync();
            }

            var reservation = new Reservation
            {
                GuestId = guest.Id,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                Status = ReservationStatus.CONFIRMED,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(reservation);
        }

        public async Task UpdateAsync(int id, UpdateReservationDto dto)
        {
            var reservation = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status == ReservationStatus.CHECKED_IN ||
                reservation.Status == ReservationStatus.CHECKED_OUT)
                throw new Exception("Cannot update a reservation that is checked-in or checked-out");

            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new Exception("Check-out date must be after check-in date");

            // Kiểm tra phòng có trống không (trừ reservation hiện tại)
            var hasOverlap = await _repo.HasOverlappingReservationAsync(
                dto.RoomId, dto.CheckInDate, dto.CheckOutDate, id);
            if (hasOverlap)
                throw new Exception("Room is not available for the selected dates");

            reservation.RoomId = dto.RoomId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;

            await _repo.UpdateAsync(reservation);
        }

        public async Task CancelAsync(int id)
        {
            var reservation = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status == ReservationStatus.CHECKED_IN)
                throw new Exception("Cannot cancel a reservation that is checked-in");

            if (reservation.Status == ReservationStatus.CHECKED_OUT)
                throw new Exception("Cannot cancel a reservation that is already checked-out");

            if (reservation.Status == ReservationStatus.CANCELED)
                throw new Exception("Reservation is already cancelled");

            await _repo.UpdateStatusAsync(id, ReservationStatus.CANCELED);
        }

        // ===== MAPPERS =====
        private static ReservationListDto ToListDto(Reservation r) => new()
        {
            Id = r.Id,
            RoomNumber = r.Room?.RoomNumber ?? "",
            GuestName = r.Guest?.FullName ?? "",
            GuestPhone = r.Guest?.Phone ?? "",
            CheckInDate = r.CheckInDate,
            CheckOutDate = r.CheckOutDate,
            Status = r.Status.ToString()
        };

        private static ReservationDetailDto ToDetailDto(Reservation r) => new()
        {
            Id = r.Id,
            RoomNumber = r.Room?.RoomNumber ?? "",
            GuestName = r.Guest?.FullName ?? "",
            GuestPhone = r.Guest?.Phone ?? "",
            GuestIdNumber = r.Guest?.IdNumber ?? "",
            GuestEmail = r.Guest?.Email,
            RoomTypeName = r.Room?.RoomType?.Name ?? "",
            Floor = r.Room?.Floor ?? 0,
            CheckInDate = r.CheckInDate,
            CheckOutDate = r.CheckOutDate,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        };

        public async Task CheckInAsync(int reservationId)
        {
            var reservation = await _repo.GetByIdAsync(reservationId)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status != ReservationStatus.CONFIRMED)
                throw new Exception("Only confirmed reservations can be checked in");

            if (reservation.CheckInDate.Date > DateTime.Today)
                throw new Exception("Check-in date has not arrived yet");

            // Cập nhật Reservation status
            await _repo.UpdateStatusAsync(reservationId, ReservationStatus.CHECKED_IN);

            // Cập nhật Room BookingStatus = OCCUPIED
            var room = await _context.Rooms.FindAsync(reservation.RoomId)
                ?? throw new Exception("Room not found");

            room.BookingStatus = RoomBookingStatus.OCCUPIED;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }
    }
}