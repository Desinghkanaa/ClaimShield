using System.ComponentModel.DataAnnotations;

namespace ClaimShield.Api.Models.DTOs.RepairEstimates
{
    public class RejectRepairEstimateRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Remarks { get; set; } = string.Empty;
    }
}
