using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    // =================================================================
    // Read-only aggregation over data every earlier phase already
    // produces (Claims, ClaimScoringResults, Payments, RepairEstimates,
    // Customers). No new data collection, no business logic - purely a
    // summary view for the Admin dashboard.
    // =================================================================

    public class DashboardService : IDashboardService
    {
        private readonly ClaimShieldDbContext _context;

        public DashboardService(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var summary = new DashboardSummaryDto
            {
                TotalClaims = await _context.Claims.CountAsync(),

                TotalCustomers = await _context.Customers.CountAsync(),

                TotalPaidAmount =
                    await _context.Payments
                        .Where(x => x.PaymentStatusId == PaymentStatusConstants.Paid)
                        .SumAsync(x => (decimal?)x.Amount) ?? 0m,

                ClaimsByStatus = await GetClaimsByStatusAsync(),

                ClaimsOverTime = await GetClaimsOverTimeAsync(),

                RiskBandDistribution = await GetRiskBandDistributionAsync(),

                PaymentsByStatus = await GetPaymentsByStatusAsync(),

                RepairEstimateOutcomes = await GetRepairEstimateOutcomesAsync(),

                AverageApprovalTurnaroundDays =
                    await GetAverageApprovalTurnaroundDaysAsync()
            };

            var lossAmounts =
                await _context.Claims
                    .Where(x => x.EstimatedLossAmount != null)
                    .Select(x => x.EstimatedLossAmount!.Value)
                    .ToListAsync();

            summary.AverageClaimAmount =
                lossAmounts.Count > 0
                    ? lossAmounts.Average()
                    : 0m;

            return summary;
        }

        private async Task<List<StatusCountDto>> GetClaimsByStatusAsync()
        {
            var grouped =
                await _context.Claims
                    .GroupBy(x => x.StatusId)
                    .Select(g => new { StatusId = g.Key, Count = g.Count() })
                    .ToListAsync();

            return grouped
                .Select(
                    x => new StatusCountDto
                    {
                        StatusId = x.StatusId ?? 0,
                        StatusName = GetClaimStatusName(x.StatusId ?? 0),
                        Count = x.Count
                    })
                .OrderBy(x => x.StatusId)
                .ToList();
        }

        private async Task<List<DailyCountDto>> GetClaimsOverTimeAsync()
        {
            var since = DateTime.UtcNow.Date.AddDays(-29);

            var claims =
                await _context.Claims
                    .Where(x => x.CreatedDate != null && x.CreatedDate >= since)
                    .Select(x => x.CreatedDate!.Value.Date)
                    .ToListAsync();

            var byDay =
                claims
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<DailyCountDto>();

            for (var day = since; day <= DateTime.UtcNow.Date; day = day.AddDays(1))
            {
                result.Add(
                    new DailyCountDto
                    {
                        Date = day.ToString("yyyy-MM-dd"),
                        Count = byDay.TryGetValue(day, out var count) ? count : 0
                    });
            }

            return result;
        }

        // -----------------------------------------------------------
        // Latest non-superseded scoring result per claim, grouped by
        // Band. In-memory grouping (data volumes here are POC-scale) -
        // same style already used in
        // ClaimDecisionService.GetMyQueueAsync. This is a
        // simplification versus ClaimScoringService's full composite
        // recomputation (which sums both stages and re-bands) - fine
        // for a high-level dashboard, not claiming per-claim precision.
        // -----------------------------------------------------------

        private async Task<List<BandCountDto>> GetRiskBandDistributionAsync()
        {
            var activeResults =
                await _context.ClaimScoringResults
                    .Where(x => x.SupersededBy == null)
                    .Select(x => new { x.ClaimId, x.Band, x.ScoredAt })
                    .ToListAsync();

            var latestPerClaim =
                activeResults
                    .GroupBy(x => x.ClaimId)
                    .Select(g => g.OrderByDescending(x => x.ScoredAt).First())
                    .ToList();

            return latestPerClaim
                .GroupBy(x => x.Band)
                .Select(
                    g => new BandCountDto
                    {
                        BandName = GetBandName(g.Key),
                        Count = g.Count()
                    })
                .OrderBy(x => x.BandName)
                .ToList();
        }

        private async Task<List<StatusCountDto>> GetPaymentsByStatusAsync()
        {
            var grouped =
                await _context.Payments
                    .GroupBy(x => x.PaymentStatusId)
                    .Select(g => new { PaymentStatusId = g.Key, Count = g.Count() })
                    .ToListAsync();

            return grouped
                .Select(
                    x => new StatusCountDto
                    {
                        StatusId = x.PaymentStatusId,
                        StatusName = GetPaymentStatusName(x.PaymentStatusId),
                        Count = x.Count
                    })
                .OrderBy(x => x.StatusId)
                .ToList();
        }

        private async Task<List<StatusCountDto>> GetRepairEstimateOutcomesAsync()
        {
            var grouped =
                await _context.RepairEstimates
                    .GroupBy(x => x.ApprovalStatusId)
                    .Select(g => new { ApprovalStatusId = g.Key, Count = g.Count() })
                    .ToListAsync();

            return grouped
                .Select(
                    x => new StatusCountDto
                    {
                        StatusId = x.ApprovalStatusId ?? 0,
                        StatusName = GetApprovalStatusName(x.ApprovalStatusId),
                        Count = x.Count
                    })
                .OrderBy(x => x.StatusId)
                .ToList();
        }

        private async Task<double?> GetAverageApprovalTurnaroundDaysAsync()
        {
            var pairs =
                await _context.Claims
                    .Where(
                        x =>
                            x.StatusId != null &&
                            x.StatusId >= ClaimStatusConstants.Approved &&
                            x.CreatedDate != null &&
                            x.UpdatedDate != null)
                    .Select(x => new { x.CreatedDate, x.UpdatedDate })
                    .ToListAsync();

            if (pairs.Count == 0)
            {
                return null;
            }

            return pairs
                .Select(x => (x.UpdatedDate!.Value - x.CreatedDate!.Value).TotalDays)
                .Average();
        }

        private static string GetClaimStatusName(
            int statusId)
        {
            return statusId switch
            {
                ClaimStatusConstants.Submitted => "Submitted",
                ClaimStatusConstants.UnderReview => "Under Review",
                ClaimStatusConstants.SurveyAssigned => "Survey Assigned",
                ClaimStatusConstants.SurveyCompleted => "Survey Completed",
                ClaimStatusConstants.RepairAssigned => "Repair Assigned",
                ClaimStatusConstants.RepairInProgress => "Repair In Progress",
                ClaimStatusConstants.Approved => "Approved",
                ClaimStatusConstants.Rejected => "Rejected",
                ClaimStatusConstants.Settled => "Settled",
                ClaimStatusConstants.Closed => "Closed",
                _ => "Unknown"
            };
        }

        private static string GetPaymentStatusName(
            int paymentStatusId)
        {
            return paymentStatusId switch
            {
                PaymentStatusConstants.Pending => "Pending",
                PaymentStatusConstants.Processing => "Processing",
                PaymentStatusConstants.Paid => "Paid",
                PaymentStatusConstants.Failed => "Failed",
                PaymentStatusConstants.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        private static string GetApprovalStatusName(
            int? approvalStatusId)
        {
            return approvalStatusId switch
            {
                null => "Pending",
                RepairEstimateApprovalStatusConstants.Approved => "Approved",
                RepairEstimateApprovalStatusConstants.Rejected => "Rejected",
                _ => "Unknown"
            };
        }

        private static string GetBandName(
            int band)
        {
            return band switch
            {
                ScoringBandConstants.Green => "Green",
                ScoringBandConstants.Amber => "Amber",
                ScoringBandConstants.Red => "Red",
                _ => "Unknown"
            };
        }
    }
}
