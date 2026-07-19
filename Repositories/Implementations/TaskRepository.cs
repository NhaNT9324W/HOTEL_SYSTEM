using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.8 TaskRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể HousekeepingTask (Tác vụ buồng phòng/bảo trì).
     * Áp dụng mẫu kiến trúc Repository Pattern nhằm cô lập tầng Data Access Layer (DAL) độc lập hoàn toàn với Business Logic Layer (BLL).
     * Cung cấp các phương thức truy vấn và cập nhật dữ liệu cốt lõi phục vụ trực tiếp cho phân hệ Quản lý tác vụ buồng phòng (UC19),
     * quy trình cập nhật trạng thái phòng vật lý (UC19.2), xử lý sự cố (UC19.4) và Nghiệm thu chất lượng phòng (UC20).
     */
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public TaskRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các tác vụ buồng phòng và bảo trì trong hệ thống.
         * Sử dụng kỹ thuật Eager Loading (`Include`) để nạp kèm thông tin Phòng vật lý (`Room`), Nhân viên được phân gán (`AssignedTo`),
         * và Người khởi tạo tác vụ (`CreatedBy`), giúp tối ưu hóa hiệu năng, tránh triệt để lỗi truy vấn N+1.
         * Kết quả đầu ra được sắp xếp giảm dần theo thời gian tạo (`CreatedAt`) để hiển thị các tác vụ mới nhất lên đầu danh sách quản trị.
         */
        public async Task<IEnumerable<HousekeepingTask>> GetAllAsync() =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        /** 
         * Tìm kiếm một tác vụ cụ thể dựa trên mã định danh số nguyên Primary Key (Id).
         * Đi kèm đầy đủ các thông tin điều hướng liên kết liên quan (Room, AssignedTo, CreatedBy) để phục vụ cho các logic kiểm tra quyền,
         * xử lý chuyển đổi trạng thái tiến độ hoặc hiển thị chi tiết công việc trên ứng dụng di động của nhân viên buồng phòng.
         */
        public async Task<HousekeepingTask?> GetByIdAsync(int id) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .FirstOrDefaultAsync(t => t.Id == id);

        /** 
         * Tìm kiếm nâng cao và lọc danh sách tác vụ buồng phòng theo từ khóa linh hoạt.
         * Hệ thống sử dụng biểu thức Lambda để biên dịch thành câu lệnh SQL LIKE, hỗ trợ tìm kiếm tương đối trên 3 tiêu chí:
         * Số phòng vật lý (`Room.RoomNumber`), Tên nhân viên xử lý (`AssignedTo.FullName`) hoặc Nội dung mô tả công việc (`Description`).
         */
        public async Task<IEnumerable<HousekeepingTask>> SearchAsync(string keyword) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.AssignedTo)
                .Include(t => t.CreatedBy)
                .Where(t => t.Room.RoomNumber.Contains(keyword) ||
                            t.AssignedTo.FullName.Contains(keyword) ||
                            t.Description.Contains(keyword))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        /** 
         * Lấy danh sách toàn bộ các tác vụ được phân gán riêng cho một nhân viên buồng phòng cụ thể dựa trên ID nhân sự.
         * Phục vụ đắc lực cho phân hệ giao diện thực địa của Room Staff (UC19.1 – View Assigned Tasks), giúp nhân viên theo dõi danh sách
         * nhiệm vụ cần làm của riêng mình. Chỉ nạp kèm thông tin `Room` để biết vị trí làm việc và `CreatedBy` để biết người chỉ đạo, 
         * tối ưu dung lượng gói tin truyền tải.
         */
        public async Task<IEnumerable<HousekeepingTask>> GetByStaffIdAsync(int staffId) =>
            await _context.HousekeepingTasks
                .Include(t => t.Room)
                .Include(t => t.CreatedBy)
                .Where(t => t.AssignedToId == staffId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        /** 
         * Thực hiện thêm mới một tác vụ buồng phòng/bảo trì vào cơ sở dữ liệu.
         * Thao tác được kích hoạt khi Quản lý phân công dọn phòng (UC19), khi hệ thống tự động sinh tác vụ dọn dẹp sau khi khách Check-out (UC18),
         * hoặc khi Lễ tân/Nhân viên báo cáo sự cố phòng hỏng (UC19.4). Xác nhận và lưu giao dịch tức thời thông qua SaveChangesAsync.
         */
        public async Task AddAsync(HousekeepingTask task)
        {
            await _context.HousekeepingTasks.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        /** 
         * Cập nhật thông tin thay đổi của tác vụ (ví dụ: Thay đổi nhân viên xử lý, điều chỉnh mức độ ưu tiên hoặc cập nhật tiến độ công việc).
         * Trước khi đồng bộ xuống DB vật lý, hệ thống tự động ghi nhận mốc thời gian hệ thống `UpdatedAt` theo chuẩn UTC,
         * đóng vai trò là cơ sở dữ liệu kiểm toán lịch sử và tính toán thời gian hoàn thành KPIs của nhân sự.
         */
        public async Task UpdateAsync(HousekeepingTask task)
        {
            task.UpdatedAt = DateTime.UtcNow;
            _context.HousekeepingTasks.Update(task);
            await _context.SaveChangesAsync();
        }

        /** 
         * Xóa vật lý bản ghi tác vụ khỏi hệ thống cơ sở dữ liệu dựa trên ID.
         * Thường chỉ áp dụng trong trường hợp Quản lý khởi tạo sai thông tin phôi tác vụ ở trạng thái 'Pending' và muốn dọn dẹp dữ liệu rác.
         * Hàm thực hiện tìm kiếm thực thể trước, nếu tồn tại hợp lệ mới tiến hành lệnh Remove và cam kết thay đổi xuống CSDL.
         */
        public async Task DeleteAsync(int id)
        {
            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task != null)
            {
                _context.HousekeepingTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}