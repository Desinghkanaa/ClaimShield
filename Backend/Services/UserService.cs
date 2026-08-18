using ClaimShield.Api.Interfaces.Repositories;
using ClaimShield.Api.Interfaces.Services;
using ClaimShield.Api.Models.DTOs.Users;

namespace ClaimShield.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISupabaseAdminService _supabaseAdminService;

        public UserService(
            IUserRepository userRepository,
            ISupabaseAdminService supabaseAdminService)
        {
            _userRepository = userRepository;
            _supabaseAdminService = supabaseAdminService;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();

            return users.Select(user => new UserResponseDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            });
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            return new UserResponseDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserRequest request)
        {
            // -----------------------------------------------------
            // Creates the auth.users row via Supabase's Admin API.
            // The on_auth_user_created trigger creates the matching
            // public.profiles row (with RoleId/FirstName/etc. from
            // the user_metadata passed here) synchronously, in the
            // same transaction, so it already exists by the time
            // this call returns.
            // -----------------------------------------------------

            var userId =
                await _supabaseAdminService.CreateUserAsync(
                    request.Email,
                    request.Password,
                    request.RoleId,
                    request.FirstName,
                    request.LastName,
                    request.PhoneNumber);

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "User was created in Supabase Auth but the " +
                    "profiles row was not found afterward.");
            }

            return new UserResponseDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                return false;

            // -----------------------------------------------------
            // Email lives in Supabase Auth (auth.users), not just
            // our profiles mirror - only Supabase's Admin API can
            // change it, so only call it when it actually changed.
            // -----------------------------------------------------

            if (!string.Equals(
                    user.Email,
                    request.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _supabaseAdminService.UpdateUserEmailAsync(
                    user.UserId,
                    request.Email);
            }

            user.RoleId = request.RoleId;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.IsActive = request.IsActive;

            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            // -----------------------------------------------------
            // Deletes the auth.users row via Supabase's Admin API;
            // the ON DELETE CASCADE foreign key from profiles.id to
            // auth.users.id removes the profiles row automatically.
            // -----------------------------------------------------

            await _supabaseAdminService.DeleteUserAsync(userId);

            return true;
        }
    }
}