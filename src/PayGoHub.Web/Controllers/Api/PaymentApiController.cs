using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers.Api;

/// <summary>
/// MoMo payment API endpoints for validation and confirmation
/// Matches momoep Rails API: /api/v1/:provider_key/validate and /api/v1/:provider_key/confirm
/// </summary>
[ApiController]
[Route("api/v1")]
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
    /// Sample request (momoep Rails API format):
    ///
    ///     POST /api/v1/ke_safaricom_mpesa/validate
    ///     {
    ///         "amount": "300",
    ///         "account_reference": "254543",
    ///         "business_reference": "544544",
    ///         "transaction_reference": "unique_tx_id",
    ///         "subscriber_msisdn": "+254727123123",
    ///         "subscriber_name": "John Doe"
    ///     }
    ///
    /// Legacy format also supported:
    ///
    ///     POST /api/v1/payment/validate
    ///     {
    ///         "reference": "254543",
    ///         "currency": "KES",
    ///         "business_account": "544544",
    ///         "provider_key": "ke_safaricom_mpesa",
    ///         "amount_subunit": 20000,
    ///         "additional_fields": ["customer_name"]
    ///     }
    /// </remarks>
    /// <param name="providerKey">Provider key (e.g., ke_safaricom_mpesa)</param>
    /// <param name="request">Validation request</param>
    /// <returns>Validation response with customer info</returns>
    /// <response code="200">Validation successful (status: "01")</response>
    /// <response code="400">Validation failed (status: "02")</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("{providerKey}/validate")]
    [HttpPost("payment/validate")]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate([FromRoute] string? providerKey, [FromBody] ValidationRequestDto request)
    {
        // Use provider_key from route if present, otherwise from request body
        if (!string.IsNullOrEmpty(providerKey) && string.IsNullOrEmpty(request.ProviderKey))
        {
            request.ProviderKey = providerKey;
        }

        _logger.LogInformation("Payment validation request for reference {Reference}, provider {Provider}",
            request.Reference, request.ProviderKey);

        var response = await _paymentService.ValidateAsync(request);

        // Return momoep-compatible response (status "01" = success, "02" = failure)
        if (response.Status == "error")
        {
            return BadRequest(response);
        }

        return Ok(response);
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
    [HttpPost("{providerKey}/confirm")]
    [HttpPost("payment/confirm")]
    [ProducesResponseType(typeof(ConfirmationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ConfirmationResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm([FromRoute] string? providerKey, [FromBody] ConfirmationRequestDto request)
    {
        // Use provider_key from route if present, otherwise from request body
        if (!string.IsNullOrEmpty(providerKey) && string.IsNullOrEmpty(request.ProviderKey))
        {
            request.ProviderKey = providerKey;
        }

        _logger.LogInformation("Payment confirmation request for reference {Reference}, provider_tx {ProviderTx}, provider {Provider}",
            request.Reference, request.ProviderTx, request.ProviderKey);

        var response = await _paymentService.ConfirmAsync(request);

        return response.ErrorCode switch
        {
            "duplicate" => Conflict(response),
            _ when response.Status == "error" => BadRequest(response),
            _ => Ok(response)
        };
    }
}
