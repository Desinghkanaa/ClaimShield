using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.RepairEstimates
{
    public class ApproveRepairEstimateRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ApprovedAmount { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}
