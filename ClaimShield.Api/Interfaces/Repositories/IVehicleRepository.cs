using ClaimShield.Api.Models.Entities;

namespace ClaimShield.Api.Interfaces.Repositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();

        Task<Vehicle?> GetByIdAsync(Guid vehicleId);

        Task<IEnumerable<Vehicle>> GetByCustomerIdAsync(Guid customerId);

        Task AddAsync(Vehicle vehicle);

        Task UpdateAsync(Vehicle vehicle);

        Task DeleteAsync(Guid vehicleId);
    }
}