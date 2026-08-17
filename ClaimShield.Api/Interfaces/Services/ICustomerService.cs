using ClaimShield.Api.Models.DTOs.Customers;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync();

        Task<CustomerResponseDto?> GetCustomerByIdAsync(Guid customerId);

        Task<CustomerResponseDto?> GetCustomerByUserIdAsync(Guid userId);

        Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerRequest request);

        Task<bool> UpdateCustomerAsync(UpdateCustomerRequest request);

        Task<bool> DeleteCustomerAsync(Guid customerId);
    }
}