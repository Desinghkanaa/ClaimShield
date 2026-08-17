namespace ClaimShield.Api.Models.DTOs.Vehicles
{
    public class VehicleResponseDto
    {
        public Guid VehicleId { get; set; }

        public Guid CustomerId { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public string ChassisNumber { get; set; } = string.Empty;

        public string EngineNumber { get; set; } = string.Empty;

        public string? Variant { get; set; }

        public int ManufacturingYear { get; set; }

        public string? VehicleColor { get; set; }

        public string? RCNumber { get; set; }

        public bool IsActive { get; set; }

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        public int? FuelTypeId { get; set; }

        public string? RCStatus { get; set; }
    }
}