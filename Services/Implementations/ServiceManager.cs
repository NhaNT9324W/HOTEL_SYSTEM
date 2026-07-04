using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class ServiceManager : IServiceManager
    {
        private readonly IServiceRepository _repo;

        public ServiceManager(IServiceRepository repo) => _repo = repo;

        public async Task<IEnumerable<ServiceDto>> GetAllAsync()
        {
            var services = await _repo.GetAllAsync();
            return services.Select(ToDto);
        }

        public async Task<ServiceDto?> GetByIdAsync(int id)
        {
            var service = await _repo.GetByIdAsync(id);
            return service == null ? null : ToDto(service);
        }

        public async Task<IEnumerable<ServiceDto>> SearchAsync(string keyword)
        {
            var services = await _repo.SearchAsync(keyword);
            return services.Select(ToDto);
        }

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

        public async Task UpdateAsync(UpdateServiceDto dto)
        {
            var service = await _repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Service not found");

            // Kiểm tra trùng tên (trừ chính nó)
            var existing = await _repo.SearchAsync(dto.ServiceName);
            if (existing.Any(s => s.ServiceName == dto.ServiceName && s.Id != dto.Id))
                throw new Exception("Service name already exists");

            service.ServiceName = dto.ServiceName;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Status = dto.Status;

            await _repo.UpdateAsync(service);
        }

        public async Task DeleteAsync(int id)
        {
            var service = await _repo.GetByIdAsync(id)
                ?? throw new Exception("Service not found");

            var usageCount = await _repo.CountUsageAsync(id);
            if (usageCount > 0)
                throw new Exception("Cannot delete service that is being used");

            await _repo.DeleteAsync(id);
        }

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