using PayGoHub.Application.DTOs;
using PayGoHub.Application.DTOs.M2M;

namespace PayGoHub.Web.ViewModels;

public class CustomerDetailsViewModel
{
    public CustomerDto Customer { get; set; } = null!;
    public IEnumerable<DeviceDto> Devices { get; set; } = new List<DeviceDto>();
    public IEnumerable<PaymentDto> RecentPayments { get; set; } = new List<PaymentDto>();
    public IEnumerable<LoanDto> ActiveLoans { get; set; } = new List<LoanDto>();

    // Quick action commands for devices
    public static readonly Dictionary<string, string> QuickCommands = new()
    {
        { "request_status", "Request Status" },
        { "sync_time", "Sync Time" },
        { "unlock_token", "Send Unlock Token" }
    };
}

public class LoanDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalPayments { get; set; }
}
