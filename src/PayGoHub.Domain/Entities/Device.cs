using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

public class Device : BaseEntity
{
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DeviceStatus Status { get; set; } = DeviceStatus.Active;
    public Guid? InstallationId { get; set; }
    public int BatteryHealth { get; set; } = 100;
    public DateTime? LastSyncDate { get; set; }

    // Navigation property
    public virtual Installation? Installation { get; set; }
}
