namespace ClaimShield.Api.Models.DTOs.Users
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }

        public int RoleId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }
    }
}