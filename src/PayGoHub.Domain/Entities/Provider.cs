namespace PayGoHub.Domain.Entities;

/// <summary>
/// Mobile money provider configuration (e.g., ke_safaricom_mpesa, ug_mtn_mobilemoney)
/// </summary>
public class Provider : BaseEntity
{
    /// <summary>Unique provider key in format: countryiso3166code_companyname_brandname</summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Human-readable display name</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>ISO 3166 country code (e.g., "KE", "UG", "TZ")</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>ISO 4217 currency code (e.g., "KES", "UGX", "TZS")</summary>
    public string Currency { get; set; } = "KES";

    /// <summary>Whether this provider is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Provider-specific configuration as JSON</summary>
    public string? ConfigurationJson { get; set; }

    /// <summary>Minimum allowed amount in subunits (cents)</summary>
    public long MinAmountSubunit { get; set; } = 100;

    /// <summary>Maximum allowed amount in subunits (cents)</summary>
    public long MaxAmountSubunit { get; set; } = 100000000; // 1M in major units
}
