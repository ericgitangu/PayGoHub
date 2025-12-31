using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

public class Installation : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid? DeviceId { get; set; }
    public string SystemType { get; set; } = "SHS-80W";
    public InstallationStatus Status { get; set; } = InstallationStatus.Pending;
    public DateTime? ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Location { get; set; }
    public string? TechnicianName { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Device? Device { get; set; }
}
