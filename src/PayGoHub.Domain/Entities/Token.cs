using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

/// <summary>
/// Generated PAYG token for device unlock
/// </summary>
public class Token : BaseEntity
{
    /// <summary>Device serial number/identifier</summary>
    public string DeviceIdentifier { get; set; } = string.Empty;

    /// <summary>Type of token (stateless or stateful)</summary>
    public TokenType Type { get; set; } = TokenType.Stateful;

    /// <summary>Generated token value for keypad entry</summary>
    public string TokenValue { get; set; } = string.Empty;

    /// <summary>Command used to generate token (e.g., "unlock_relative")</summary>
    public string Command { get; set; } = "unlock_relative";

    /// <summary>Payload value (e.g., days of credit)</summary>
    public string? Payload { get; set; }

    /// <summary>Sequence number for token replay prevention</summary>
    public int SequenceNumber { get; set; }

    /// <summary>Token encoding format used</summary>
    public string? Encoding { get; set; }

    /// <summary>Number of days credit added</summary>
    public int? DaysCredit { get; set; }

    /// <summary>When the token becomes valid</summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>When the token expires</summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>Whether the token has been used on device</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Timestamp when token was used</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>Associated payment ID if generated for a payment</summary>
    public Guid? PaymentId { get; set; }

    /// <summary>API client that generated this token</summary>
    public Guid ApiClientId { get; set; }
    public virtual ApiClient ApiClient { get; set; } = null!;
}
