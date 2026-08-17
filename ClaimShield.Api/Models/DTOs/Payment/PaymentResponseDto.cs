namespace ClaimShield.Api.Models.DTOs.Payments
{
    public class PaymentResponseDto
    {
        public Guid PaymentId { get; set; }

        public Guid ClaimId { get; set; }

        public decimal Amount { get; set; }

        public int PaymentStatusId { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public string? TransactionReference { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Remarks { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}