using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllAsync();
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PaymentDto>> GetRecentPaymentsAsync(int count);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetRevenueGrowthAsync();
}
