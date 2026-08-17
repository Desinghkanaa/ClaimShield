using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Users
{
    public class UpdateUserRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}