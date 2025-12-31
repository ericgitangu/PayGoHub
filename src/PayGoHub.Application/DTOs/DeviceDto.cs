namespace PayGoHub.Application.DTOs;

public class DeviceDto
{
    public Guid Id { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public int BatteryHealth { get; set; }
    public DateTime? LastSyncDate { get; set; }
    public string? CustomerName { get; set; }
}

public class CreateDeviceDto
{
    public string SerialNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
}
