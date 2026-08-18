using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        public int RoleId { get; set; }

        [Required]
        [MaxLength(200)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? LastName { get; set; }

        [Required]
        [MaxLength(300)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [MaxLength(1000)]
        public string? ProfileImage { get; set; }

        public bool IsActive { get; set; }

        // Navigation property (we'll use this later)
        // public Role? Role { get; set; }
    }
}