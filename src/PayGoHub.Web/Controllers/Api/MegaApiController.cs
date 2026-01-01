using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.Mega;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers.Api;

/// <summary>
/// Mega SMS Gateway API endpoints
/// Matches Mega Rails API: /send_short_message
/// Used for token delivery and customer notifications via SMS/USSD
/// </summary>
[ApiController]
[Route("")]
[Produces("application/json")]
public class MegaApiController : ControllerBase
{
    private readonly IMegaSmsService _smsService;
    private readonly ILogger<MegaApiController> _logger;

    public MegaApiController(IMegaSmsService smsService, ILogger<MegaApiController> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    /// <summary>
    /// Send an SMS message (matches Mega Rails API)
    /// </summary>
    /// <remarks>
    /// Sample request (Mega Rails API format):
    ///
    ///     POST /send_short_message
    ///     {
    ///         "text": "Your token is: 1234-5678-9012-3456",
    ///         "recipient": "254700123456",
    ///         "sender": "SOLARIUM",
    ///         "instance_sms_id": 12345,
    ///         "category": "token-delivery"
    ///     }
    ///
    /// Response codes:
    /// - 200: SMS queued successfully
    /// - 303: Duplicate SMS (already queued with same instance_sms_id)
    /// - 400: Invalid request (missing fields)
    /// - 401: API key invalid
    /// </remarks>
    /// <param name="request">SMS request</param>
    /// <returns>SMS response with mega_sms_id</returns>
    [HttpPost("send_short_message")]
    [HttpPost("api/sms/send")]
    [ProducesResponseType(typeof(SmsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SmsResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendShortMessage([FromBody] SmsRequestDto request)
    {
        _logger.LogInformation("SMS request received for recipient {Recipient}, instance_sms_id {InstanceSmsId}",
            request.Recipient, request.InstanceSmsId);

        var response = await _smsService.SendSmsAsync(request);

        return response.Status switch
        {
            200 => Ok(response),
            303 => StatusCode(303, response), // See Other - duplicate
            400 => BadRequest(response),
            _ => StatusCode(response.Status, response)
        };
    }

    /// <summary>
    /// Get SMS status by Mega SMS ID
    /// </summary>
    /// <param name="megaSmsId">Mega SMS ID returned from send</param>
    /// <returns>SMS status</returns>
    [HttpGet("sms/{megaSmsId:int}/status")]
    [HttpGet("api/sms/{megaSmsId:int}/status")]
    [ProducesResponseType(typeof(SmsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSmsStatus(int megaSmsId)
    {
        _logger.LogInformation("Getting status for mega_sms_id {MegaSmsId}", megaSmsId);

        var response = await _smsService.GetSmsStatusAsync(megaSmsId);

        if (response == null)
            return NotFound(new { error = "SMS not found" });

        return Ok(response);
    }

    /// <summary>
    /// Process delivery report callback (DLR)
    /// </summary>
    /// <remarks>
    /// Called by SMS provider when delivery status changes.
    ///
    /// Sample request:
    ///
    ///     POST /dlr/callback
    ///     {
    ///         "mega_sms_id": 12345,
    ///         "instance_sms_id": 54321,
    ///         "status": "delivered",
    ///         "delivered_at": "2025-12-31T10:00:00Z"
    ///     }
    /// </remarks>
    /// <param name="report">Delivery report</param>
    /// <returns>Acknowledgement</returns>
    [HttpPost("dlr/callback")]
    [HttpPost("api/dlr/callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeliveryReportCallback([FromBody] DeliveryReportDto report)
    {
        _logger.LogInformation("DLR callback received for mega_sms_id {MegaSmsId}, status {Status}",
            report.MegaSmsId, report.Status);

        await _smsService.ProcessDeliveryReportAsync(report);

        return Ok(new { status = "ok" });
    }
}
