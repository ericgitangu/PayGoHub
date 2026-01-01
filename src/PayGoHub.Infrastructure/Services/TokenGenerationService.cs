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
/// Token generation service implementing LibToken Paygee algorithm (from moto Rails)
/// Ported from: /moto/lib/tokens/libtoken_paygee/generator.rb
/// </summary>
public class TokenGenerationService : ITokenGenerationService
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly ILogger<TokenGenerationService> _logger;

    // LibToken Paygee constants (from moto Rails)
    private const int HMAC_BITS = 19;
    private const int CMD_BITS = 3;
    private const int CMD_EXT = (1 << CMD_BITS) - 1; // 7
    private const int CMD_EXT_BITS = 3;

    // Command definitions from /moto/lib/tokens/libtoken_paygee/commands.rb
    private static readonly Dictionary<string, CommandDefinition> Commands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["unlock_absolute"] = new(0, PayloadValidation.Integer),
        ["lock"] = new(0, PayloadValidation.Zero, 0),
        ["unlock_relative"] = new(1, PayloadValidation.Integer),
        ["unlock_relative_days"] = new(1, PayloadValidation.Integer),
        ["demo_mode"] = new(2, PayloadValidation.Integer, 0),
        ["unlock_forever"] = new(7, PayloadValidation.Absence),
        ["calibrate"] = new(8, PayloadValidation.Integer),
        ["counter_sync"] = new(9, PayloadValidation.Integer),
        ["misc"] = new(10, PayloadValidation.Integer)
    };

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

            // Validate command
            if (!Commands.TryGetValue(request.Command, out var cmdDef))
            {
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = $"Unsupported command '{request.Command}'",
                    SequenceNumber = request.SequenceNumber
                };
            }

            // Parse payload
            var payload = ParsePayload(request.Payload, cmdDef);
            if (payload < 0)
            {
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = $"Invalid payload for command '{request.Command}'",
                    SequenceNumber = request.SequenceNumber
                };
            }

            // Generate token using LibToken Paygee algorithm
            var secretBytes = ConvertHexToBytes(request.Secret);
            var tokenValue = GenerateLibTokenPaygee(
                cmdDef.CommandInt,
                payload,
                request.SequenceNumber,
                secretBytes,
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

            // Validate command
            if (!Commands.TryGetValue(request.Command, out var cmdDef))
            {
                return new TokenGenerationResponseDto
                {
                    Status = "error",
                    Error = $"Command '{request.Command}' is not a valid command",
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

            // Parse payload
            var payload = ParsePayload(request.Payload, cmdDef);

            // Get next sequence number if not provided
            var sequenceNumber = request.SequenceNumber > 0
                ? request.SequenceNumber
                : await GetNextSequenceNumberAsync(request.Device, cancellationToken);

            // Generate token using LibToken Paygee algorithm
            var secretBytes = ConvertHexToBytes(secret);
            var tokenValue = GenerateLibTokenPaygee(
                cmdDef.CommandInt,
                payload,
                sequenceNumber,
                secretBytes,
                request.Encoding);

            // Parse payload for days credit
            int? daysCredit = null;
            if (request.Command.Equals("unlock_relative", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(request.Payload, out var days))
            {
                daysCredit = days;
            }

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
                ApiClientId = Guid.Empty
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
    /// Generate LibToken Paygee token (ported from moto Rails)
    /// Bit structure: payload (up to 32 bits) | cmd (3 bits) | HMAC (19 bits)
    /// Extended: payload | cmd_ext (3 bits) | CMD_EXT (3 bits) | HMAC (19 bits)
    /// </summary>
    private string GenerateLibTokenPaygee(int cmd, long payload, int counter, byte[] secretKey, string? encoding)
    {
        // Pack token data: [cmd, payload, counter] as 3 uint32 little-endian
        var tokenData = new byte[12];
        BitConverter.GetBytes((uint)cmd).CopyTo(tokenData, 0);
        BitConverter.GetBytes((uint)payload).CopyTo(tokenData, 4);
        BitConverter.GetBytes((uint)counter).CopyTo(tokenData, 8);

        // Generate HMAC-SHA1
        using var hmac = new HMACSHA1(secretKey);
        var hash = hmac.ComputeHash(tokenData);

        // Truncate HMAC to 19 bits using RFC 4226 dynamic truncation
        var signature = HmacTruncateHash(hash, 1 << HMAC_BITS);

        // Build token based on command value
        ulong tkn;
        if (cmd < CMD_EXT)
        {
            // Standard format: <payload> <cmd (3 bits)> <HMAC (19 bits)>
            tkn = ((ulong)payload << (CMD_BITS + HMAC_BITS)) |
                  ((ulong)cmd << HMAC_BITS) |
                  (ulong)signature;
        }
        else
        {
            // Extended format: <payload> <cmd-CMD_EXT (3 bits)> <CMD_EXT (3 bits)> <HMAC (19 bits)>
            tkn = ((ulong)payload << (CMD_EXT_BITS + CMD_BITS + HMAC_BITS)) |
                  ((ulong)(cmd - CMD_EXT) << (CMD_BITS + HMAC_BITS)) |
                  ((ulong)CMD_EXT << HMAC_BITS) |
                  (ulong)signature;
        }

        // Encode token
        return EncodeToken(tkn, encoding);
    }

    /// <summary>
    /// RFC 4226 HMAC truncation (ported from moto Rails helper.rb)
    /// </summary>
    private static int HmacTruncateHash(byte[] hash, int trunc)
    {
        var offset = hash[^1] & 0xF;
        var code = ((hash[offset] & 0x7F) << 24) |
                   ((hash[offset + 1] & 0xFF) << 16) |
                   ((hash[offset + 2] & 0xFF) << 8) |
                   (hash[offset + 3] & 0xFF);
        return code % trunc;
    }

    /// <summary>
    /// Encode token with optional formatting
    /// </summary>
    private static string EncodeToken(ulong token, string? encoding)
    {
        if (string.IsNullOrEmpty(encoding))
        {
            // Default: numeric with spaces every 3 digits from right
            var tokenStr = token.ToString();
            return FormatWithSpaces(tokenStr, 3);
        }

        // Parse encoding string format: prefix+low-high+suffix+options
        // Example: "+0-9++space3" means base 10, space every 3 chars
        if (encoding.Contains("+"))
        {
            var parts = encoding.Split('+');
            if (parts.Length >= 2)
            {
                var range = parts[1].Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out var low) && int.TryParse(range[1], out var high))
                {
                    var baseNum = high - low + 1;
                    var result = ConvertToBase(token, baseNum, low);

                    // Apply prefix/suffix
                    var prefix = parts[0];
                    var suffix = parts.Length > 2 ? parts[2] : "";
                    result = prefix + result + suffix;

                    // Check for space option
                    if (parts.Length > 3 && parts[3].StartsWith("space"))
                    {
                        var spaceSize = 3;
                        if (parts[3].Length > 5 && int.TryParse(parts[3][5..], out var size))
                        {
                            spaceSize = size;
                        }
                        result = FormatWithSpaces(result, spaceSize);
                    }

                    return result;
                }
            }
        }

        return token.ToString();
    }

    private static string ConvertToBase(ulong value, int baseNum, int offset)
    {
        if (value == 0) return (offset).ToString();

        var result = new StringBuilder();
        while (value > 0)
        {
            var digit = (int)(value % (ulong)baseNum) + offset;
            result.Insert(0, digit.ToString());
            value /= (ulong)baseNum;
        }
        return result.ToString();
    }

    private static string FormatWithSpaces(string value, int groupSize)
    {
        var reversed = new string(value.Reverse().ToArray());
        var groups = new List<string>();
        for (var i = 0; i < reversed.Length; i += groupSize)
        {
            var chunk = reversed.Substring(i, Math.Min(groupSize, reversed.Length - i));
            groups.Add(new string(chunk.Reverse().ToArray()));
        }
        groups.Reverse();
        return string.Join(" ", groups);
    }

    private static int ParsePayload(string? payload, CommandDefinition cmdDef)
    {
        if (cmdDef.AutoPayload.HasValue)
            return cmdDef.AutoPayload.Value;

        if (string.IsNullOrEmpty(payload))
            return cmdDef.Validation == PayloadValidation.Absence ? 0 : -1;

        if (!int.TryParse(payload, out var value))
            return -1;

        return cmdDef.Validation switch
        {
            PayloadValidation.Zero when value != 0 => -1,
            PayloadValidation.Absence when value != 0 => -1,
            _ => value
        };
    }

    private static byte[] ConvertHexToBytes(string hex)
    {
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
        return null;
    }

    private enum PayloadValidation { Integer, Zero, Absence }

    private record CommandDefinition(int CommandInt, PayloadValidation Validation, int? AutoPayload = null);
}
