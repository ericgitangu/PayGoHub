using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.M2M;

/// <summary>
/// M2M device command request
/// </summary>
public class CommandRequestDto
{
    /// <summary>Device identifier</summary>
    [Required]
    [JsonPropertyName("identifier")]
    public IdentifierDto Identifier { get; set; } = new();

    /// <summary>Command to execute</summary>
    [Required]
    [JsonPropertyName("command")]
    public CommandDetailDto Command { get; set; } = new();

    /// <summary>URL to call when command status changes</summary>
    [Required]
    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = string.Empty;
}

/// <summary>
/// Device identifier
/// </summary>
public class IdentifierDto
{
    /// <summary>Type of identifier ("serial" or "imei")</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "serial";

    /// <summary>Identifier value</summary>
    [Required]
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Command details
/// </summary>
public class CommandDetailDto
{
    /// <summary>Command name (unlock_token, sync, synciv, reset, useracct)</summary>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Command-specific details</summary>
    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; set; }
}
