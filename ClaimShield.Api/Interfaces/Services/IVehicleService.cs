using ClaimShield.Api.Models.DTOs.Vehicles;

namespace ClaimShield.Api.Interfaces.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponseDto>> GetAllVehiclesAsync();

        Task<VehicleResponseDto?> GetVehicleByIdAsync(Guid vehicleId);

        Task<IEnumerable<VehicleResponseDto>> GetVehiclesByCustomerAsync(Guid customerId);

        Task<VehicleResponseDto> CreateVehicleAsync(CreateVehicleRequest request);

        Task<bool> UpdateVehicleAsync(UpdateVehicleRequest request);

        Task<bool> DeleteVehicleAsync(Guid vehicleId);
    }
}