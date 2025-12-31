using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.MoMo;

/// <summary>
/// MoMo payment validation response
/// </summary>
public class ValidationResponseDto
{
    /// <summary>Validation status: "ok" or "error"</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "ok";

    /// <summary>Customer name if requested in additional_fields</summary>
    [JsonPropertyName("customer_name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CustomerName { get; set; }

    /// <summary>Error code if validation failed</summary>
    [JsonPropertyName("error")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Error { get; set; }

    /// <summary>Human-readable error message</summary>
    [JsonPropertyName("error_message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ErrorMessage { get; set; }
}
