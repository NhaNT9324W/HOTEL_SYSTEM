using Hotel_System.Entities.Enums;
using System.ComponentModel.DataAnnotations;

namespace Hotel_System.DTOs
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public ServiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Service name is required")]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
    }

    public class UpdateServiceDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        public ServiceStatus Status { get; set; }
    }
}