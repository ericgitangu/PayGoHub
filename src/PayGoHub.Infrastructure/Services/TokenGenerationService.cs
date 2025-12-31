using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayGoHub.Application.DTOs.Tokens;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

/// <summary>
/// Token generation service for PAYG devices (Moto API implementation)
/// </summary>
public class TokenGenerationService : ITokenGenerationService
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly ILogger<TokenGenerationService> _logger;

    public TokenGenerationService(PayGoHubDbContext dbContext, ILogger<TokenGenerationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TokenGenerationResponseDto> GenerateStatelessAsync(TokenGenerationRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating stateless token for device {Device} with command {Command}",
            request.Device, request.Command);

        try
        {
            // Validate secret is provided
            if (string.IsNullOrEmpty(request.Secret))
            {
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = "Device secret is required for stateless token generation",
                    SequenceNumber = request.SequenceNumber
                };
            }

            // Generate token using provided secret and sequence
            var tokenValue = GenerateToken(
                request.Device,
                request.Secret,
                request.Command,
                request.Payload,
                request.SequenceNumber,
                request.Encoding);

            _logger.LogInformation("Stateless token generated for device {Device}, sequence {Sequence}",
                request.Device, request.SequenceNumber);

            return new TokenGenerationResponseDto
            {
                Status = "ok",
                Token = tokenValue,
                SequenceNumber = request.SequenceNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate stateless token for device {Device}", request.Device);
            return new TokenGenerationResponseDto
            {
                Status = "error",
                Error = ex.Message,
                SequenceNumber = request.SequenceNumber
            };
        }
    }

    public async Task<TokenGenerationResponseDto> GenerateAsync(TokenGenerationRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating stateful token for device {Device} with command {Command}",
            request.Device, request.Command);

        try
        {
            // For stateful generation, look up device secret from database
            var device = await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.SerialNumber == request.Device, cancellationToken);

            if (device == null)
            {
                _logger.LogWarning("Device {Device} not found in database", request.Device);
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = "Device not found",
                    SequenceNumber = request.SequenceNumber
                };
            }

            // Use provided secret or get from device config (in production)
            var secret = !string.IsNullOrEmpty(request.Secret) ? request.Secret : GetDeviceSecret(device);

            if (string.IsNullOrEmpty(secret))
            {
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = "Device secret not configured",
                    SequenceNumber = request.SequenceNumber
                };
            }

            // Get next sequence number if not provided
            var sequenceNumber = request.SequenceNumber > 0
                ? request.SequenceNumber
                : await GetNextSequenceNumberAsync(request.Device, cancellationToken);

            // Generate token
            var tokenValue = GenerateToken(
                request.Device,
                secret,
                request.Command,
                request.Payload,
                sequenceNumber,
                request.Encoding);

            // Parse payload for days credit
            int? daysCredit = null;
            if (request.Command == "unlock_relative" && int.TryParse(request.Payload, out var days))
            {
                daysCredit = days;
            }

            // Get current API client ID
            var apiClientId = GetCurrentApiClientId();

            // Store token record
            var token = new Token
            {
                DeviceIdentifier = request.Device,
                Type = TokenType.Stateful,
                TokenValue = tokenValue,
                Command = request.Command,
                Payload = request.Payload,
                SequenceNumber = sequenceNumber,
                Encoding = request.Encoding,
                DaysCredit = daysCredit,
                ValidFrom = DateTime.UtcNow,
                ValidUntil = daysCredit.HasValue ? DateTime.UtcNow.AddDays(daysCredit.Value + 30) : null,
                ApiClientId = apiClientId
            };

            _dbContext.Tokens.Add(token);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Stateful token generated for device {Device}, sequence {Sequence}, token ID {TokenId}",
                request.Device, sequenceNumber, token.Id);

            return new TokenGenerationResponseDto
            {
                Status = "ok",
                Token = tokenValue,
                SequenceNumber = sequenceNumber
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate stateful token for device {Device}", request.Device);
            return new TokenGenerationResponseDto
            {
                Status = "error",
                Error = ex.Message,
                SequenceNumber = request.SequenceNumber
            };
        }
    }

    /// <summary>
    /// Generate PAYG token using HMAC-based algorithm
    /// </summary>
    private string GenerateToken(
        string device,
        string secretHex,
        string command,
        string? payload,
        int sequenceNumber,
        string? encoding)
    {
        // Convert hex secret to bytes
        var secretBytes = ConvertHexToBytes(secretHex);

        // Build message: device|command|payload|sequence
        var message = $"{device}|{command}|{payload ?? ""}|{sequenceNumber}";
        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Generate HMAC-SHA256
        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(messageBytes);

        // Take first 8 bytes and convert to numeric token
        var tokenLong = BitConverter.ToUInt64(hash.Take(8).ToArray(), 0);

        // Format based on encoding
        return encoding?.ToLowerInvariant() switch
        {
            "hex" => tokenLong.ToString("X16"),
            "base32" => ToBase32(hash.Take(10).ToArray()),
            _ => FormatNumericToken(tokenLong)
        };
    }

    /// <summary>
    /// Format as numeric token with dashes for readability (e.g., 1234-5678-9012-3456)
    /// </summary>
    private static string FormatNumericToken(ulong value)
    {
        // Take modulo to get 16-digit number
        var numericValue = value % 10000000000000000UL;
        var tokenStr = numericValue.ToString("D16");

        // Insert dashes every 4 digits
        return $"{tokenStr[0..4]}-{tokenStr[4..8]}-{tokenStr[8..12]}-{tokenStr[12..16]}";
    }

    private static string ToBase32(byte[] bytes)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new StringBuilder();
        var bits = 0;
        var value = 0;

        foreach (var b in bytes)
        {
            value = (value << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                result.Append(alphabet[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }

        if (bits > 0)
        {
            result.Append(alphabet[(value << (5 - bits)) & 31]);
        }

        return result.ToString();
    }

    private static byte[] ConvertHexToBytes(string hex)
    {
        // Remove any dashes or spaces
        hex = hex.Replace("-", "").Replace(" ", "");

        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string must have even length");

        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    private async Task<int> GetNextSequenceNumberAsync(string deviceIdentifier, CancellationToken cancellationToken)
    {
        var lastToken = await _dbContext.Tokens
            .Where(t => t.DeviceIdentifier == deviceIdentifier)
            .OrderByDescending(t => t.SequenceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return (lastToken?.SequenceNumber ?? 0) + 1;
    }

    private string? GetDeviceSecret(Device device)
    {
        // In production, device secrets would be stored securely
        // This could be in a separate secure storage or encrypted in DB
        // For now, return null to require explicit secret in request
        return null;
    }

    private Guid GetCurrentApiClientId()
    {
        // In production, this would be retrieved from HttpContext set by auth middleware
        return Guid.Empty;
    }
}
