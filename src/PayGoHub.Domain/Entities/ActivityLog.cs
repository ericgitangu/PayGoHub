namespace PayGoHub.Domain.Entities;

/// <summary>
/// Tracks all system activities for audit and display
/// </summary>
public class ActivityLog : BaseEntity
{
    /// <summary>Activity type (token_generated, m2m_command, payment, etc.)</summary>
    public string ActivityType { get; set; } = string.Empty;

    /// <summary>Title for display</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description details</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Entity type being acted upon (Device, Customer, Payment)</summary>
    public string? EntityType { get; set; }

    /// <summary>Entity ID being acted upon</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Additional identifier (e.g., serial number)</summary>
    public string? EntityIdentifier { get; set; }

    /// <summary>Status of the activity (success, pending, failed)</summary>
    public string Status { get; set; } = "success";

    /// <summary>User who performed the action (if authenticated)</summary>
    public string? PerformedBy { get; set; }

    /// <summary>Icon class for display</summary>
    public string IconClass { get; set; } = "bi-activity";

    /// <summary>Color class for display (primary, success, warning, danger, info)</summary>
    public string ColorClass { get; set; } = "primary";

    /// <summary>Additional metadata as JSON</summary>
    public string? Metadata { get; set; }
}
