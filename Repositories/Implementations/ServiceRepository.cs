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
     * [V.1.7 ServiceRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể Service (Dịch vụ khách sạn).
     * Áp dụng mẫu kiến trúc Repository Pattern nhằm cô lập tầng Data Access Layer (DAL) độc lập hoàn toàn với Business Logic Layer (BLL).
     * Cung cấp các thao tác xử lý dữ liệu phục vụ trực tiếp cho phân hệ Quản lý danh mục dịch vụ (UC10), 
     * đồng thời hỗ trợ tra cứu thông tin giá niêm yết khi Tiếp tân ghi nhận tiêu dùng dịch vụ tại quầy (UC17).
     */
    public class ServiceRepository : IServiceRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor để quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public ServiceRepository(AppDbContext context) => _context = context;

        /** 
         * Lấy toàn bộ danh sách tất cả các dịch vụ hiện có trong cơ sở dữ liệu.
         * Phục vụ cho màn hình hiển thị tổng quan danh mục tiện ích của khách sạn tại giao diện quản trị (UC10.1).
         */
        public async Task<IEnumerable<Service>> GetAllAsync() =>
            await _context.Services.ToListAsync();

        /** 
         * Tìm kiếm thông tin chi tiết một dịch vụ dựa trên mã định danh số nguyên Primary Key (Id).
         * Hỗ trợ tải dữ liệu gốc lên biểu mẫu hiệu chỉnh của quy trình Cập nhật thông tin dịch vụ (UC10.2).
         */
        public async Task<Service?> GetByIdAsync(int id) =>
            await _context.Services.FindAsync(id);

        /** 
         * Tìm kiếm nâng cao và lọc danh sách dịch vụ theo từ khóa linh hoạt.
         * Thực hiện tìm kiếm tương đối (SQL LIKE) trên cả hai trường thông tin: Tên dịch vụ (ServiceName) và Mô tả chi tiết (Description).
         */
        public async Task<IEnumerable<Service>> SearchAsync(string keyword) =>
            await _context.Services
                .Where(s => s.ServiceName.Contains(keyword) ||
                            s.Description.Contains(keyword))
                .ToListAsync();

        /** 
         * Kiểm tra sự tồn tại của tên dịch vụ trên hệ thống.
         * Phục vụ bộ quy tắc nghiệp vụ Business Rule BR-35: Đảm bảo tên dịch vụ là duy nhất (Unique Service Name Constraint), 
         * không cho phép trùng lặp tiêu đề danh mục khi thêm mới.
         */
        public async Task<bool> IsNameExistsAsync(string serviceName) =>
            await _context.Services
                .AnyAsync(s => s.ServiceName == serviceName);

        /** 
         * Đếm tổng số lượt tiêu dùng phát sinh của dịch vụ này trong lịch sử vận hành khách sạn.
         * Kết nối trực tiếp với bảng dữ liệu 'ServiceUsages' để thống kê tần suất, làm cơ sở ràng buộc nghiệp vụ: 
         * Chặn hoàn toàn hành vi xóa vật lý nếu dịch vụ đã có dữ liệu tiêu dùng lịch sử nhằm bảo toàn tính toàn vẹn kế toán.
         */
        public async Task<int> CountUsageAsync(int serviceId) =>
            await _context.ServiceUsages.CountAsync(su => su.ServiceId == serviceId);

        /** 
         * Thực hiện thêm mới một bản ghi dịch vụ tiện ích vào cơ sở dữ liệu (UC10.3).
         * Lưu thay đổi tức thời xuống database thông qua SaveChangesAsync để hoàn tất giao dịch.
         */
        public async Task AddAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        /** 
         * Cập nhật thông tin thay đổi của dịch vụ (ví dụ: điều chỉnh giá niêm yết Price hoặc chuyển trạng thái Status).
         * Hệ thống tự động thiết lập thuộc tính hệ thống `UpdatedAt` theo chuẩn thời gian UTC nhằm phục vụ mục đích kiểm toán dữ liệu (Data Auditing).
         */
        public async Task UpdateAsync(Service service)
        {
            service.UpdatedAt = DateTime.UtcNow;
            _context.Services.Update(service);
            await _context.SaveChangesAsync();
        }

        /** 
         * Xóa vật lý bản ghi dịch vụ khỏi danh mục hệ thống.
         * Luồng xử lý tìm kiếm thực thể trước, nếu tồn tại sẽ tiến hành loại bỏ khỏi ngữ cảnh và đồng bộ xuống database.
         * Lưu ý: Tầng Service Layer cần gọi hàm `CountUsageAsync` để kiểm tra điều kiện an toàn trước khi kích hoạt phương thức này.
         */
        public async Task DeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
            }
        }
    }
}