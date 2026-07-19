using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.5 RoomRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể Room (Phòng vật lý).
     * Áp dụng mẫu kiến trúc Repository Pattern nhằm cô lập tầng Data Access Layer (DAL) độc lập hoàn toàn với Business Logic Layer (BLL).
     * Cung cấp các phương thức truy vấn tối ưu phục vụ trực tiếp sơ đồ trạng thái phòng thời gian thực, 
     * phân hệ Quản lý phòng (UC07), quy trình Xếp phòng Check-in (UC14), Đổi phòng (UC16) và Phân ban buồng phòng (UC19).
     */
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public RoomRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các phòng vật lý có trong hệ thống khách sạn.
         * Sử dụng kỹ thuật Eager Loading (`Include`) để nạp kèm thông tin cấu hình hạng phòng (RoomType), 
         * tránh triệt để lỗi truy vấn N+1 khi hiển thị danh sách phòng.
         * Kết quả đầu ra được sắp xếp tăng dần theo số phòng (`RoomNumber`) để đồng bộ hiển thị trực quan trên sơ đồ Room Matrix.
         */
        public async Task<List<Room>> GetAllAsync() =>
            await _context.Rooms
                .Include(r => r.RoomType)
                .OrderBy(r => r.RoomNumber)
                .ToListAsync();

        /** 
         * Tìm kiếm thông tin chi tiết một phòng vật lý dựa trên mã định danh số nguyên Primary Key (Id).
         * Đi kèm đầy đủ thông tin định mức hạng phòng (RoomType) để phục vụ cho các logic kiểm tra ràng buộc 
         * về giá phòng hoặc sức chứa tối đa tại tầng Service Layer.
         */
        public async Task<Room?> GetByIdAsync(int id) =>
            await _context.Rooms.Include(r => r.RoomType)
                .FirstOrDefaultAsync(r => r.Id == id);

        /** 
         * Kiểm tra sự tồn tại của số phòng vật lý trên hệ thống, hỗ trợ cơ chế loại trừ ID cụ thể.
         * Phục vụ đắc lực cho Business Rule BR-05: Đảm bảo tính duy nhất của số phòng (Unique Room Number Constraint) 
         * khi Tiếp tân thực hiện thêm mới phòng vật lý hoặc hiệu chỉnh thông tin số phòng hiện hành, tránh xung đột cấu hình.
         */
        public async Task<bool> RoomNumberExistsAsync(string roomNumber, int? excludeId = null) =>
            await _context.Rooms.AnyAsync(r =>
                r.RoomNumber == roomNumber && (excludeId == null || r.Id != excludeId));

        /** 
         * Thực hiện thêm mới một bản ghi phòng vật lý vào ngữ cảnh theo dõi (Change Tracker) của Entity Framework.
         * Thao tác xử lý bất đồng bộ (Async) ở mức cấu hình bộ nhớ trước khi đồng bộ dữ liệu vật lý.
         */
        public async Task AddAsync(Room entity) =>
            await _context.Rooms.AddAsync(entity);

        /** 
         * Đánh dấu thực thể phòng vật lý đã có sự thay đổi thông tin (ví dụ: chuyển trạng thái dọn dẹp, đổi tầng).
         * Hàm chạy đồng bộ (Synchronous) trên Change Tracker và sẽ được ghi nhận chính thức khi Unit of Work kích hoạt lệnh Save.
         */
        public void Update(Room entity) =>
            _context.Rooms.Update(entity);

        /** 
         * Thực hiện xóa vật lý bản ghi phòng khỏi hệ thống cơ sở dữ liệu.
         * Lưu ý nghiệp vụ: Việc xóa vật lý chỉ được tầng Service cho phép khi phòng này không chứa bất kỳ lịch sử đơn đặt phòng (Reservation) 
         * hay tác vụ buồng phòng (HousekeepingTask) nào đang neo giữ để đảm bảo tính toàn vẹn khóa ngoại tham chiếu.
         */
        public void Delete(Room entity) =>
            _context.Rooms.Remove(entity);

        /** 
         * Xác nhận và chính thức cam kết đẩy toàn bộ các thay đổi đang chờ (Thêm, Sửa, Xóa) xuống cơ sở dữ liệu vật lý.
         * Trả về giá trị `true` nếu có ít nhất một bản ghi được tác động thành công trong Transaction, hỗ trợ kiểm soát kết quả đầu ra tại Service.
         */
        public async Task<bool> SaveChangesAsync() =>
            (await _context.SaveChangesAsync()) > 0;
    }
}