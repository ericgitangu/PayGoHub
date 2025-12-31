using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PayGoHub.Application.DTOs.MoMo;

/// <summary>
/// MoMo payment confirmation request from MOMOEP
/// </summary>
public class ConfirmationRequestDto
{
    /// <summary>Unique identifier for the account (payment/meter number)</summary>
    [Required]
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    /// <summary>Amount in subunits (cents)</summary>
    [Required]
    [JsonPropertyName("amount_subunit")]
    public long AmountSubunit { get; set; }

    /// <summary>ISO 4217 currency code</summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "KES";

    /// <summary>Phone number of payment sender (E.164 without +)</summary>
    [JsonPropertyName("sender_phone_number")]
    public string? SenderPhoneNumber { get; set; }

    /// <summary>Unique transaction ID from payment provider</summary>
    [Required]
    [JsonPropertyName("provider_tx")]
    public string ProviderTx { get; set; } = string.Empty;

    /// <summary>Company name of payment provider</summary>
    [JsonPropertyName("provider_name")]
    public string? ProviderName { get; set; }

    /// <summary>Brand name of payment product</summary>
    [JsonPropertyName("provider_brand")]
    public string? ProviderBrand { get; set; }

    /// <summary>ISO 3166 country code</summary>
    [JsonPropertyName("provider_country")]
    public string? ProviderCountry { get; set; }

    /// <summary>Human-readable provider name</summary>
    [JsonPropertyName("provider_humanized")]
    public string? ProviderHumanized { get; set; }

    /// <summary>MoMo provider key (e.g., "ke_safaricom_mpesa")</summary>
    [Required]
    [JsonPropertyName("provider_key")]
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Time MoMo received the payment (UTC)</summary>
    [JsonPropertyName("received_at")]
    public DateTime? ReceivedAt { get; set; }

    /// <summary>Database ID from MOMOEP</summary>
    [JsonPropertyName("momoep_id")]
    public string? MomoepId { get; set; }

    /// <summary>Business account customers are paying to</summary>
    [Required]
    [JsonPropertyName("business_account")]
    public string BusinessAccount { get; set; } = string.Empty;

    /// <summary>Secondary provider transaction reference</summary>
    [JsonPropertyName("secondary_provider_tx")]
    public string? SecondaryProviderTx { get; set; }

    /// <summary>Timestamp from payment provider (UTC)</summary>
    [JsonPropertyName("transaction_at")]
    public DateTime? TransactionAt { get; set; }

    /// <summary>Full name of payment sender</summary>
    [JsonPropertyName("sender_name")]
    public string? SenderName { get; set; }

    /// <summary>Type of transaction</summary>
    [JsonPropertyName("transaction_kind")]
    public string? TransactionKind { get; set; }
}
