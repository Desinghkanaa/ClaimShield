using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        public Guid VehicleId { get; set; }

        public Guid CustomerId { get; set; }

        [MaxLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ChassisNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string EngineNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Variant { get; set; }

        public int ManufacturingYear { get; set; }

        [MaxLength(100)]
        public string? VehicleColor { get; set; }

        [MaxLength(30)]
        public string? RCNumber { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? MakeId { get; set; }

        public int? ModelId { get; set; }

        public int? FuelTypeId { get; set; }

        [MaxLength(50)]
        public string? RCStatus { get; set; }

        // Navigation Property
        public Customer? Customer { get; set; }
    }
}