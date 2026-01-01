using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

/// <summary>
/// Activity logging service implementation
/// </summary>
public class ActivityLogService : IActivityLogService
{
    private readonly PayGoHubDbContext _dbContext;

    public ActivityLogService(PayGoHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(
        string activityType,
        string title,
        string description,
        string? entityType = null,
        Guid? entityId = null,
        string? entityIdentifier = null,
        string status = "success",
        string? performedBy = null,
        string iconClass = "bi-activity",
        string colorClass = "primary",
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var activity = new ActivityLog
            {
                ActivityType = activityType,
                Title = title,
                Description = description,
                EntityType = entityType,
                EntityId = entityId,
                EntityIdentifier = entityIdentifier,
                Status = status,
                PerformedBy = performedBy,
                IconClass = iconClass,
                ColorClass = colorClass,
                Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null
            };

            _dbContext.ActivityLogs.Add(activity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // ActivityLogs table may not exist yet - silently continue
        }
    }

    public async Task<IEnumerable<ActivityDto>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var activities = await _dbContext.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            return activities.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<ActivityDto>();
        }
    }

    public async Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, Guid entityId, int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var activities = await _dbContext.ActivityLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            return activities.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<ActivityDto>();
        }
    }

    public async Task<IEnumerable<ActivityDto>> GetByEntityIdentifierAsync(string entityType, string identifier, int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var activities = await _dbContext.ActivityLogs
                .Where(a => a.EntityType == entityType && a.EntityIdentifier == identifier)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            return activities.Select(MapToDto);
        }
        catch
        {
            return Enumerable.Empty<ActivityDto>();
        }
    }

    private static ActivityDto MapToDto(ActivityLog log)
    {
        return new ActivityDto
        {
            Title = log.Title,
            Description = log.Description,
            TimeAgo = GetTimeAgo(log.CreatedAt),
            IconClass = log.IconClass,
            ColorClass = log.ColorClass
        };
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalSeconds < 60)
            return "Just now";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays}d ago";
        return dateTime.ToString("MMM dd");
    }
}
