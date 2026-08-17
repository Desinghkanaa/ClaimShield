using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.AuthorityLimits;
using ClaimShield.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Services
{
    public class AuthorityLimitService : IAuthorityLimitService
    {
        private readonly ClaimShieldDbContext _context;

        public AuthorityLimitService(
            ClaimShieldDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuthorityLimitResponseDto>> GetAllAsync()
        {
            var limits =
                await _context.AuthorityLimits
                    .OrderBy(x => x.RoleId)
                    .ToListAsync();

            return limits.Select(MapToDto);
        }

        public async Task<AuthorityLimitResponseDto?> GetByRoleAsync(
            int roleId)
        {
            var limit =
                await _context.AuthorityLimits
                    .FirstOrDefaultAsync(
                        x => x.RoleId == roleId);

            return limit == null
                ? null
                : MapToDto(limit);
        }

        public async Task<AuthorityLimitResponseDto> UpsertAsync(
            int roleId,
            Guid updatedBy,
            UpdateAuthorityLimitRequest request)
        {
            var limit =
                await _context.AuthorityLimits
                    .FirstOrDefaultAsync(
                        x => x.RoleId == roleId);

            if (limit == null)
            {
                limit = new AuthorityLimit
                {
                    RoleId = roleId
                };

                _context.AuthorityLimits.Add(limit);
            }

            limit.MaxApprovalAmount = request.MaxApprovalAmount;

            limit.MaxRiskScore = request.MaxRiskScore;

            limit.UpdatedDate = DateTime.UtcNow;

            limit.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();

            return MapToDto(limit);
        }

        private static AuthorityLimitResponseDto MapToDto(
            AuthorityLimit limit)
        {
            return new AuthorityLimitResponseDto
            {
                RoleId = limit.RoleId,

                RoleName = GetRoleName(limit.RoleId),

                MaxApprovalAmount = limit.MaxApprovalAmount,

                MaxRiskScore = limit.MaxRiskScore,

                UpdatedDate = limit.UpdatedDate,

                UpdatedBy = limit.UpdatedBy
            };
        }

        private static string GetRoleName(
            int roleId)
        {
            return roleId switch
            {
                RoleConstants.CustomerId => RoleConstants.Customer,
                RoleConstants.RepairerId => RoleConstants.Repairer,
                RoleConstants.SurveyorId => RoleConstants.Surveyor,
                RoleConstants.ApproverId => RoleConstants.Approver,
                RoleConstants.AdminId => RoleConstants.Admin,
                _ => "Unknown"
            };
        }
    }
}
