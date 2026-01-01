using PayGoHub.Application.DTOs.M2M;

namespace PayGoHub.Application.Interfaces;

/// <summary>
/// M2M device command service
/// </summary>
public interface IM2MCommandService
{
    /// <summary>
    /// Create a new device command
    /// </summary>
    Task<CommandResponseDto> CreateCommandAsync(CommandRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get command status by identifier
    /// </summary>
    Task<CommandResponseDto?> GetCommandStatusAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process callback from device
    /// </summary>
    Task ProcessCallbackAsync(CallbackDto callback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent commands for a device
    /// </summary>
    Task<IEnumerable<CommandResponseDto>> GetRecentCommandsAsync(string deviceIdentifier, int count = 10, CancellationToken cancellationToken = default);
}
