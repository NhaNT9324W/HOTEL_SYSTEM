using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface IServiceManager
    {
        Task<IEnumerable<ServiceDto>> GetAllAsync();
        Task<ServiceDto?> GetByIdAsync(int id);
        Task<IEnumerable<ServiceDto>> SearchAsync(string keyword);
        Task CreateAsync(CreateServiceDto dto);
        Task UpdateAsync(UpdateServiceDto dto);
        Task DeleteAsync(int id);
    }
}