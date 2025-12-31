namespace PayGoHub.Domain.Entities;

/// <summary>
/// API client for authentication - stores API keys and access permissions
/// </summary>
public class ApiClient : BaseEntity
{
    /// <summary>Human-readable name for the API client (e.g., "MoMo Gateway")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>SHA256 hash of the API key (never store plaintext)</summary>
    public string ApiKeyHash { get; set; } = string.Empty;

    /// <summary>Whether this client is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Scopes this client can access (e.g., "momo:validate", "momo:confirm", "m2m:command")</summary>
    public string[] AllowedScopes { get; set; } = Array.Empty<string>();

    /// <summary>Provider keys this client can access (e.g., "ke_safaricom_mpesa")</summary>
    public string[] AllowedProviders { get; set; } = Array.Empty<string>();

    /// <summary>Last time this API key was used</summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>Rate limit for requests per minute</summary>
    public int RateLimitPerMinute { get; set; } = 100;

    /// <summary>Comma-separated IP addresses or CIDR ranges allowed (null = all IPs allowed)</summary>
    public string? IpWhitelist { get; set; }

    // Navigation properties
    public virtual ICollection<DeviceCommand> DeviceCommands { get; set; } = new List<DeviceCommand>();
    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
