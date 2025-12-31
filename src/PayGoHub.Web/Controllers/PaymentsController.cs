using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Common;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private const int DefaultPageSize = 10;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize, string? search = null)
    {
        ViewData["ActivePage"] = "Payments";
        ViewData["CurrentSearch"] = search;

        var allPayments = await _paymentService.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            allPayments = allPayments.Where(p =>
                (p.Reference?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                p.Amount.ToString().Contains(search));
        }

        var paginatedList = PaginatedList<PaymentDto>.Create(allPayments, page, pageSize);
        return View(paginatedList);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        return View(payment);
    }
}
