using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.4 ReservationRepository Implementation]
     * Lớp triển khai các phương thức truy xuất và thao tác dữ liệu cho thực thể Reservation (Đặt phòng).
     * Áp dụng mẫu kiến trúc Repository Pattern để bao bọc các câu lệnh LINQ và tương tác Entity Framework Core.
     * Đây là phân hệ dữ liệu lõi phục vụ trực tiếp cho các luồng nghiệp vụ: Đặt phòng (UC13), Nhận phòng (UC14), 
     * Đổi phòng (UC16) và Trả phòng quyết toán hóa đơn (UC18).
     */
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public ReservationRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các đơn đặt phòng trong hệ thống.
         * Sử dụng kỹ thuật Eager Loading (`Include`, `ThenInclude`) để nạp kèm thông tin Khách hàng, Phòng và Hạng phòng, 
         * giúp tối ưu hóa hiệu năng, tránh triệt để lỗi truy vấn N+1.
         * Kết quả được sắp xếp giảm dần theo thời điểm tạo (Mới nhất lên đầu).
         */
        public async Task<IEnumerable<Reservation>> GetAllAsync() =>
            await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        /** 
         * Tìm kiếm một đơn đặt phòng cụ thể theo mã định danh Primary Key (Id).
         * Đi kèm đầy đủ thông tin liên kết (Guest, Room, RoomType) để phục vụ hiển thị chi tiết phôi đặt phòng 
         * hoặc cung cấp dữ liệu gốc làm cơ sở tính toán Folio/Hóa đơn khi khách Check-out (UC18).
         */
        public async Task<Reservation?> GetByIdAsync(int id) =>
            await _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                    .ThenInclude(r => r!.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

        /** 
         * Tìm kiếm nâng cao danh sách đơn đặt phòng dựa trên từ khóa linh hoạt.
         * Hỗ trợ Tiếp tân tra cứu nhanh trên giao diện thông qua 3 tiêu chí: Tên khách hàng (FullName), 
         * Số điện thoại khách (Phone) hoặc Số phòng vật lý (RoomNumber).
         */
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

        /** 
         * Thuật toán kiểm tra trùng lịch đặt phòng (Overlapping Schedule Validation).
         * Phục vụ bộ quy tắc nghiệp vụ nghiêm ngặt BR-15: Ngăn chặn hiện tượng Double-booking (Một phòng bị đặt trùng thời gian).
         * 
         * Logic loại trừ:
         * 1. Bỏ qua các đơn đặt phòng đã bị hủy (`Status != ReservationStatus.CANCELED`).
         * 2. Bỏ qua chính đơn đặt phòng hiện tại nếu đang trong luồng cập nhật/sửa lịch (`r.Id != excludeId`).
         * 
         * Công thức toán học xác định giao thoa khoảng thời gian: (Thời điểm Check-in mới < Thời điểm Check-out cũ) 
         * VÀ (Thời điểm Check-out mới > Thời điểm Check-in cũ).
         */
        public async Task<bool> HasOverlappingReservationAsync(
            int roomId, DateTime checkIn, DateTime checkOut, int? excludeId = null) =>
            await _context.Reservations
                .Where(r => r.RoomId == roomId
                    && r.Id != (excludeId ?? 0)
                    && r.Status != ReservationStatus.CANCELED
                    && r.CheckInDate < checkOut
                    && r.CheckOutDate > checkIn)
                .AnyAsync();

        /** 
         * Thực hiện thêm mới một phôi đơn đặt phòng vào cơ sở dữ liệu (UC13).
         * Xác nhận lưu thay đổi tức thời xuống database thông qua SaveChangesAsync.
         */
        public async Task AddAsync(Reservation reservation)
        {
            await _context.Reservations.AddAsync(reservation);
            await _context.SaveChangesAsync();
        }

        /** 
         * Cập nhật toàn diện thông tin thay đổi của đơn đặt phòng (ví dụ: thay đổi ngày đi/đến, đổi phòng gán).
         * Trước khi đồng bộ, hệ thống tự động ghi nhận mốc thời gian `UpdatedAt` theo chuẩn UTC để phục vụ kiểm toán dữ liệu.
         */
        public async Task UpdateAsync(Reservation reservation)
        {
            reservation.UpdatedAt = DateTime.UtcNow;
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        /** 
         * Cập nhật nhanh trạng thái đơn đặt phòng (Targeted Status Update).
         * Tối ưu hiệu năng khi Tiếp tân thực hiện các tác vụ chuyển trạng thái đơn hàng đơn lẻ như: 
         * Xác nhận cọc (CONFIRMED), Tiếp nhận buồng (CHECKED_IN), hoặc Hủy đơn (CANCELED) mà không cần gửi toàn bộ object thực thể.
         */
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