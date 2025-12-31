using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetCountAsync();
    Task<int> GetNewCustomersThisMonthAsync();
}
