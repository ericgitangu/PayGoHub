using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayGoHub.Application.DTOs.M2M;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

/// <summary>
/// M2M device command service implementation
/// </summary>
public class M2MCommandService : IM2MCommandService
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly ILogger<M2MCommandService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public M2MCommandService(
        PayGoHubDbContext dbContext,
        ILogger<M2MCommandService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<CommandResponseDto> CreateCommandAsync(CommandRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating command {CommandName} for device {DeviceIdentifier}",
            request.Command.Name, request.Identifier.Value);

        // Verify device exists
        var device = await FindDeviceAsync(request.Identifier.Kind, request.Identifier.Value, cancellationToken);

        if (device == null)
        {
            _logger.LogWarning("Device {Identifier} ({Kind}) not found",
                request.Identifier.Value, request.Identifier.Kind);
        }

        // Get current API client from HTTP context (set by middleware)
        var apiClientId = GetCurrentApiClientId();

        // Create command record
        var command = new DeviceCommand
        {
            DeviceIdentifier = request.Identifier.Value,
            IdentifierKind = request.Identifier.Kind,
            CommandName = request.Command.Name,
            CommandDetails = request.Command.Details != null
                ? JsonSerializer.Serialize(request.Command.Details)
                : null,
            CallbackUrl = request.CallbackUrl,
            Status = CommandStatus.Pending,
            ApiClientId = apiClientId
        };

        _dbContext.DeviceCommands.Add(command);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Command {CommandId} created for device {DeviceIdentifier}",
            command.Id, request.Identifier.Value);

        // Format command string for response
        var formattedCommand = FormatCommand(request.Command.Name, request.Command.Details);

        return new CommandResponseDto
        {
            Identifier = request.Identifier.Value,
            Command = formattedCommand,
            Status = "pending",
            CreatedAt = command.CreatedAt
        };
    }

    public async Task<CommandResponseDto?> GetCommandStatusAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var command = await _dbContext.DeviceCommands
            .Where(c => c.DeviceIdentifier == identifier)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (command == null)
            return null;

        var formattedCommand = FormatCommand(command.CommandName, command.CommandDetails);

        return new CommandResponseDto
        {
            CommandId = command.Id.ToString(),
            Identifier = command.DeviceIdentifier,
            Command = formattedCommand,
            Status = command.Status.ToString().ToLowerInvariant(),
            CreatedAt = command.CreatedAt,
            DeliveredAt = command.ExecutedAt,
            DeviceResponse = command.DeviceResponse
        };
    }

    public async Task<IEnumerable<CommandResponseDto>> GetRecentCommandsAsync(string deviceIdentifier, int count = 10, CancellationToken cancellationToken = default)
    {
        var commands = await _dbContext.DeviceCommands
            .Where(c => c.DeviceIdentifier == deviceIdentifier)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return commands.Select(c => new CommandResponseDto
        {
            CommandId = c.Id.ToString(),
            Identifier = c.DeviceIdentifier,
            Command = FormatCommand(c.CommandName, c.CommandDetails),
            Status = c.Status.ToString().ToLowerInvariant(),
            CreatedAt = c.CreatedAt,
            DeliveredAt = c.ExecutedAt,
            DeviceResponse = c.DeviceResponse
        });
    }

    public async Task ProcessCallbackAsync(CallbackDto callback, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing callback for device {Identifier} with status {Status}",
            callback.Identifier, callback.Status);

        // Find the pending/sent command
        var command = await _dbContext.DeviceCommands
            .Where(c => c.DeviceIdentifier == callback.Identifier &&
                       (c.Status == CommandStatus.Pending || c.Status == CommandStatus.Sent))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (command == null)
        {
            _logger.LogWarning("No pending command found for device {Identifier}", callback.Identifier);
            return;
        }

        // Update command status based on callback
        command.DeviceResponse = callback.DeviceResponse;
        command.ExecutedAt = callback.DeliveredAt ?? DateTime.UtcNow;

        command.Status = callback.Status.ToLowerInvariant() switch
        {
            "completed" or "success" or "ok" => CommandStatus.Completed,
            "failed" or "error" => CommandStatus.Failed,
            "acknowledged" => CommandStatus.Acknowledged,
            "timeout" => CommandStatus.TimedOut,
            _ => CommandStatus.Completed
        };

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Send callback to registered URL if configured
        if (!string.IsNullOrEmpty(command.CallbackUrl))
        {
            await SendCallbackAsync(command, cancellationToken);
        }

        _logger.LogInformation("Command {CommandId} updated to status {Status}",
            command.Id, command.Status);
    }

    private async Task<Device?> FindDeviceAsync(string kind, string value, CancellationToken cancellationToken)
    {
        return kind.ToLowerInvariant() switch
        {
            "serial" => await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.SerialNumber == value, cancellationToken),
            "imei" => await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.SerialNumber == value, cancellationToken), // IMEI stored as serial
            _ => null
        };
    }

    private static string FormatCommand(string commandName, object? details)
    {
        if (details == null)
            return commandName;

        var detailsStr = details is string s ? s : JsonSerializer.Serialize(details);
        return $"{commandName}:{detailsStr}";
    }

    private Guid GetCurrentApiClientId()
    {
        // In production, this would be retrieved from HttpContext set by auth middleware
        // For now, return a default value that should be replaced
        return Guid.Empty;
    }

    private async Task SendCallbackAsync(DeviceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("M2MCallback");

            var callbackPayload = new CallbackDto
            {
                Identifier = command.DeviceIdentifier,
                Command = FormatCommand(command.CommandName, command.CommandDetails),
                Status = command.Status.ToString().ToLowerInvariant(),
                DeviceResponse = command.DeviceResponse,
                CreatedAt = command.CreatedAt,
                DeliveredAt = command.ExecutedAt
            };

            var response = await client.PostAsJsonAsync(command.CallbackUrl, callbackPayload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                command.CallbackDeliveredAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Callback delivered to {Url} for command {CommandId}",
                    command.CallbackUrl, command.Id);
            }
            else
            {
                _logger.LogWarning("Callback to {Url} failed with status {StatusCode}",
                    command.CallbackUrl, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send callback to {Url} for command {CommandId}",
                command.CallbackUrl, command.Id);
        }
    }
}
