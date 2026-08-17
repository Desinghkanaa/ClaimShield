using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using ClaimShield.Api.Interfaces.Services;

namespace ClaimShield.Api.Services
{
    // =============================================================
    // Wraps Supabase Auth's Admin API (service_role key required).
    // Used by UserService for Admin-driven user provisioning -
    // account creation/updates/deletion now go through Supabase
    // Auth instead of writing a password hash directly, since
    // Supabase owns auth.users.
    // =============================================================

    public class SupabaseAdminService : ISupabaseAdminService
    {
        private readonly HttpClient _httpClient;

        public SupabaseAdminService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            var supabaseUrl =
                configuration["Supabase:Url"];

            var serviceRoleKey =
                configuration["Supabase:ServiceRoleKey"];

            if (string.IsNullOrWhiteSpace(supabaseUrl))
            {
                throw new InvalidOperationException(
                    "Supabase:Url is not configured.");
            }

            if (string.IsNullOrWhiteSpace(serviceRoleKey))
            {
                throw new InvalidOperationException(
                    "Supabase:ServiceRoleKey is not configured.");
            }

            httpClient.BaseAddress =
                new Uri($"{supabaseUrl}/auth/v1/");

            httpClient.DefaultRequestHeaders.Add(
                "apikey",
                serviceRoleKey);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    serviceRoleKey);

            _httpClient = httpClient;
        }

        public async Task<Guid> CreateUserAsync(
            string email,
            string password,
            int roleId,
            string firstName,
            string? lastName,
            string? phoneNumber)
        {
            var payload = new
            {
                email,
                password,
                email_confirm = true,
                user_metadata = new
                {
                    role_id = roleId,
                    first_name = firstName,
                    last_name = lastName,
                    phone_number = phoneNumber
                }
            };

            var response =
                await _httpClient.PostAsJsonAsync(
                    "admin/users",
                    payload);

            await EnsureSuccessAsync(
                response,
                "create user");

            var body =
                await response.Content
                    .ReadFromJsonAsync<JsonElement>();

            return Guid.Parse(
                body.GetProperty("id").GetString()!);
        }

        public async Task UpdateUserEmailAsync(
            Guid userId,
            string newEmail)
        {
            var payload = new
            {
                email = newEmail
            };

            var response =
                await _httpClient.PutAsJsonAsync(
                    $"admin/users/{userId}",
                    payload);

            await EnsureSuccessAsync(
                response,
                "update user email");
        }

        public async Task DeleteUserAsync(
            Guid userId)
        {
            var response =
                await _httpClient.DeleteAsync(
                    $"admin/users/{userId}");

            await EnsureSuccessAsync(
                response,
                "delete user");
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string action)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var errorBody =
                await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Supabase Admin API failed to {action} " +
                $"({(int)response.StatusCode}): {errorBody}");
        }
    }
}
