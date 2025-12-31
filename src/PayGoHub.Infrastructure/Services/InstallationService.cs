using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

public class InstallationService : IInstallationService
{
    private readonly PayGoHubDbContext _context;

    public InstallationService(PayGoHubDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InstallationDto>> GetAllAsync()
    {
        return await _context.Installations
            .Include(i => i.Customer)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InstallationDto
            {
                Id = i.Id,
                CustomerName = i.Customer.FirstName + " " + i.Customer.LastName,
                CustomerInitials = (i.Customer.FirstName.Substring(0, 1) + i.Customer.LastName.Substring(0, 1)).ToUpper(),
                Location = i.Location ?? "",
                SystemType = i.SystemType,
                Status = i.Status.ToString(),
                StatusClass = GetStatusClass(i.Status),
                ScheduledDate = i.ScheduledDate,
                ScheduledDateFormatted = FormatScheduledDate(i.ScheduledDate)
            })
            .ToListAsync();
    }

    public async Task<InstallationDto?> GetByIdAsync(Guid id)
    {
        var installation = await _context.Installations
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (installation == null)
            return null;

        return new InstallationDto
        {
            Id = installation.Id,
            CustomerName = installation.Customer.FullName,
            CustomerInitials = (installation.Customer.FirstName.Substring(0, 1) + installation.Customer.LastName.Substring(0, 1)).ToUpper(),
            Location = installation.Location ?? "",
            SystemType = installation.SystemType,
            Status = installation.Status.ToString(),
            StatusClass = GetStatusClass(installation.Status),
            ScheduledDate = installation.ScheduledDate,
            ScheduledDateFormatted = FormatScheduledDate(installation.ScheduledDate)
        };
    }

    public async Task<IEnumerable<InstallationDto>> GetPendingAsync()
    {
        return await _context.Installations
            .Include(i => i.Customer)
            .Where(i => i.Status == InstallationStatus.Pending || i.Status == InstallationStatus.Scheduled || i.Status == InstallationStatus.InProgress)
            .OrderBy(i => i.ScheduledDate)
            .Select(i => new InstallationDto
            {
                Id = i.Id,
                CustomerName = i.Customer.FirstName + " " + i.Customer.LastName,
                CustomerInitials = (i.Customer.FirstName.Substring(0, 1) + i.Customer.LastName.Substring(0, 1)).ToUpper(),
                Location = i.Location ?? "",
                SystemType = i.SystemType,
                Status = i.Status.ToString(),
                StatusClass = GetStatusClass(i.Status),
                ScheduledDate = i.ScheduledDate,
                ScheduledDateFormatted = FormatScheduledDate(i.ScheduledDate)
            })
            .ToListAsync();
    }

    public async Task<int> GetInstallationsThisMonthAsync()
    {
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _context.Installations
            .Where(i => i.CompletedDate >= thisMonth || (i.ScheduledDate >= thisMonth && i.Status != InstallationStatus.Failed))
            .CountAsync();
    }

    public async Task<decimal> GetInstallationGrowthAsync()
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var thisMonthInstalls = await _context.Installations
            .Where(i => i.CreatedAt >= thisMonth)
            .CountAsync();

        var lastMonthInstalls = await _context.Installations
            .Where(i => i.CreatedAt >= lastMonth && i.CreatedAt < thisMonth)
            .CountAsync();

        if (lastMonthInstalls == 0)
            return 100;

        return Math.Round(((decimal)(thisMonthInstalls - lastMonthInstalls) / lastMonthInstalls) * 100, 1);
    }

    private static string GetStatusClass(InstallationStatus status) => status switch
    {
        InstallationStatus.Completed => "success",
        InstallationStatus.InProgress => "primary",
        InstallationStatus.Scheduled => "info",
        InstallationStatus.Pending => "warning",
        InstallationStatus.Failed => "danger",
        _ => "secondary"
    };

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

        return scheduleDate.ToString("MMM dd");
    }
}
