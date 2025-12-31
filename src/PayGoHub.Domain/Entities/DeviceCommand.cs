using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

/// <summary>
/// M2M device command for async device communication
/// </summary>
public class DeviceCommand : BaseEntity
{
    /// <summary>Device serial number/identifier</summary>
    public string DeviceIdentifier { get; set; } = string.Empty;

    /// <summary>Type of identifier (e.g., "serial", "imei")</summary>
    public string IdentifierKind { get; set; } = "serial";

    /// <summary>Command name (e.g., "unlock_token", "sync", "reset")</summary>
    public string CommandName { get; set; } = string.Empty;

    /// <summary>Command-specific details as JSON (e.g., {"unlock_code": "123456"})</summary>
    public string? CommandDetails { get; set; }

    /// <summary>Current command status</summary>
    public CommandStatus Status { get; set; } = CommandStatus.Pending;

    /// <summary>URL to call when command status changes</summary>
    public string? CallbackUrl { get; set; }

    /// <summary>Response received from device</summary>
    public string? DeviceResponse { get; set; }

    /// <summary>Timestamp when command was sent to device</summary>
    public DateTime? SentAt { get; set; }

    /// <summary>Timestamp when command was executed</summary>
    public DateTime? ExecutedAt { get; set; }

    /// <summary>Timestamp when callback was delivered</summary>
    public DateTime? CallbackDeliveredAt { get; set; }

    /// <summary>Error message if command failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Number of retry attempts</summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>API client that created this command</summary>
    public Guid ApiClientId { get; set; }
    public virtual ApiClient ApiClient { get; set; } = null!;
}
