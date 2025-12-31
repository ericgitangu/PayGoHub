using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.M2M;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers.Api;

/// <summary>
/// M2M device command API endpoints
/// </summary>
[ApiController]
[Route("api/m2m")]
[Produces("application/json")]
public class M2MApiController : ControllerBase
{
    private readonly IM2MCommandService _commandService;
    private readonly ILogger<M2MApiController> _logger;

    public M2MApiController(IM2MCommandService commandService, ILogger<M2MApiController> logger)
    {
        _commandService = commandService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new device command
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/m2m/command
    ///     {
    ///         "identifier": {
    ///             "kind": "serial",
    ///             "value": "SCBLNX/A/BT/240300126005"
    ///         },
    ///         "command": {
    ///             "name": "unlock_token",
    ///             "details": {
    ///                 "unlock_code": "1234-5678-9012-3456"
    ///             }
    ///         },
    ///         "callback_url": "https://example.com/callback"
    ///     }
    /// </remarks>
    /// <param name="request">Command request</param>
    /// <returns>Command response with status</returns>
    /// <response code="200">Command created successfully</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("command")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCommand([FromBody] CommandRequestDto request)
    {
        _logger.LogInformation("M2M command request: {CommandName} for device {DeviceIdentifier}",
            request.Command.Name, request.Identifier.Value);

        var response = await _commandService.CreateCommandAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Get command status by device identifier
    /// </summary>
    /// <param name="identifier">Device identifier (serial or IMEI)</param>
    /// <returns>Latest command status for device</returns>
    /// <response code="200">Command found</response>
    /// <response code="404">No command found for device</response>
    [HttpGet("command/{identifier}")]
    [ProducesResponseType(typeof(CommandResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommandStatus(string identifier)
    {
        _logger.LogInformation("Getting command status for device {Identifier}", identifier);

        var response = await _commandService.GetCommandStatusAsync(identifier);

        if (response == null)
            return NotFound(new { error = "No command found for device" });

        return Ok(response);
    }

    /// <summary>
    /// Handle command callback from device
    /// </summary>
    /// <remarks>
    /// This endpoint receives callbacks when a device command status changes.
    /// </remarks>
    /// <param name="callback">Callback payload</param>
    /// <returns>Acknowledgement</returns>
    /// <response code="200">Callback processed</response>
    [HttpPost("callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HandleCallback([FromBody] CallbackDto callback)
    {
        _logger.LogInformation("M2M callback received for device {Identifier} with status {Status}",
            callback.Identifier, callback.Status);

        await _commandService.ProcessCallbackAsync(callback);

        return Ok(new { status = "ok" });
    }
}
