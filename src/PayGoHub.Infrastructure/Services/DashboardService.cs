using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly PayGoHubDbContext _context;

    public DashboardService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = thisMonth.AddMonths(-1);

        // Calculate KPIs
        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount);

        var thisMonthRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= thisMonth)
            .SumAsync(p => p.Amount);

        var lastMonthRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= lastMonth && p.PaidAt < thisMonth)
            .SumAsync(p => p.Amount);

        var revenueGrowth = lastMonthRevenue > 0
            ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
            : 100;

        var newCustomers = await _context.Customers
            .Where(c => c.CreatedAt >= thisMonth)
            .CountAsync();

        var lastMonthCustomers = await _context.Customers
            .Where(c => c.CreatedAt >= lastMonth && c.CreatedAt < thisMonth)
            .CountAsync();

        var customerGrowth = lastMonthCustomers > 0
            ? ((decimal)(newCustomers - lastMonthCustomers) / lastMonthCustomers) * 100
            : 100;

        var activeLoans = await _context.Loans
            .Where(l => l.Status == LoanStatus.Active)
            .CountAsync();

        var thisMonthInstallations = await _context.Installations
            .Where(i => i.CompletedDate >= thisMonth || i.ScheduledDate >= thisMonth)
            .CountAsync();

        // Get Regional Sales
        var regionalSales = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .Include(p => p.Customer)
            .GroupBy(p => p.Customer.Region)
            .Select(g => new RegionalSalesDto
            {
                Region = g.Key,
                Amount = g.Sum(p => p.Amount)
            })
            .OrderByDescending(r => r.Amount)
            .Take(5)
            .ToListAsync();

        var maxRegionalAmount = regionalSales.Any() ? regionalSales.Max(r => r.Amount) : 1;
        var colorClasses = new[] { "primary", "success", "info", "warning", "danger" };
        for (int i = 0; i < regionalSales.Count; i++)
        {
            regionalSales[i].Percentage = (regionalSales[i].Amount / maxRegionalAmount) * 100;
            regionalSales[i].ColorClass = colorClasses[i % colorClasses.Length];
        }

        // Get Recent Payments
        var recentPayments = await _context.Payments
            .Include(p => p.Customer)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                CustomerName = p.Customer.FirstName + " " + p.Customer.LastName,
                CustomerInitials = (p.Customer.FirstName.Substring(0, 1) + p.Customer.LastName.Substring(0, 1)).ToUpper(),
                Amount = p.Amount,
                Method = p.Method.ToString(),
                Status = p.Status.ToString(),
                StatusClass = GetPaymentStatusClass(p.Status),
                PaidAt = p.PaidAt
            })
            .ToListAsync();

        foreach (var payment in recentPayments)
        {
            payment.TimeAgo = GetTimeAgo(payment.PaidAt ?? now);
        }

        // Get Pending Installations
        var pendingInstallations = await _context.Installations
            .Include(i => i.Customer)
            .Where(i => i.Status == InstallationStatus.Pending || i.Status == InstallationStatus.Scheduled || i.Status == InstallationStatus.InProgress)
            .OrderBy(i => i.ScheduledDate)
            .Take(5)
            .Select(i => new InstallationDto
            {
                Id = i.Id,
                CustomerName = i.Customer.FirstName + " " + i.Customer.LastName,
                CustomerInitials = (i.Customer.FirstName.Substring(0, 1) + i.Customer.LastName.Substring(0, 1)).ToUpper(),
                Location = i.Location ?? "",
                SystemType = i.SystemType,
                Status = i.Status.ToString(),
                StatusClass = GetInstallationStatusClass(i.Status),
                ScheduledDate = i.ScheduledDate
            })
            .ToListAsync();

        foreach (var installation in pendingInstallations)
        {
            installation.ScheduledDateFormatted = FormatScheduledDate(installation.ScheduledDate);
        }

        // Get Monthly Revenue for Chart (last 6 months)
        var sixMonthsAgo = now.AddMonths(-6);
        var monthlyRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaidAt >= sixMonthsAgo)
            .GroupBy(p => new { Year = p.PaidAt!.Value.Year, Month = p.PaidAt!.Value.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(p => p.Amount),
                Payments = g.Count()
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync();

        var monthlyRevenueDto = new List<MonthlyRevenueDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthDate = now.AddMonths(-i);
            var monthData = monthlyRevenue.FirstOrDefault(m => m.Year == monthDate.Year && m.Month == monthDate.Month);
            monthlyRevenueDto.Add(new MonthlyRevenueDto
            {
                Month = monthDate.ToString("MMM"),
                Revenue = monthData?.Revenue ?? 0,
                Payments = monthData?.Payments ?? 0
            });
        }

        // Get Recent Activity - prioritize ActivityLog entries (M-Services operations)
        var recentActivity = new List<ActivityDto>();

        // Get activities from ActivityLog table (includes M-Services operations)
        // Use try-catch to handle case where table doesn't exist yet (migration not applied)
        try
        {
            var loggedActivities = await _context.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new ActivityDto
                {
                    Title = a.Title,
                    Description = a.Description,
                    TimeAgo = GetTimeAgo(a.CreatedAt),
                    IconClass = a.IconClass,
                    ColorClass = a.ColorClass
                })
                .ToListAsync();

            recentActivity.AddRange(loggedActivities);
        }
        catch (Exception)
        {
            // ActivityLogs table may not exist yet - continue with other activities
        }

        // Also include recent payments if we need more activities
        if (recentActivity.Count < 5)
        {
            var recentPaymentActivities = await _context.Payments
                .Include(p => p.Customer)
                .Where(p => p.Status == PaymentStatus.Completed)
                .OrderByDescending(p => p.PaidAt)
                .Take(5 - recentActivity.Count)
                .ToListAsync();

            foreach (var p in recentPaymentActivities)
            {
                recentActivity.Add(new ActivityDto
                {
                    Title = "Payment Received",
                    Description = $"KES {p.Amount:N0} from {p.Customer.FirstName} {p.Customer.LastName}",
                    TimeAgo = GetTimeAgo(p.PaidAt ?? p.CreatedAt),
                    IconClass = "bi-cash",
                    ColorClass = "success"
                });
            }
        }

        // Include recent installations if we still need more
        if (recentActivity.Count < 5)
        {
            var recentInstallationActivities = await _context.Installations
                .Include(i => i.Customer)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5 - recentActivity.Count)
                .ToListAsync();

            foreach (var i in recentInstallationActivities)
            {
                recentActivity.Add(new ActivityDto
                {
                    Title = i.Status == InstallationStatus.Completed ? "Installation Completed" : "Installation Scheduled",
                    Description = $"{i.SystemType} for {i.Customer.FirstName} {i.Customer.LastName}",
                    TimeAgo = GetTimeAgo(i.CreatedAt),
                    IconClass = "bi-tools",
                    ColorClass = i.Status == InstallationStatus.Completed ? "primary" : "info"
                });
            }
        }

        return new DashboardViewModel
        {
            TotalRevenue = totalRevenue,
            RevenueGrowth = Math.Round(revenueGrowth, 1),
            NewCustomers = newCustomers > 0 ? newCustomers : await _context.Customers.CountAsync(),
            CustomerGrowth = Math.Round(customerGrowth, 1),
            ActiveLoans = activeLoans,
            LoanGrowth = 8.3m, // Placeholder
            Installations = thisMonthInstallations,
            InstallationGrowth = 15.2m, // Placeholder
            RegionalSales = regionalSales,
            RecentPayments = recentPayments,
            PendingInstallations = pendingInstallations,
            RecentActivity = recentActivity.OrderByDescending(a => a.TimeAgo).Take(5).ToList(),
            MonthlyRevenue = monthlyRevenueDto
        };
    }

    private static string GetPaymentStatusClass(PaymentStatus status) => status switch
    {
        PaymentStatus.Completed => "success",
        PaymentStatus.Pending => "warning",
        PaymentStatus.Failed => "danger",
        PaymentStatus.Reversed => "secondary",
        _ => "secondary"
    };

    private static string GetInstallationStatusClass(InstallationStatus status) => status switch
    {
        InstallationStatus.Completed => "success",
        InstallationStatus.InProgress => "primary",
        InstallationStatus.Scheduled => "info",
        InstallationStatus.Pending => "warning",
        InstallationStatus.Failed => "danger",
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

    private static string FormatScheduledDate(DateTime? date)
    {
        if (!date.HasValue)
            return "TBD";

        var today = DateTime.UtcNow.Date;
        var scheduleDate = date.Value.Date;

        if (scheduleDate == today)
            return "Today";
        if (scheduleDate == today.AddDays(1))
            return "Tomorrow";
        if (scheduleDate < today)
            return scheduleDate.ToString("MMM dd");

        return scheduleDate.ToString("MMM dd");
    }
}
