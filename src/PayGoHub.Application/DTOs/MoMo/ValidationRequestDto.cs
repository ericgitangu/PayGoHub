using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.MoMo;

/// <summary>
/// MoMo payment validation request from MOMOEP
/// </summary>
public class ValidationRequestDto
{
    /// <summary>Unique identifier for the account (payment/meter number)</summary>
    [Required]
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    /// <summary>ISO 4217 currency code</summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "KES";

    /// <summary>Unique identifier for the business account (paybill/short code)</summary>
    [Required]
    [JsonPropertyName("business_account")]
    public string BusinessAccount { get; set; } = string.Empty;

    /// <summary>MoMo provider key (e.g., "ke_safaricom_mpesa")</summary>
    [Required]
    [JsonPropertyName("provider_key")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Amount in subunits (cents)</summary>
    [JsonPropertyName("amount_subunit")]
    public long? AmountSubunit { get; set; }

    /// <summary>Additional fields to return (e.g., ["customer_name"])</summary>
    [JsonPropertyName("additional_fields")]
    public string[]? AdditionalFields { get; set; }
}
