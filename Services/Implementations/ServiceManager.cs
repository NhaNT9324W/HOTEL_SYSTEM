using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.12 ServiceManager Implementation]
     * Lớp xử lý nghiệp vụ quản lý danh mục dịch vụ tiện ích của khách sạn.
     * Phục vụ phân hệ Quản lý danh mục dịch vụ (UC10) và tra cứu khi ghi nhận tiêu dùng tại quầy (UC17).
     */
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceRepository _repo;

        /** Inject Repository quản lý thông tin dịch vụ thông qua Constructor. */
        public ServiceManager(IServiceRepository repo) => _repo = repo;

        /** Lấy toàn bộ danh sách tất cả dịch vụ dưới dạng DTO để hiển thị lên bảng danh mục quản trị (UC10.1). */
        public async Task<IEnumerable<ServiceDto>> GetAllAsync()
        {
            var services = await _repo.GetAllAsync();
            return services.Select(ToDto);
        }

        /** Tìm chi tiết dịch vụ theo ID phục vụ xem thông tin hoặc chuẩn bị hiệu chỉnh cấu hình (UC10.2). */
        public async Task<ServiceDto?> GetByIdAsync(int id)
        {
            var service = await _repo.GetByIdAsync(id);
            return service == null ? null : ToDto(service);
        }

        /** Tra cứu nhanh danh sách dịch vụ theo từ khóa linh hoạt tại bộ lọc giao diện. */
        public async Task<IEnumerable<ServiceDto>> SearchAsync(string keyword)
        {
            var services = await _repo.SearchAsync(keyword);
            return services.Select(ToDto);
        }

        /**
         * Khởi tạo cấu hình dịch vụ tiện ích mới (UC10.3).
         * Ràng buộc nghiệp vụ: Kiểm tra trùng tên dịch vụ trên hệ thống nhằm ngăn chặn dữ liệu rác.
         */
        public async Task CreateAsync(CreateServiceDto dto)
        {
            if (await _repo.IsNameExistsAsync(dto.ServiceName))
                throw new Exception("Service name already exists");

            var service = new Service
            {
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                Price = dto.Price,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(service);
        }

        /** 
         * Hiệu chỉnh thông tin dịch vụ (tên, mô tả, giá niêm yết, trạng thái).
         * Thực hiện kiểm tra đối chiếu trùng tên ngoại trừ mã định danh của chính dịch vụ hiện tại.
         */
        public async Task UpdateAsync(UpdateServiceDto dto)
        {
            var service = await _repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Service not found");

            // Kiểm tra trùng tên với các bản ghi khác trong hệ thống
            var existing = await _repo.SearchAsync(dto.ServiceName);
            if (existing.Any(s => s.ServiceName == dto.ServiceName && s.Id != dto.Id))
                throw new Exception("Service name already exists");

            service.ServiceName = dto.ServiceName;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Status = dto.Status;

            await _repo.UpdateAsync(service);
        }

        /** 
         * Xóa vật lý bản ghi dịch vụ ra khỏi hệ thống danh mục.
         * Ràng buộc nghiệp vụ: Chặn xóa nếu dịch vụ đã phát sinh lịch sử tiêu dùng (Folio) để bảo toàn dữ liệu kế toán.
         */
        public async Task DeleteAsync(int id)
        {
            var service = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Service not found");

            var usageCount = await _repo.CountUsageAsync(id);
            if (usageCount > 0)
                throw new Exception("Cannot delete service that is being used");

            await _repo.DeleteAsync(id);
        }

        // ===== PRIVATE HELPERS =====
        /** Hàm tiện ích ánh xạ (Mapping) dữ liệu phẳng từ thực thể Service sang cấu trúc ServiceDto an toàn. */
        private static ServiceDto ToDto(Service s) => new()
        {
            Id = s.Id,
            ServiceName = s.ServiceName,
            Description = s.Description,
            Price = s.Price,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        };
    }
}