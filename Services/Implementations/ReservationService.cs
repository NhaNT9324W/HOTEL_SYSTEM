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
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(IReservationRepository repo, ILogger<ReservationService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<ReservationListDto>> GetAllAsync(string? search)
        {
            var list = await _repo.GetAllAsync(search);
            return list.Select(r => new ReservationListDto
            {
                Id = r.Id,
                RoomNumber = r.Room!.RoomNumber,
                GuestName = r.Guest!.FullName,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                Status = r.Status.ToString()
            }).ToList();
        }

        public async Task<ReservationDetailDto?> GetDetailAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return null;

            return new ReservationDetailDto
            {
                Id = r.Id,
                RoomNumber = r.Room!.RoomNumber,
                RoomTypeName = r.Room.RoomType?.Name ?? "N/A", // giả định RoomType có field Name
                GuestName = r.Guest!.FullName,
                GuestPhone = r.Guest.Phone,
                GuestEmail = r.Guest.Email,
                CheckInDate = r.CheckInDate,
                CheckOutDate = r.CheckOutDate,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(CreateReservationDto dto)
        {
            try
            {
                // --- Root Cause Gate 1: Ngày hợp lệ ---
                if (dto.CheckInDate >= dto.CheckOutDate)
                {
                    _logger.LogWarning("Reservation rejected: invalid date range RoomId={RoomId}", dto.RoomId);
                    return (false, "Ngày check-out phải sau ngày check-in.");
                }

                // --- Root Cause Gate 2: Trạng thái phòng - PHẢI ĐỦ CẢ 2 ĐIỀU KIỆN ---
                var room = await _repo.GetRoomWithStatusAsync(dto.RoomId);
                if (room == null)
                {
                    return (false, "Phòng không tồn tại.");
                }

                bool isBookable = room.BookingStatus == RoomBookingStatus.AVAILABLE
                                   && room.HousekeepingStatus == RoomHousekeepingStatus.CLEAN;

                if (!isBookable)
                {
                    _logger.LogWarning(
                        "Reservation rejected: RoomId={RoomId} BookingStatus={BookingStatus} HousekeepingStatus={HousekeepingStatus}",
                        room.Id, room.BookingStatus, room.HousekeepingStatus);
                    return (false, $"Phòng chưa đủ điều kiện đặt (Booking: {room.BookingStatus}, Housekeeping: {room.HousekeepingStatus}). Cần AVAILABLE + CLEAN.");
                }

                // --- Root Cause Gate 3: Trùng lịch ---
                bool overlap = await _repo.HasOverlappingReservationAsync(dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
                if (overlap)
                {
                    return (false, "Phòng đã có đặt phòng khác trong khoảng thời gian này.");
                }

                // --- Guest: tìm theo phone, chưa có thì tạo mới ---
                var guest = await _repo.FindGuestByPhoneAsync(dto.GuestPhone);
                if (guest == null)
                {
                    guest = new Guest { FullName = dto.GuestFullName, Phone = dto.GuestPhone, Email = dto.GuestEmail };
                    await _repo.AddGuestAsync(guest);
                    await _repo.SaveChangesAsync(); // cần Id của guest trước khi gán vào Reservation
                }

                var reservation = new Reservation
                {
                    RoomId = dto.RoomId,
                    GuestId = guest.Id,
                    CheckInDate = dto.CheckInDate,
                    CheckOutDate = dto.CheckOutDate,
                    Status = ReservationStatus.PENDING
                };

                await _repo.AddAsync(reservation);
                await _repo.SaveChangesAsync();

                return (true, "Đặt phòng thành công.");
            }
            catch (Exception ex)
            {
                // Log chi tiết để trace root cause kể cả khi IDE không hiện lỗi runtime
                _logger.LogError(ex, "Unhandled error while creating reservation for RoomId={RoomId}", dto.RoomId);
                return (false, "Có lỗi hệ thống xảy ra, vui lòng thử lại.");
            }
        }
    }
}