using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index()
    {
        var payments = await _paymentService.GetAllAsync();
        return View(payments);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        return View(payment);
    }
}
