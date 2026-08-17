using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClaimShield.Api.Models.Entities
{
    [Table("Payments", Schema = "dbo")]
    public class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }

        public Guid ClaimId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int PaymentStatusId { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}