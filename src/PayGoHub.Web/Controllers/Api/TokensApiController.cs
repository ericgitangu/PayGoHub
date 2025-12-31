using Microsoft.AspNetCore.Mvc;
using PayGoHub.Application.DTOs.Tokens;
using PayGoHub.Application.Interfaces;

namespace PayGoHub.Web.Controllers.Api;

/// <summary>
/// Token generation API endpoints (Moto API)
/// </summary>
[ApiController]
[Route("api/tokens")]
[Produces("application/json")]
public class TokensApiController : ControllerBase
{
    private readonly ITokenGenerationService _tokenService;
    private readonly ILogger<TokensApiController> _logger;

    public TokensApiController(ITokenGenerationService tokenService, ILogger<TokensApiController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Generate a stateless token (no server-side tracking)
    /// </summary>
    /// <remarks>
    /// Generates a PAYG token using the provided device secret without storing state.
    /// Requires device secret to be provided in the request.
    ///
    /// Sample request:
    ///
    ///     POST /api/tokens/stateless/generate
    ///     {
    ///         "device": "SCBLNX/A/BT/240300126005",
    ///         "command": "unlock_relative",
    ///         "payload": "30",
    ///         "sequence_number": 42,
    ///         "secret": "0123456789ABCDEF0123456789ABCDEF",
    ///         "encoding": "numeric"
    ///     }
    /// </remarks>
    /// <param name="request">Token generation request</param>
    /// <returns>Generated token</returns>
    /// <response code="200">Token generated successfully</response>
    /// <response code="400">Invalid request or missing secret</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("stateless/generate")]
    [ProducesResponseType(typeof(TokenGenerationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TokenGenerationResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateStateless([FromBody] TokenGenerationRequestDto request)
    {
        _logger.LogInformation("Stateless token generation request for device {Device}, command {Command}",
            request.Device, request.Command);

        var response = await _tokenService.GenerateStatelessAsync(request);

        if (response.Status == "error")
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Generate a stateful token (with server-side tracking)
    /// </summary>
    /// <remarks>
    /// Generates a PAYG token and stores it in the database for tracking.
    /// If no sequence number is provided, auto-increments based on previous tokens.
    ///
    /// Sample request:
    ///
    ///     POST /api/tokens/generate
    ///     {
    ///         "device": "SCBLNX/A/BT/240300126005",
    ///         "command": "unlock_relative",
    ///         "payload": "30",
    ///         "sequence_number": 0,
    ///         "secret": "0123456789ABCDEF0123456789ABCDEF"
    ///     }
    /// </remarks>
    /// <param name="request">Token generation request</param>
    /// <returns>Generated token with assigned sequence number</returns>
    /// <response code="200">Token generated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Device not found</response>
    /// <response code="401">API key missing or invalid</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(TokenGenerationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TokenGenerationResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(TokenGenerationResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate([FromBody] TokenGenerationRequestDto request)
    {
        _logger.LogInformation("Stateful token generation request for device {Device}, command {Command}",
            request.Device, request.Command);

        var response = await _tokenService.GenerateAsync(request);

        return response.Error switch
        {
            "Device not found" => NotFound(response),
            _ when response.Status == "error" => BadRequest(response),
            _ => Ok(response)
        };
    }
}
