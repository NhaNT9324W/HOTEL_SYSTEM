using Hotel_System.Entities.Enums;

namespace Hotel_System.Entities
{
    public class Service : BaseEntity
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; } = ServiceStatus.Active;
    }
}