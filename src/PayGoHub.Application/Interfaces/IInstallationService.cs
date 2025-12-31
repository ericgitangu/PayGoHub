using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface IInstallationService
{
    Task<IEnumerable<InstallationDto>> GetAllAsync();
    Task<InstallationDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<InstallationDto>> GetPendingAsync();
    Task<int> GetInstallationsThisMonthAsync();
    Task<decimal> GetInstallationGrowthAsync();
}
