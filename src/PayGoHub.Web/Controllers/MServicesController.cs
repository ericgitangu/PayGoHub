using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.M2M;
using PayGoHub.Application.DTOs.Mega;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Application.DTOs.Tokens;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers;

/// <summary>
/// M-Services demo and testing pages
/// Provides UI for testing MOTO, MEGA, MOMOEP, and M2M APIs
/// </summary>
[Authorize]
public class MServicesController : Controller
{
    private readonly ITokenGenerationService _tokenService;
    private readonly IMegaSmsService _smsService;
    private readonly IMomoPaymentService _paymentService;
    private readonly IM2MCommandService _m2mService;
    private readonly ILogger<MServicesController> _logger;

    public MServicesController(
        ITokenGenerationService tokenService,
        IMegaSmsService smsService,
        IMomoPaymentService paymentService,
        IM2MCommandService m2mService,
        ILogger<MServicesController> logger)
    {
        _tokenService = tokenService;
        _smsService = smsService;
        _paymentService = paymentService;
        _m2mService = m2mService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["ActivePage"] = "MServices";
        return View();
    }

    #region MOTO - Token Generation

    public IActionResult TokenGenerator()
    {
        ViewData["ActivePage"] = "MServices";
        ViewData["SubPage"] = "TokenGenerator";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateToken(string device, string command, string payload, int sequenceNumber, string secret, string? encoding)
    {
        var request = new TokenGenerationRequestDto
        {
            Device = device,
            Command = command,
            Payload = payload,
            SequenceNumber = sequenceNumber,
            Secret = secret,
            Encoding = encoding
        };

        var result = await _tokenService.GenerateStatelessAsync(request);

        return Json(result);
    }

    #endregion

    #region MEGA - SMS Gateway

    public IActionResult SmsGateway()
    {
        ViewData["ActivePage"] = "MServices";
        ViewData["SubPage"] = "SmsGateway";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendSms(string recipient, string text, string? sender, string? category)
    {
        var request = new SmsRequestDto
        {
            Recipient = recipient,
            Text = text,
            Sender = sender ?? "SOLARIUM",
            Category = category,
            InstanceSmsId = new Random().Next(100000, 999999)
        };

        var result = await _smsService.SendSmsAsync(request);

        return Json(result);
    }

    #endregion

    #region MOMOEP - Payment Validation

    public IActionResult PaymentValidation()
    {
        ViewData["ActivePage"] = "MServices";
        ViewData["SubPage"] = "PaymentValidation";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidatePayment(string reference, string providerKey, string currency, long amountSubunit, string businessAccount)
    {
        var request = new ValidationRequestDto
        {
            Reference = reference,
            ProviderKey = providerKey,
            Currency = currency,
            AmountSubunit = amountSubunit,
            BusinessAccount = businessAccount,
            AdditionalFields = new[] { "customer_name" }
        };

        var result = await _paymentService.ValidateAsync(request);

        return Json(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(string reference, string providerKey, string currency, long amountSubunit, string businessAccount, string providerTx, string? senderPhone)
    {
        var request = new ConfirmationRequestDto
        {
            Reference = reference,
            ProviderKey = providerKey,
            Currency = currency,
            AmountSubunit = amountSubunit,
            BusinessAccount = businessAccount,
            ProviderTx = providerTx,
            SenderPhoneNumber = senderPhone,
            ReceivedAt = DateTime.UtcNow
        };

        var result = await _paymentService.ConfirmAsync(request);

        return Json(result);
    }

    #endregion

    #region M2M - Device Commands

    public IActionResult DeviceCommands()
    {
        ViewData["ActivePage"] = "MServices";
        ViewData["SubPage"] = "DeviceCommands";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCommand(string identifierKind, string identifierValue, string commandName, string? unlockCode, string? callbackUrl)
    {
        var details = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(unlockCode))
        {
            details["unlock_code"] = unlockCode;
        }

        var request = new CommandRequestDto
        {
            Identifier = new IdentifierDto
            {
                Kind = identifierKind,
                Value = identifierValue
            },
            Command = new CommandDetailDto
            {
                Name = commandName,
                Details = details.Count > 0 ? details : null
            },
            CallbackUrl = callbackUrl ?? string.Empty
        };

        var result = await _m2mService.CreateCommandAsync(request);

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetCommandStatus(string identifier)
    {
        var result = await _m2mService.GetCommandStatusAsync(identifier);

        if (result == null)
            return Json(new { error = "No command found" });

        return Json(result);
    }

    #endregion
}
