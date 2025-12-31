using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

/// <summary>
/// MoMo payment transaction tracking through validation and confirmation phases
/// </summary>
public class MomoPaymentTransaction : BaseEntity
{
    /// <summary>Unique identifier for the account (payment number/meter number)</summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>Amount in subunits (cents - multiplied by 100)</summary>
    public long AmountSubunit { get; set; }

    /// <summary>ISO 4217 currency code</summary>
    public string Currency { get; set; } = "KES";

    /// <summary>Business account customers are paying to (paybill/short code)</summary>
    public string BusinessAccount { get; set; } = string.Empty;

    /// <summary>MoMo provider key (e.g., "ke_safaricom_mpesa")</summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>Phone number of payment sender (E.164 format without +)</summary>
    public string? SenderPhoneNumber { get; set; }

    /// <summary>Unique transaction ID from the payment provider</summary>
    public string? ProviderTx { get; set; }

    /// <summary>Database ID from MOMOEP service</summary>
    public string? MomoepId { get; set; }

    /// <summary>Current transaction status</summary>
    public MomoTransactionStatus Status { get; set; } = MomoTransactionStatus.Pending;

    /// <summary>Customer name returned during validation</summary>
    public string? CustomerName { get; set; }

    /// <summary>Error message if validation/confirmation failed</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Timestamp when validation was performed</summary>
    public DateTime? ValidatedAt { get; set; }

    /// <summary>Timestamp when payment was confirmed</summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>Idempotency key for duplicate detection</summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Secondary provider transaction reference</summary>
    public string? SecondaryProviderTx { get; set; }

    /// <summary>Timestamp from the payment provider</summary>
    public DateTime? TransactionAt { get; set; }

    /// <summary>Full name of payment sender from provider</summary>
    public string? SenderName { get; set; }

    /// <summary>Type of transaction (e.g., "Paybill", "Merchant Payment")</summary>
    public string? TransactionKind { get; set; }
}
