using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;
using PayGoHub.Web.ViewModels;

namespace PayGoHub.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IPaymentService _paymentService;
    private readonly IDeviceService _deviceService;

    public SearchController(
        ICustomerService customerService,
        IPaymentService paymentService,
        IDeviceService deviceService)
    {
        _customerService = customerService;
        _paymentService = paymentService;
        _deviceService = deviceService;
    }

    public async Task<IActionResult> Index(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new SearchResultsViewModel { Query = "" });
        }

        var query = q.Trim().ToLowerInvariant();
        var results = new SearchResultsViewModel { Query = q };

        // Search customers
        var customers = await _customerService.GetAllAsync();
        results.Customers = customers
            .Where(c =>
                (c.FirstName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.LastName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Email?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.PhoneNumber?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.AccountNumber?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(20)
            .ToList();

        // Search payments
        var payments = await _paymentService.GetAllAsync();
        results.Payments = payments
            .Where(p =>
                (p.Reference?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.CustomerName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                p.Amount.ToString().Contains(query))
            .Take(20)
            .ToList();

        // Search devices
        var devices = await _deviceService.GetAllAsync();
        results.Devices = devices
            .Where(d =>
                (d.SerialNumber?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Model?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Type?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(20)
            .ToList();

        return View(results);
    }

    [HttpGet("api/search")]
    public async Task<IActionResult> QuickSearch([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Json(new { results = Array.Empty<object>() });
        }

        var query = q.Trim().ToLowerInvariant();
        var results = new List<object>();

        // Search customers (limit to 5)
        var customers = await _customerService.GetAllAsync();
        var customerResults = customers
            .Where(c =>
                (c.FirstName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.LastName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.PhoneNumber?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(5)
            .Select(c => new
            {
                type = "customer",
                id = c.Id,
                title = $"{c.FirstName} {c.LastName}",
                subtitle = c.PhoneNumber,
                url = Url.Action("Details", "Customers", new { id = c.Id }),
                icon = "bi-person"
            });
        results.AddRange(customerResults);

        // Search payments (limit to 3)
        var payments = await _paymentService.GetAllAsync();
        var paymentResults = payments
            .Where(p =>
                (p.Reference?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.CustomerName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(3)
            .Select(p => new
            {
                type = "payment",
                id = p.Id,
                title = p.Reference,
                subtitle = $"{p.Amount:C} - {p.CustomerName}",
                url = Url.Action("Details", "Payments", new { id = p.Id }),
                icon = "bi-credit-card"
            });
        results.AddRange(paymentResults);

        // Search devices (limit to 5)
        var devices = await _deviceService.GetAllAsync();
        var deviceResults = devices
            .Where(d =>
                (d.SerialNumber?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.Model?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false))
            .Take(5)
            .Select(d => new
            {
                type = "device",
                id = d.Id,
                title = d.SerialNumber,
                subtitle = $"{d.Model} - {d.Status}",
                url = Url.Action("Details", "Devices", new { id = d.Id }),
                icon = "bi-cpu"
            });
        results.AddRange(deviceResults);

        return Json(new { results, query = q });
    }
}
