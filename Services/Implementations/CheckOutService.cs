using Hotel_System.Data;
using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.3 CheckOutService Implementation]
     * Lớp xử lý nghiệp vụ quản lý tiêu dùng tiện ích và quyết toán trả phòng.
     * Điều phối phân hệ Ghi nhận dịch vụ (UC17) và quy trình Trả phòng xuất hóa đơn (UC18).
     */
    public class CheckOutService : ICheckOutService
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext để thao tác trực tiếp trên các bảng liên quan đến hóa đơn. */
        public CheckOutService(AppDbContext context) => _context = context;

        // ===== GET SERVICE USAGES =====
        /** Lấy danh sách chi tiết các dịch vụ mà khách hàng đã tiêu dùng trong thời gian lưu trú (UC17.1). */
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
        /** 
         * Ghi nhận một lượt tiêu dùng dịch vụ/mini-bar tại phòng hoặc tại quầy (UC17.2).
         * Ràng buộc nghiệp vụ: Chỉ cho phép thêm dịch vụ khi đơn đặt phòng đang ở trạng thái CHECKED_IN.
         */
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
        /** Hủy ghi nhận dịch vụ đã sử dụng trong trường hợp Tiếp tân nhập sai thông tin trước khi chốt hóa đơn. */
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
        /** Cho phép Tiếp tân in hoặc hiển thị biểu mẫu xem trước tổng chi phí bill (UC18.1) để khách đối chiếu trước khi quẹt thẻ. */
        public async Task<InvoiceDto> PreviewInvoiceAsync(int reservationId)
        {
            return await BuildInvoiceDtoAsync(reservationId);
        }

        // ===== CHECK OUT =====
        /** 
         * Quy trình quyết toán trả phòng chính thức (UC18.2).
         * Đồng thời thực hiện 4 tác vụ nguyên tử (Atomic Transaction):
         * 1. Tạo bản ghi Hóa đơn tài chính (Invoice).
         * 2. Chuyển trạng thái đơn đặt phòng sang CHECKED_OUT.
         * 3. Giải phóng trạng thái kinh doanh của phòng vật lý về AVAILABLE.
         * 4. Tự động chuyển trạng thái dọn dẹp của phòng thành DIRTY (cơ sở để hệ thống tự động sinh tác vụ dọn dẹp ở UC19).
         */
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

            // Tính toán số liệu invoice
            var invoiceDto = await BuildInvoiceDtoAsync(reservationId);

            // Lưu Invoice vào DB nếu chưa tồn tại
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

            // Cập nhật trạng thái Reservation
            reservation.Status = ReservationStatus.CHECKED_OUT;
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);

            // Chuyển đổi trạng thái phòng vật lý phục vụ luồng buồng phòng tiếp diễn
            var room = reservation.Room!;
            room.BookingStatus = RoomBookingStatus.AVAILABLE;
            room.HousekeepingStatus = RoomHousekeepingStatus.DIRTY;
            _context.Rooms.Update(room);

            await _context.SaveChangesAsync();

            return invoiceDto;
        }

        // ===== BUILD INVOICE DTO =====
        /** Thuật toán tổng hợp chi phí Folio: Tính tổng tiền phòng (Số đêm x Giá cấu hình) + Tổng tiền dịch vụ phát sinh. */
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