using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
