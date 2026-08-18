using System.Security.Claims;

namespace ClaimShield.Api.Authentication
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }

        string? RoleName { get; }
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId =>
            GetUserId(
                _httpContextAccessor.HttpContext?.User);

        public string? RoleName =>
            GetRoleName(
                _httpContextAccessor.HttpContext?.User);

        // =========================================================
        // USER ID
        // =========================================================

        private static Guid? GetUserId(
            ClaimsPrincipal? user)
        {
            if (user == null ||
                user.Identity == null ||
                !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var value =
                user.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(value))
            {
                value =
                    user.FindFirst(
                        "sub")?.Value;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value =
                    user.FindFirst(
                        "userId")?.Value;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value =
                    user.FindFirst(
                        "UserId")?.Value;
            }

            if (Guid.TryParse(
                    value,
                    out var userId))
            {
                return userId;
            }

            return null;
        }

        // =========================================================
        // ROLE NAME
        // =========================================================

        private static string? GetRoleName(
            ClaimsPrincipal? user)
        {
            if (user == null ||
                user.Identity == null ||
                !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var role =
                user.FindFirst(
                    ClaimTypes.Role)?.Value;

            if (!string.IsNullOrWhiteSpace(role))
            {
                return role;
            }

            role =
                user.FindFirst(
                    "role")?.Value;

            if (!string.IsNullOrWhiteSpace(role))
            {
                return role;
            }

            role =
                user.FindFirst(
                    "Role")?.Value;

            return role;
        }
    }
}
