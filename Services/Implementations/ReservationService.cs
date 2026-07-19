using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.9 ReservationService Implementation]
     * Lớp xử lý nghiệp vụ lõi điều phối vòng đời lưu trú của khách hàng.
     * Quản lý chặt chẽ luồng Đặt phòng (UC13), Nhận phòng (UC14), điều chỉnh lịch trình và kiểm tra phòng trống khả dụng.
     */
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repo;
        private readonly IGuestRepository _guestRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly AppDbContext _context;

        /** Inject phối hợp các Repository liên quan và Context phụ trợ thông qua Constructor. */
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

        /** Lấy toàn bộ danh sách đơn đặt phòng dưới dạng DTO rút gọn, phục vụ màn hình tổng quan (UC13.1). */
        public async Task<IEnumerable<ReservationListDto>> GetAllAsync()
        {
            var reservations = await _repo.GetAllAsync();
            return reservations.Select(ToListDto);
        }

        /** Tra cứu thông tin chi tiết một đơn đặt phòng phục vụ thủ tục Check-in hoặc đối soát dữ liệu Folio. */
        public async Task<ReservationDetailDto?> GetByIdAsync(int id)
        {
            var reservation = await _repo.GetByIdAsync(id);
            return reservation == null ? null : ToDetailDto(reservation);
        }

        /** Tìm kiếm đơn đặt phòng theo từ khóa linh hoạt (Tên khách, Số điện thoại, Số phòng) tại bộ lọc. */
        public async Task<IEnumerable<ReservationListDto>> SearchAsync(string keyword)
        {
            var reservations = await _repo.SearchAsync(keyword);
            return reservations.Select(ToListDto);
        }

        /**
         * Thuật toán lọc và tìm kiếm phòng trống khả dụng thời gian thực.
         * Áp dụng đồng thời 4 bộ lọc nghiêm ngặt: Không trùng lịch đặt, BookingStatus tự do, phòng đã dọn sạch (READY), và không vướng sự cố bảo trì.
         */
        public async Task<IEnumerable<object>> GetAvailableRoomsAsync(
            DateTime checkIn, DateTime checkOut, int? roomTypeId = null)
        {
            var allRooms = await _roomRepo.GetAllAsync();

            if (roomTypeId.HasValue)
                allRooms = allRooms.Where(r => r.RoomTypeId == roomTypeId.Value).ToList();

            // Lấy danh sách ID phòng đang có sự cố bảo trì chưa xử lý xong
            var maintenanceRoomIds = await _context.MaintenanceIssues
                .Where(m => m.Status == "PENDING" || m.Status == "IN_PROGRESS")
                .Select(m => m.RoomId)
                .Distinct()
                .ToListAsync();

            var availableRooms = new List<object>();
            foreach (var room in allRooms)
            {
                // Kiểm tra xung đột lịch đặt (Double-booking)
                var hasOverlap = await _repo.HasOverlappingReservationAsync(
                    room.Id, checkIn, checkOut);

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

        /** 
         * Xử lý tạo mới đơn đặt phòng tại quầy Tiền sảnh (UC13).
         * Ràng buộc nghiệp vụ (BR): Xác minh tính hợp lệ của thời gian, chặn phòng hỏng/chưa dọn, và tự động liên kết hoặc tạo mới hồ sơ khách hàng.
         */
        public async Task CreateAsync(CreateReservationDto dto)
        {
            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new Exception("Check-out date must be after check-in date");

            if (dto.CheckInDate < DateTime.Today)
                throw new Exception("Check-in date cannot be in the past");

            // Kiểm tra an toàn xung đột lịch phòng vật lý
            var hasOverlap = await _repo.HasOverlappingReservationAsync(
                dto.RoomId, dto.CheckInDate, dto.CheckOutDate);
            if (hasOverlap)
                throw new Exception("Room is not available for the selected dates");

            // Kiểm tra tiêu chuẩn buồng phòng trước khi cho thuê
            var room = await _context.Rooms
                .Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == dto.RoomId)
                ?? throw new Exception("Room not found");

            if (room.HousekeepingStatus != RoomHousekeepingStatus.READY)
                throw new Exception("Room is not ready for check-in " +
                    $"(Current status: {room.HousekeepingStatus})");

            // Chặn đặt nếu phòng đang vướng sự cố kỹ thuật kỹ sư chưa nghiệm thu
            var hasMaintenance = await _context.MaintenanceIssues
                .AnyAsync(m => m.RoomId == dto.RoomId
                    && (m.Status == "PENDING" || m.Status == "IN_PROGRESS"));
            if (hasMaintenance)
                throw new Exception("Room has pending maintenance issues");

            // Quy trình tra cứu tự động: Tìm khách cũ qua SĐT, nếu mới hoàn toàn sẽ tự tạo hồ sơ Guest
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

        /** Cập nhật lịch trình hoặc thay đổi phòng vật lý trước khi Check-in, loại trừ chính đơn hiện tại khi check trùng lịch. */
        public async Task UpdateAsync(int id, UpdateReservationDto dto)
        {
            var reservation = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status == ReservationStatus.CHECKED_IN ||
                reservation.Status == ReservationStatus.CHECKED_OUT)
                throw new Exception("Cannot update a reservation that is checked-in or checked-out");

            if (dto.CheckInDate >= dto.CheckOutDate)
                throw new Exception("Check-out date must be after check-in date");

            var hasOverlap = await _repo.HasOverlappingReservationAsync(
                dto.RoomId, dto.CheckInDate, dto.CheckOutDate, id);
            if (hasOverlap)
                throw new Exception("Room is not available for the selected dates");

            reservation.RoomId = dto.RoomId;
            reservation.CheckInDate = dto.CheckInDate;
            reservation.CheckOutDate = dto.CheckOutDate;

            await _repo.UpdateAsync(reservation);
        }

        /** Hủy bỏ đơn đặt phòng (CANCELED) phục vụ luồng xử lý yêu cầu hủy của khách, đảm bảo chặt chẽ điều kiện trạng thái. */
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

        /** 
         * Quy trình làm thủ tục Nhận phòng chính thức (UC14 - Check-in).
         * Xác nhận trạng thái lưu trú thành CHECKED_IN và đồng thời chuyển BookingStatus của phòng sang OCCUPIED (Đang có khách).
         */
        public async Task CheckInAsync(int reservationId)
        {
            var reservation = await _repo.GetByIdAsync(reservationId)
                ?? throw new Exception("Reservation not found");

            if (reservation.Status != ReservationStatus.CONFIRMED)
                throw new Exception("Only confirmed reservations can be checked in");

            if (reservation.CheckInDate.Date > DateTime.Today)
                throw new Exception("Check-in date has not arrived yet");

            // Kích hoạt cập nhật trạng thái đơn đặt
            await _repo.UpdateStatusAsync(reservationId, ReservationStatus.CHECKED_IN);

            // Cập nhật trạng thái phòng vật lý tương ứng để hiển thị lên Room Matrix
            var room = await _context.Rooms.FindAsync(reservation.RoomId)
                ?? throw new Exception("Room not found");

            room.BookingStatus = RoomBookingStatus.OCCUPIED;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

        // ===== MAPPERS =====
        /** Ánh xạ dữ liệu thô sang DTO gọn nhẹ dùng cho danh sách hiển thị bảng. */
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

        /** Ánh xạ dữ liệu chi tiết cấu trúc hình cây của đơn đặt phòng kèm định danh cá nhân khách hàng. */
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