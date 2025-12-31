using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.Tokens;

/// <summary>
/// Token generation response
/// </summary>
public class TokenGenerationResponseDto
{
    /// <summary>Status of token generation</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "ok";

    /// <summary>Generated token value</summary>
    [JsonPropertyName("token")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Token { get; set; }

    /// <summary>Sequence number used</summary>
    [JsonPropertyName("sequence_number")]
    public int SequenceNumber { get; set; }

    /// <summary>Error message if generation failed</summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }
}
