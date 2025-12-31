using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.MoMo;

/// <summary>
/// Payment confirmation response
/// </summary>
public class ConfirmationResponseDto
{
    /// <summary>Status of confirmation</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "ok";

    /// <summary>Error message if confirmation failed</summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>Error code for specific failures</summary>
    [JsonPropertyName("error_code")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorCode { get; set; }
}
