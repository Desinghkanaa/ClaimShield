using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Customers")]
    public class Customer
    {
        [Key]
        public Guid CustomerId { get; set; }

        public Guid UserId { get; set; }

        [MaxLength(20)]
        public string CustomerCode { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }

        [MaxLength(12)]
        public string? AadhaarNumber { get; set; }

        [MaxLength(25)]
        public string? DrivingLicenseNumber { get; set; }

        [MaxLength(400)]
        public string? AddressLine1 { get; set; }

        [MaxLength(400)]
        public string? AddressLine2 { get; set; }

        [MaxLength(200)]
        public string? City { get; set; }

        [MaxLength(200)]
        public string? State { get; set; }

        [MaxLength(10)]
        public string? Pincode { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        // Navigation Property
        public User? User { get; set; }
    }
}