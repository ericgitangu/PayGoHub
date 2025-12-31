using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly PayGoHubDbContext _context;

    public PaymentService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllAsync()
    {
        return await _context.Payments
            .Include(p => p.Customer)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                CustomerName = p.Customer.FirstName + " " + p.Customer.LastName,
                CustomerInitials = (p.Customer.FirstName.Substring(0, 1) + p.Customer.LastName.Substring(0, 1)).ToUpper(),
                Amount = p.Amount,
                Method = p.Method.ToString(),
                Status = p.Status.ToString(),
                StatusClass = GetStatusClass(p.Status),
                PaidAt = p.PaidAt
            })
            .ToListAsync();
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id)
    {
        var payment = await _context.Payments
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            CustomerName = payment.Customer.FullName,
            CustomerInitials = (payment.Customer.FirstName.Substring(0, 1) + payment.Customer.LastName.Substring(0, 1)).ToUpper(),
            Amount = payment.Amount,
            Method = payment.Method.ToString(),
            Status = payment.Status.ToString(),
            StatusClass = GetStatusClass(payment.Status),
            PaidAt = payment.PaidAt,
            TimeAgo = GetTimeAgo(payment.PaidAt ?? payment.CreatedAt)
        };
    }

    public async Task<IEnumerable<PaymentDto>> GetRecentPaymentsAsync(int count)
    {
        var payments = await _context.Payments
            .Include(p => p.Customer)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                CustomerName = p.Customer.FirstName + " " + p.Customer.LastName,
                CustomerInitials = (p.Customer.FirstName.Substring(0, 1) + p.Customer.LastName.Substring(0, 1)).ToUpper(),
                Amount = p.Amount,
                Method = p.Method.ToString(),
                Status = p.Status.ToString(),
                StatusClass = GetStatusClass(p.Status),
                PaidAt = p.PaidAt
            })
            .ToListAsync();

        foreach (var payment in payments)
        {
            payment.TimeAgo = GetTimeAgo(payment.PaidAt ?? DateTime.UtcNow);
        }

        return payments;
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount);
    }

    public async Task<decimal> GetRevenueGrowthAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var thisMonthRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= thisMonth)
            .SumAsync(p => p.Amount);

        var lastMonthRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= lastMonth && p.PaidAt < thisMonth)
            .SumAsync(p => p.Amount);

        if (lastMonthRevenue == 0)
            return 100;

        return Math.Round(((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100, 1);
    }

    private static string GetStatusClass(PaymentStatus status) => status switch
    {
        PaymentStatus.Completed => "success",
        PaymentStatus.Pending => "warning",
        PaymentStatus.Failed => "danger",
        PaymentStatus.Reversed => "secondary",
        _ => "secondary"
    };

    private static string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;

        if (span.TotalMinutes < 1)
            return "Just now";
        if (span.TotalMinutes < 60)
            return $"{(int)span.TotalMinutes} min ago";
        if (span.TotalHours < 24)
            return $"{(int)span.TotalHours} hours ago";
        if (span.TotalDays < 7)
            return $"{(int)span.TotalDays} days ago";

        return dateTime.ToString("MMM dd");
    }
}
