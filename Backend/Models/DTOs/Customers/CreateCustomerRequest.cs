using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Customers
{
    public class CreateCustomerRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string CustomerCode { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? AadhaarNumber { get; set; }

        public string? DrivingLicenseNumber { get; set; }

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Pincode { get; set; }
    }
}