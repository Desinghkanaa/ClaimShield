using ClaimShield.Api.Models.DTOs.Policies;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IPolicyService
    {
        Task<IEnumerable<PolicyResponseDto>> GetAllPoliciesAsync();

        Task<PolicyResponseDto?> GetPolicyByIdAsync(Guid policyId);

        Task<IEnumerable<PolicyResponseDto>> GetPoliciesByCustomerAsync(Guid customerId);

        Task<PolicyResponseDto> CreatePolicyAsync(CreatePolicyRequest request);

        Task<bool> UpdatePolicyAsync(UpdatePolicyRequest request);

        Task<bool> DeletePolicyAsync(Guid policyId);
    }
}