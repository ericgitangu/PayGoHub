using PayGoHub.Application.DTOs.Tokens;

namespace PayGoHub.Application.Interfaces;

/// <summary>
/// Token generation service for PAYG devices
/// </summary>
public interface ITokenGenerationService
{
    /// <summary>
    /// Generate a token statelessly (no database storage)
    /// </summary>
    Task<TokenGenerationResponseDto> GenerateStatelessAsync(TokenGenerationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate a token with state tracking
    /// </summary>
    Task<TokenGenerationResponseDto> GenerateAsync(TokenGenerationRequestDto request, CancellationToken cancellationToken = default);
}
