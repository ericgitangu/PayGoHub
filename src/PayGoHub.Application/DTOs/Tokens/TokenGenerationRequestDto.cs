using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.Tokens;

/// <summary>
/// Token generation request for Moto API
/// </summary>
public class TokenGenerationRequestDto
{
    /// <summary>Device serial/identifier</summary>
    [Required]
    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;

    /// <summary>Command type (unlock_relative, unlock_absolute, counter_sync)</summary>
    [Required]
    [JsonPropertyName("command")]
    public string Command { get; set; } = "unlock_relative";

    /// <summary>Command payload (e.g., number of days)</summary>
    [Required]
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    /// <summary>Sequence number for anti-replay</summary>
    [Required]
    [JsonPropertyName("sequence_number")]
    public int SequenceNumber { get; set; }

    /// <summary>Device secret key (hex)</summary>
    [Required]
    [JsonPropertyName("secret")]
    public string Secret { get; set; } = string.Empty;

    /// <summary>Output encoding format</summary>
    [JsonPropertyName("encoding")]
    public string? Encoding { get; set; }
}
