using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface IDeviceService
{
    Task<IEnumerable<DeviceDto>> GetAllAsync();
    Task<DeviceDto?> GetByIdAsync(Guid id);
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
}
