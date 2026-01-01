using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.M2M;

/// <summary>
/// M2M device command response
/// </summary>
public class CommandResponseDto
{
    /// <summary>Command ID</summary>
    [JsonPropertyName("command_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? CommandId { get; set; }

    /// <summary>Device identifier</summary>
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>Formatted command string</summary>
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    /// <summary>Command status</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>When command was created</summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>When command was delivered (if applicable)</summary>
    [JsonPropertyName("delivered_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DeliveredAt { get; set; }

    /// <summary>Device response (if applicable)</summary>
    [JsonPropertyName("device_response")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeviceResponse { get; set; }
}
