using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.2 GuestRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể Guest (Khách lưu trú).
     * Áp dụng mẫu kiến trúc Repository Pattern để cô lập logic truy xuất dữ liệu lớp nền.
     * Hỗ trợ đắc lực cho phân hệ Quản lý hồ sơ khách hàng (UC15) và quy trình Đặt phòng tiền sảnh (UC13).
     */
    public class GuestRepository : IGuestRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GuestRepository> _logger;

        /** 
         * Khởi tạo Repository với cơ chế Dependency Injection.
         * Tích hợp thêm ILogger để phục vụ công tác giám sát, ghi vết vận hành (Operational Auditing).
         */
        public GuestRepository(AppDbContext context, ILogger<GuestRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /** 
         * Lấy danh sách khách hàng có bộ lọc tìm kiếm và phân trang/sắp xếp.
         * Áp dụng bộ lọc Root Cause Gate: Chỉ truy vấn các hồ sơ chưa bị xóa mềm (!g.IsDeleted).
         * Hỗ trợ tìm kiếm tương đối không phân biệt hoa thường dựa trên FullName, Phone và Email.
         */
        public async Task<List<Guest>> GetAllAsync(string? search)
        {
            var query = _context.Guests.Where(g => !g.IsDeleted).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(g =>
                    g.FullName.ToLower().Contains(search) ||
                    g.Phone.Contains(search) ||
                    (g.Email != null && g.Email.ToLower().Contains(search)));
            }

            // Sắp xếp mặc định theo thời gian tạo mới nhất đưa lên đầu
            return await query.OrderByDescending(g => g.CreatedAt).ToListAsync();
        }

        /** 
         * Thực hiện xóa mềm (Soft-delete) hồ sơ khách hàng thay vì xóa vật lý khỏi database.
         * Giúp bảo toàn tính toàn vẹn dữ liệu cho các hóa đơn (Invoice) và đơn đặt phòng (Reservation) lịch sử.
         */
        public void SoftDelete(Guest guest)
        {
            guest.IsDeleted = true;
            _context.Guests.Update(guest);
            _logger.LogInformation("Guest soft-deleted: Id={Id}, Phone={Phone}", guest.Id, guest.Phone);
        }

        /** Tìm kiếm thông tin chi tiết một khách hàng dựa trên mã định danh Primary Key (Id). */
        public async Task<Guest?> GetByIdAsync(int id) =>
            await _context.Guests.FindAsync(id);

        /** 
         * Tìm kiếm khách hàng theo số điện thoại, hỗ trợ loại trừ ID cụ thể.
         * Phục vụ đắc lực cho tầng nghiệp vụ kiểm tra Unique Phone Constraint (BR-22): 
         * Đảm bảo số điện thoại không bị trùng lặp khi thêm mới hoặc cập nhật hồ sơ khách hàng khác.
         */
        public async Task<Guest?> GetByPhoneAsync(string phone, int? excludeId = null)
        {
            var query = _context.Guests.Where(g => g.Phone == phone);
            if (excludeId.HasValue)
                query = query.Where(g => g.Id != excludeId.Value);

            return await query.FirstOrDefaultAsync();
        }

        /** Đếm tổng số lượng đơn đặt phòng của một khách hàng, hỗ trợ thống kê tần suất hoặc kiểm tra ràng buộc trước khi xóa. */
        public async Task<int> CountReservationsAsync(int guestId) =>
            await _context.Reservations.CountAsync(r => r.GuestId == guestId);

        /** 
         * Xếp hàng thực thể khách hàng để chuẩn bị thêm mới vào ngữ cảnh dữ liệu (Change Tracker).
         * Hàm không gọi SaveChanges trực tiếp nhằm tối ưu hóa Unit of Work khi thực hiện nhiều tác vụ.
         */
        public async Task AddAsync(Guest guest)
        {
            await _context.Guests.AddAsync(guest);
            _logger.LogInformation("Guest queued for creation: Phone={Phone}", guest.Phone);
        }

        /** Đánh dấu thực thể khách hàng đã có sự thay đổi thông tin để Entity Framework cập nhật trong phiên làm việc. */
        public void Update(Guest guest)
        {
            _context.Guests.Update(guest);
            _logger.LogInformation("Guest queued for update: Id={Id}", guest.Id);
        }

        /** Xác nhận và chính thức đẩy toàn bộ các thay đổi đang chờ (Thêm, Sửa, Xóa mềm) xuống cơ sở dữ liệu vật lý dưới dạng một Transaction. */
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}