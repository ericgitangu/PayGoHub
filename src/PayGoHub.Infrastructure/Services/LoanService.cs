using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class LoanService : ILoanService
{
    private readonly PayGoHubDbContext _context;

    public LoanService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LoanDto>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.Customer)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LoanDto
            {
                Id = l.Id,
                CustomerId = l.CustomerId,
                CustomerName = l.Customer.FirstName + " " + l.Customer.LastName,
                Amount = l.Amount,
                InterestRate = l.InterestRate,
                Status = l.Status.ToString(),
                StatusClass = GetStatusClass(l.Status),
                IssuedDate = l.IssuedDate,
                DueDate = l.DueDate,
                RemainingBalance = l.RemainingBalance,
                Notes = l.Notes
            })
            .ToListAsync();
    }

    public async Task<LoanDto?> GetByIdAsync(Guid id)
    {
        var loan = await _context.Loans
            .Include(l => l.Customer)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
            return null;

        return new LoanDto
        {
            Id = loan.Id,
            CustomerId = loan.CustomerId,
            CustomerName = loan.Customer.FullName,
            Amount = loan.Amount,
            InterestRate = loan.InterestRate,
            Status = loan.Status.ToString(),
            StatusClass = GetStatusClass(loan.Status),
            IssuedDate = loan.IssuedDate,
            DueDate = loan.DueDate,
            RemainingBalance = loan.RemainingBalance,
            Notes = loan.Notes
        };
    }

    public async Task<LoanDto> CreateAsync(CreateLoanDto dto)
    {
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            CustomerId = dto.CustomerId,
            Amount = dto.Amount,
            InterestRate = dto.InterestRate,
            Status = LoanStatus.Pending,
            IssuedDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddMonths(dto.DurationMonths),
            RemainingBalance = dto.Amount,
            Notes = dto.Notes
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(loan.Id) ?? throw new InvalidOperationException("Failed to create loan");
    }

    public async Task<int> GetActiveLoansCountAsync()
    {
        return await _context.Loans
            .Where(l => l.Status == LoanStatus.Active)
            .CountAsync();
    }

    public async Task<decimal> GetLoanGrowthAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var thisMonthLoans = await _context.Loans
            .Where(l => l.CreatedAt >= thisMonth)
            .CountAsync();

        var lastMonthLoans = await _context.Loans
            .Where(l => l.CreatedAt >= lastMonth && l.CreatedAt < thisMonth)
            .CountAsync();

        if (lastMonthLoans == 0)
            return 100;

        return Math.Round(((decimal)(thisMonthLoans - lastMonthLoans) / lastMonthLoans) * 100, 1);
    }

    private static string GetStatusClass(LoanStatus status) => status switch
    {
        LoanStatus.Active => "primary",
        LoanStatus.PaidOff => "success",
        LoanStatus.Pending => "warning",
        LoanStatus.Defaulted => "danger",
        LoanStatus.Rescheduled => "info",
        _ => "secondary"
    };
}
