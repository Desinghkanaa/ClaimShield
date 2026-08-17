using ClaimShield.Api.Models.DTOs.Users;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();

        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);

        Task<UserResponseDto> CreateUserAsync(CreateUserRequest request);

        Task<bool> UpdateUserAsync(UpdateUserRequest request);

        Task<bool> DeleteUserAsync(Guid userId);
    }
}