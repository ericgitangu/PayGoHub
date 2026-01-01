using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Common;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Application.Interfaces;
using PayGoHub.Web.ViewModels;

namespace PayGoHub.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;
    private readonly IMomoPaymentService _momoService;
    private readonly IActivityLogService _activityLog;
    private const int DefaultPageSize = 10;

    public PaymentsController(
        IPaymentService paymentService,
        ICustomerService customerService,
        IMomoPaymentService momoService,
        IActivityLogService activityLog)
    {
        _paymentService = paymentService;
        _customerService = customerService;
        _momoService = momoService;
        _activityLog = activityLog;
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
        ViewData["ActivePage"] = "Payments";

        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        CustomerDto? customer = null;
        if (payment.CustomerId != Guid.Empty)
        {
            customer = await _customerService.GetByIdAsync(payment.CustomerId);
        }

        var viewModel = new PaymentDetailsViewModel
        {
            Payment = payment,
            Customer = customer,
            CanValidate = payment.Status == "Pending",
            CanConfirm = payment.Status == "Pending" || payment.Status == "Validated"
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidatePayment(Guid id, string reference, string providerKey, string currency, decimal amount, string businessAccount)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        var request = new ValidationRequestDto
        {
            Reference = reference,
            ProviderKey = providerKey,
            Currency = currency,
            AmountSubunit = (long)(amount * 100),
            BusinessAccount = businessAccount,
            AdditionalFields = new[] { "customer_name" }
        };

        var result = await _momoService.ValidateAsync(request);

        if (result.Status == "ok")
        {
            TempData["ValidationSuccess"] = true;
            TempData["CustomerName"] = result.CustomerName;

            await _activityLog.LogAsync(
                "momo_validation",
                "MoMo Payment Validated",
                $"Validated payment {reference} for customer {result.CustomerName}",
                "Payment", id, reference,
                "success", User.Identity?.Name,
                "bi-check-circle-fill", "success",
                new { providerKey, currency, amount }
            );
        }
        else
        {
            TempData["ValidationError"] = result.Error ?? result.ErrorMessage ?? "Validation failed";

            await _activityLog.LogAsync(
                "momo_validation_failed",
                "MoMo Validation Failed",
                $"Failed to validate payment {reference}: {result.Error}",
                "Payment", id, reference,
                "failed", User.Identity?.Name,
                "bi-x-circle-fill", "danger",
                new { providerKey, error = result.Error }
            );
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(
        Guid id,
        string reference,
        string providerKey,
        string currency,
        decimal amount,
        string businessAccount,
        string providerTx,
        string? senderPhone,
        string? senderName)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound();

        var request = new ConfirmationRequestDto
        {
            Reference = reference,
            ProviderKey = providerKey,
            Currency = currency,
            AmountSubunit = (long)(amount * 100),
            BusinessAccount = businessAccount,
            ProviderTx = providerTx,
            SenderPhoneNumber = senderPhone,
            SenderName = senderName,
            ReceivedAt = DateTime.UtcNow
        };

        var result = await _momoService.ConfirmAsync(request);

        var isDuplicate = result.ErrorCode == "duplicate";

        if (result.Status == "ok")
        {
            TempData["ConfirmationSuccess"] = true;

            await _activityLog.LogAsync(
                "momo_confirmation",
                "MoMo Payment Confirmed",
                $"Confirmed payment {reference} - {currency} {amount:N2}",
                "Payment", id, reference,
                "success", User.Identity?.Name,
                "bi-cash-coin", "success",
                new { providerKey, providerTx, amount, senderPhone }
            );
        }
        else
        {
            TempData["ConfirmationError"] = result.Error ?? "Confirmation failed";
            if (isDuplicate)
            {
                TempData["IsDuplicate"] = true;
            }

            await _activityLog.LogAsync(
                "momo_confirmation_failed",
                "MoMo Confirmation Failed",
                $"Failed to confirm payment {reference}: {result.Error}",
                "Payment", id, reference,
                "failed", User.Identity?.Name,
                "bi-exclamation-triangle-fill", "danger",
                new { providerKey, providerTx, error = result.Error, isDuplicate }
            );
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
