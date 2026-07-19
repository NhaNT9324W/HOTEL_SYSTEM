using Microsoft.EntityFrameworkCore;
using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.6 RoomTypeRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể RoomType (Phân loại hạng phòng).
     * Áp dụng mẫu kiến trúc Repository Pattern để cô lập tầng truy xuất dữ liệu (Data Access Layer) độc lập với tầng xử lý nghiệp vụ (Business Logic Layer).
     * Cung cấp các thao tác dữ liệu nền tảng phục vụ trực tiếp cho mô-đun Quản lý hạng phòng (UC09), 
     * đồng thời làm cơ sở dữ liệu tham chiếu cốt lõi cho phân hệ Đặt phòng (UC13) và Quản lý phòng vật lý (UC07).
     */
    public class RoomTypeRepository : IRoomTypeRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public RoomTypeRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các hạng phòng hiện có trong hệ thống khách sạn.
         * Kết quả đầu ra được sắp xếp giảm dần theo mã định danh (`Id`) giúp người quản trị dễ dàng theo dõi các hạng phòng mới 
         * được cấu hình gần đây nhất trên giao diện hiển thị danh mục (UC09.1).
         */
        public async Task<List<RoomType>> GetAllAsync() =>
            await _context.RoomTypes.OrderByDescending(r => r.Id).ToListAsync();

        /** 
         * Tìm kiếm thông tin chi tiết của một phân loại hạng phòng cụ thể dựa trên mã định danh Primary Key (Id).
         * Hỗ trợ đắc lực cho tầng nghiệp vụ kiểm tra cấu hình hoặc tải dữ liệu gốc lên biểu mẫu chỉnh sửa của quy trình Cập nhật hạng phòng (UC09.2).
         */
        public async Task<RoomType?> GetByIdAsync(int id) =>
            await _context.RoomTypes.FindAsync(id);

        /** 
         * Thực hiện thêm mới một bản ghi cấu hình hạng phòng vào ngữ cảnh theo dõi (Change Tracker) của Entity Framework (UC09.3).
         * Thao tác xử lý bất đồng bộ (Async) ở mức bộ nhớ tạm trước khi chính thức đồng bộ xuống hệ quản trị cơ sở dữ liệu.
         */
        public async Task AddAsync(RoomType entity) =>
            await _context.RoomTypes.AddAsync(entity);

        /** 
         * Đánh dấu thực thể hạng phòng đã có sự thay đổi thông tin (ví dụ: điều chỉnh biểu phí cơ bản BasePrice hoặc số lượng khách tối đa MaxOccupancy).
         * Hàm chạy đồng bộ trên Change Tracker và sẽ được ghi nhận chính thức khi Unit of Work kích hoạt lệnh lưu thay đổi.
         */
        public void Update(RoomType entity) =>
            _context.RoomTypes.Update(entity);

        /** 
         * Thực hiện xóa vật lý bản ghi cấu hình hạng phòng khỏi hệ thống cơ sở dữ liệu.
         * Lưu ý nghiệp vụ: Việc xóa vật lý chỉ được thực thi thành công khi hạng phòng này không chứa bất kỳ phòng vật lý (Room) nào đang liên kết ngoại tham chiếu,
         * nhằm tuân thủ chặt chẽ ràng buộc toàn vẹn dữ liệu (Referential Integrity Constraints) trong database.
         */
        public void Delete(RoomType entity) =>
            _context.RoomTypes.Remove(entity);

        /** 
         * Xác nhận và chính thức cam kết đẩy toàn bộ các thay đổi đang chờ (Thêm, Sửa, Xóa) xuống cơ sở dữ liệu vật lý dưới dạng một Transaction.
         * Trả về giá trị `true` nếu có ít nhất một dòng dữ liệu được tác động thành công, hỗ trợ kiểm soát kết quả đầu ra tại tầng Service.
         */
        public async Task<bool> SaveChangesAsync() =>
            (await _context.SaveChangesAsync()) > 0;
    }
}