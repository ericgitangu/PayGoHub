using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllAsync();
    Task<LoanDto?> GetByIdAsync(Guid id);
    Task<LoanDto> CreateAsync(CreateLoanDto dto);
    Task<int> GetActiveLoansCountAsync();
    Task<decimal> GetLoanGrowthAsync();
}
