using PayGoHub.Application.DTOs;
using PayGoHub.Application.DTOs.M2M;

namespace PayGoHub.Web.ViewModels;

public class DeviceDetailsViewModel
{
    public DeviceDto Device { get; set; } = null!;
    public IEnumerable<CommandResponseDto> RecentCommands { get; set; } = new List<CommandResponseDto>();

    // Token generation options
    public static readonly Dictionary<string, string> TokenCommands = new()
    {
        { "unlock_relative", "Unlock Relative (Days)" },
        { "unlock_absolute", "Unlock Absolute (Date)" },
        { "set_time", "Set Time" },
        { "disable_payg", "Disable PAYG" },
        { "counter_sync", "Counter Sync" }
    };

    // M2M command options
    public static readonly Dictionary<string, string> M2MCommands = new()
    {
        { "unlock_token", "Send Unlock Token" },
        { "request_status", "Request Status" },
        { "firmware_update", "Firmware Update" },
        { "reset_device", "Reset Device" },
        { "sync_time", "Sync Time" }
    };
}
