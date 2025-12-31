using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers.Api;

/// <summary>
/// MoMo payment API endpoints for validation and confirmation
/// </summary>
[ApiController]
[Route("api/payment")]
[Produces("application/json")]
public class PaymentApiController : ControllerBase
{
    private readonly IMomoPaymentService _paymentService;
    private readonly ILogger<PaymentApiController> _logger;

    public PaymentApiController(IMomoPaymentService paymentService, ILogger<PaymentApiController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Validate a customer account for payment
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/payment/validate
    ///     {
    ///         "reference": "254543",
    ///         "currency": "KES",
    ///         "business_account": "544544",
    ///         "provider_key": "ke_safaricom_mpesa",
    ///         "amount_subunit": 20000,
    ///         "additional_fields": ["customer_name"]
    ///     }
    /// </remarks>
    /// <param name="request">Validation request</param>
    /// <returns>Validation response with customer info</returns>
    /// <response code="200">Validation successful</response>
    /// <response code="404">Reference not found</response>
    /// <response code="412">Amount validation failed (too low/high)</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status412PreconditionFailed)]
    public async Task<IActionResult> Validate([FromBody] ValidationRequestDto request)
    {
        _logger.LogInformation("Payment validation request for reference {Reference}", request.Reference);

        var response = await _paymentService.ValidateAsync(request);

        return response.Error switch
        {
            "reference_not_found" => NotFound(response),
            "amount_too_low" or "amount_too_high" => StatusCode(StatusCodes.Status412PreconditionFailed, response),
            "provider_not_found" or "currency_mismatch" => BadRequest(response),
            _ when response.Status == "error" => BadRequest(response),
            _ => Ok(response)
        };
    }

    /// <summary>
    /// Confirm a payment transaction
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/payment/confirm
    ///     {
    ///         "reference": "244543",
    ///         "amount_subunit": 20000,
    ///         "currency": "KES",
    ///         "sender_phone_number": "254727123123",
    ///         "provider_tx": "WG53SJ8284",
    ///         "provider_key": "ke_safaricom_mpesa",
    ///         "momoep_id": "25346",
    ///         "business_account": "544544"
    ///     }
    /// </remarks>
    /// <param name="request">Confirmation request</param>
    /// <returns>Confirmation response</returns>
    /// <response code="200">Confirmation successful</response>
    /// <response code="409">Duplicate transaction</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ConfirmationResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm([FromBody] ConfirmationRequestDto request)
    {
        _logger.LogInformation("Payment confirmation request for reference {Reference}, provider_tx {ProviderTx}",
            request.Reference, request.ProviderTx);

        var response = await _paymentService.ConfirmAsync(request);

        return response.ErrorCode switch
        {
            "duplicate" => Conflict(response),
            _ when response.Status == "error" => BadRequest(response),
            _ => Ok(response)
        };
    }
}
