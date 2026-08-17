using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Users
{
    public class CreateUserRequest
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }
}