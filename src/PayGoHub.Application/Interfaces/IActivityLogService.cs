using PayGoHub.Application.DTOs;

namespace PayGoHub.Application.Interfaces;

/// <summary>
/// Service for logging and retrieving system activities
/// </summary>
public interface IActivityLogService
{
    /// <summary>Log an activity</summary>
    Task LogAsync(
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
        CancellationToken cancellationToken = default);

    /// <summary>Get recent activities</summary>
    Task<IEnumerable<ActivityDto>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default);

    /// <summary>Get activities for a specific entity</summary>
    Task<IEnumerable<ActivityDto>> GetByEntityAsync(string entityType, Guid entityId, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>Get activities for a specific entity by identifier</summary>
    Task<IEnumerable<ActivityDto>> GetByEntityIdentifierAsync(string entityType, string identifier, int count = 10, CancellationToken cancellationToken = default);
}
