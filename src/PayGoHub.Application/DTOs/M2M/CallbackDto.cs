using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.M2M;

/// <summary>
/// M2M callback payload sent to upstream service
/// </summary>
public class CallbackDto
{
    /// <summary>Device identifier</summary>
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>Formatted command string</summary>
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    /// <summary>Command status</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Device response</summary>
    [JsonPropertyName("device_response")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceResponse { get; set; }

    /// <summary>When command was created</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>When command was delivered</summary>
    [JsonPropertyName("delivered_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DeliveredAt { get; set; }
}
