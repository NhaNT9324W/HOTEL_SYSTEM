using Hotel_System.DTOs;

namespace Hotel_System.Services.Interfaces
{
    public interface ICheckOutService
    {
        Task<IEnumerable<ServiceUsageDto>> GetServiceUsagesAsync(int reservationId);
        Task AddServiceUsageAsync(AddServiceUsageDto dto);
        Task RemoveServiceUsageAsync(int serviceUsageId);
        Task<InvoiceDto> PreviewInvoiceAsync(int reservationId);
        Task<InvoiceDto> CheckOutAsync(int reservationId);
    }
}