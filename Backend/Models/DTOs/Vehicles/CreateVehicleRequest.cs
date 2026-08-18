using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Vehicles
{
    public class CreateVehicleRequest
    {
        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required]
        public string ChassisNumber { get; set; } = string.Empty;

        [Required]
        public string EngineNumber { get; set; } = string.Empty;

        public string? Variant { get; set; }

        public int ManufacturingYear { get; set; }

        public string? VehicleColor { get; set; }

        public string? RCNumber { get; set; }

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        public int? FuelTypeId { get; set; }
    }
}