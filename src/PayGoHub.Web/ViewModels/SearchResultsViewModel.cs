using PayGoHub.Application.DTOs;

namespace PayGoHub.Web.ViewModels;

public class SearchResultsViewModel
{
    public string Query { get; set; } = "";
    public List<CustomerDto> Customers { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
    public List<DeviceDto> Devices { get; set; } = new();

    public int TotalResults => Customers.Count + Payments.Count + Devices.Count;
}
