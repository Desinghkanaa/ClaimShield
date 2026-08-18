using System.Security.Claims;

using ClaimShield.Api.Constants;
using ClaimShield.Api.Data.Context;

using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace ClaimShield.Api.Authentication
{
    // =============================================================
    // Supabase's JWT carries a "role" claim, but it's the Postgres
    // role ("authenticated"), not this app's Customer/Surveyor/
    // Approver/Admin role. This runs on every authenticated
    // request, looks the real app role up from public.profiles by
    // the token's "sub", and replaces whatever role claim(s) the
    // token already had with it - so every existing
    // [Authorize(Roles = "...")] attribute and the ownership checks
    // added in Phase 0 keep working unchanged.
    // =============================================================

    public class SupabaseRoleClaimsTransformation : IClaimsTransformation
    {
        private const string ResolvedMarkerClaimType =
            "claimshield_role_resolved";

        private readonly IServiceScopeFactory _scopeFactory;

        public SupabaseRoleClaimsTransformation(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<ClaimsPrincipal> TransformAsync(
            ClaimsPrincipal principal)
        {
            if (principal.Identity is not { IsAuthenticated: true } ||
                principal.Identity is not ClaimsIdentity identity)
            {
                return principal;
            }

            // -------------------------------------------------
            // Already resolved on this principal - avoid doing
            // the DB lookup more than once per request.
            // -------------------------------------------------

            if (identity.HasClaim(
                    c => c.Type == ResolvedMarkerClaimType))
            {
                return principal;
            }

            var subClaim =
                principal.FindFirst(ClaimTypes.NameIdentifier) ??
                principal.FindFirst("sub");

            if (subClaim == null ||
                !Guid.TryParse(subClaim.Value, out var userId))
            {
                return principal;
            }

            using var scope =
                _scopeFactory.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<ClaimShieldDbContext>();

            var roleId =
                await dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == userId)
                    .Select(u => (int?)u.RoleId)
                    .FirstOrDefaultAsync();

            var roleName =
                roleId switch
                {
                    RoleConstants.CustomerId => RoleConstants.Customer,
                    RoleConstants.RepairerId => RoleConstants.Repairer,
                    RoleConstants.SurveyorId => RoleConstants.Surveyor,
                    RoleConstants.ApproverId => RoleConstants.Approver,
                    RoleConstants.AdminId => RoleConstants.Admin,
                    _ => null
                };

            // -------------------------------------------------
            // Strip Supabase's own role claim(s) so nothing else
            // in the pipeline can accidentally match against
            // "authenticated" instead of the real app role.
            // -------------------------------------------------

            foreach (var existingRoleClaim in
                     identity.FindAll(ClaimTypes.Role).ToList())
            {
                identity.RemoveClaim(existingRoleClaim);
            }

            foreach (var existingRoleClaim in
                     identity.FindAll("role").ToList())
            {
                identity.RemoveClaim(existingRoleClaim);
            }

            if (roleName != null)
            {
                identity.AddClaim(
                    new Claim(
                        ClaimTypes.Role,
                        roleName));
            }

            identity.AddClaim(
                new Claim(
                    ResolvedMarkerClaimType,
                    "true"));

            return principal;
        }
    }
}
