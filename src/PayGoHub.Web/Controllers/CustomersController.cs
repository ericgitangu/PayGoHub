using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.Common;
using PayGoHub.Application.DTOs;
using PayGoHub.Application.DTOs.M2M;
using PayGoHub.Application.DTOs.Mega;
using PayGoHub.Application.Interfaces;
using PayGoHub.Web.ViewModels;

namespace PayGoHub.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IDeviceService _deviceService;
    private readonly IPaymentService _paymentService;
    private readonly IM2MCommandService _m2mService;
    private readonly IMegaSmsService _smsService;
    private readonly IActivityLogService _activityLog;
    private const int DefaultPageSize = 10;

    public CustomersController(
        ICustomerService customerService,
        IDeviceService deviceService,
        IPaymentService paymentService,
        IM2MCommandService m2mService,
        IMegaSmsService smsService,
        IActivityLogService activityLog)
    {
        _customerService = customerService;
        _deviceService = deviceService;
        _paymentService = paymentService;
        _m2mService = m2mService;
        _smsService = smsService;
        _activityLog = activityLog;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultPageSize, string? search = null)
    {
        ViewData["ActivePage"] = "Customers";
        ViewData["CurrentSearch"] = search;

        var allCustomers = await _customerService.GetAllAsync();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            allCustomers = allCustomers.Where(c =>
                (c.FirstName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.LastName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.PhoneNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var paginatedList = PaginatedList<CustomerDto>.Create(allCustomers, page, pageSize);
        return View(paginatedList);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        ViewData["ActivePage"] = "Customers";

        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        // Get customer's devices
        var allDevices = await _deviceService.GetAllAsync();
        var customerDevices = allDevices.Where(d => d.CustomerName == customer.FullName).ToList();

        // Get customer's recent payments
        var allPayments = await _paymentService.GetAllAsync();
        var customerPayments = allPayments
            .Where(p => p.CustomerId == customer.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToList();

        var viewModel = new CustomerDetailsViewModel
        {
            Customer = customer,
            Devices = customerDevices,
            RecentPayments = customerPayments
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendDeviceCommand(Guid customerId, string deviceSerial, string commandName)
    {
        var customer = await _customerService.GetByIdAsync(customerId);
        if (customer == null)
            return NotFound();

        var request = new CommandRequestDto
        {
            Identifier = new IdentifierDto { Kind = "serial", Value = deviceSerial },
            Command = new CommandDetailDto { Name = commandName },
            CallbackUrl = string.Empty
        };

        var result = await _m2mService.CreateCommandAsync(request);

        await _activityLog.LogAsync(
            "m2m_command_from_customer",
            $"M2M Command: {commandName}",
            $"Sent {commandName} to device {deviceSerial} for customer {customer.FullName}",
            "Customer", customerId, customer.AccountNumber,
            result.Status == "pending" ? "pending" : "success",
            User.Identity?.Name,
            "bi-broadcast", "info",
            new { deviceSerial, commandName, commandId = result.CommandId }
        );

        TempData["CommandResult"] = $"Command '{commandName}' sent to device {deviceSerial}";
        TempData["CommandStatus"] = result.Status;

        return RedirectToAction(nameof(Details), new { id = customerId });
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        await _customerService.CreateAsync(dto);
        TempData["Success"] = "Customer created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null)
            return NotFound();

        var updateDto = new UpdateCustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            Region = customer.Region,
            District = customer.District,
            Address = customer.Address,
            Status = customer.Status
        };

        return View(updateDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCustomerDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(dto);

        var result = await _customerService.UpdateAsync(id, dto);
        if (result == null)
            return NotFound();

        TempData["Success"] = "Customer updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _customerService.DeleteAsync(id);
        if (!result)
            return NotFound();

        TempData["Success"] = "Customer deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSms(Guid customerId, string text, string? category)
    {
        var customer = await _customerService.GetByIdAsync(customerId);
        if (customer == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
        {
            TempData["SmsError"] = "Customer has no phone number";
            return RedirectToAction(nameof(Details), new { id = customerId });
        }

        // Remove any leading + from phone number
        var recipient = customer.PhoneNumber.TrimStart('+');

        var request = new SmsRequestDto
        {
            Recipient = recipient,
            Text = text,
            Sender = "SOLARIUM",
            Category = category,
            InstanceSmsId = new Random().Next(100000, 999999)
        };

        var result = await _smsService.SendSmsAsync(request);

        await _activityLog.LogAsync(
            "sms_sent",
            "SMS Sent",
            $"SMS sent to {customer.FullName} ({recipient}): {text.Substring(0, Math.Min(50, text.Length))}...",
            "Customer", customerId, customer.AccountNumber,
            result.Status == 200 ? "success" : "failed",
            User.Identity?.Name,
            "bi-chat-dots", result.Status == 200 ? "success" : "danger",
            new { recipient, category, megaSmsId = result.MegaSmsId, status = result.Status }
        );

        if (result.Status == 200)
        {
            TempData["SmsSuccess"] = $"SMS sent successfully (ID: {result.MegaSmsId})";
        }
        else if (result.Status == 303)
        {
            TempData["SmsWarning"] = $"Duplicate SMS: {result.Description}";
        }
        else
        {
            TempData["SmsError"] = result.Description ?? "Failed to send SMS";
        }

        return RedirectToAction(nameof(Details), new { id = customerId });
    }
}
