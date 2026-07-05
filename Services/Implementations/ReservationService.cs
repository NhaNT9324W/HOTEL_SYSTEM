using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IGuestRepository _guestRepo;
        private readonly IRoomRepository _roomRepo;

        public ReservationService(
            IReservationRepository repo,
            IGuestRepository guestRepo,
            IRoomRepository roomRepo)
        {
            _repo = repo;
            _guestRepo = guestRepo;
            _roomRepo = roomRepo;
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
            DateTime checkIn, DateTime checkOut)
        {
            var allRooms = await _roomRepo.GetAllAsync();

            var availableRooms = new List<object>();
            foreach (var room in allRooms)
            {
                var hasOverlap = await _repo.HasOverlappingReservationAsync(
                    room.Id, checkIn, checkOut);

                if (!hasOverlap && room.BookingStatus == RoomBookingStatus.AVAILABLE)
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
            // Validate ngày
            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new Exception("Check-out date must be after check-in date");

            if (dto.CheckInDate < DateTime.Today)
                throw new Exception("Check-in date cannot be in the past");

            // Kiểm tra phòng có trống không
            var hasOverlap = await _repo.HasOverlappingReservationAsync(
                dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
            if (hasOverlap)
                throw new Exception("Room is not available for the selected dates");

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
                await _guestRepo.SaveChangesAsync(); // Thêm dòng này
            }

            // Tạo Reservation
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
    }
}