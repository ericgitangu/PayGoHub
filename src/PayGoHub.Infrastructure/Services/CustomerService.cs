using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly PayGoHubDbContext _context;

    public CustomerService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        return await _context.Customers
            .Include(c => c.Payments)
            .Include(c => c.Loans)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                FullName = c.FirstName + " " + c.LastName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                AccountNumber = c.AccountNumber,
                Region = c.Region,
                District = c.District,
                Address = c.Address,
                Status = c.Status.ToString(),
                StatusClass = GetStatusClass(c.Status),
                CreatedAt = c.CreatedAt,
                TotalPayments = c.Payments.Count(p => p.Status == PaymentStatus.Completed),
                TotalPaid = c.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                ActiveLoans = c.Loans.Count(l => l.Status == LoanStatus.Active)
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var customer = await _context.Customers
            .Include(c => c.Payments)
            .Include(c => c.Loans)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return null;

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AccountNumber = customer.AccountNumber,
            Region = customer.Region,
            District = customer.District,
            Address = customer.Address,
            Status = customer.Status.ToString(),
            StatusClass = GetStatusClass(customer.Status),
            CreatedAt = customer.CreatedAt,
            TotalPayments = customer.Payments.Count(p => p.Status == PaymentStatus.Completed),
            TotalPaid = customer.Payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
            ActiveLoans = customer.Loans.Count(l => l.Status == LoanStatus.Active)
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Region = dto.Region,
            District = dto.District,
            Address = dto.Address,
            Status = CustomerStatus.Active
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AccountNumber = customer.AccountNumber,
            Region = customer.Region,
            District = customer.District,
            Address = customer.Address,
            Status = customer.Status.ToString(),
            StatusClass = GetStatusClass(customer.Status),
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return null;

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Email = dto.Email;
        customer.PhoneNumber = dto.PhoneNumber;
        customer.Region = dto.Region;
        customer.District = dto.District;
        customer.Address = dto.Address;

        if (Enum.TryParse<CustomerStatus>(dto.Status, out var status))
            customer.Status = status;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return false;

        customer.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Customers.CountAsync();
    }

    public async Task<int> GetNewCustomersThisMonthAsync()
    {
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _context.Customers
            .Where(c => c.CreatedAt >= thisMonth)
            .CountAsync();
    }

    private static string GetStatusClass(CustomerStatus status) => status switch
    {
        CustomerStatus.Active => "success",
        CustomerStatus.Inactive => "secondary",
        CustomerStatus.Suspended => "warning",
        CustomerStatus.Blacklisted => "danger",
        _ => "secondary"
    };
}
