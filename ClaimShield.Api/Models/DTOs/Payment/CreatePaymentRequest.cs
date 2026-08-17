using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.Payments
{
    public class CreatePaymentRequest
    {
        [Required]
        public Guid ClaimId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}