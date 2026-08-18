using ClaimShield.Api.Models.DTOs.Dashboard;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}
