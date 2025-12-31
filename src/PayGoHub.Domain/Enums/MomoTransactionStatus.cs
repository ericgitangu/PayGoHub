namespace PayGoHub.Domain.Enums;

/// <summary>
/// Status of a MoMo payment transaction through the validation/confirmation lifecycle
/// </summary>
public enum MomoTransactionStatus
{
    /// <summary>Transaction created, awaiting validation</summary>
    Pending,

    /// <summary>Customer account validated successfully</summary>
    Validated,

    /// <summary>Validation failed (reference not found, amount issues, etc.)</summary>
    ValidationFailed,

    /// <summary>Payment confirmed by MoMo provider</summary>
    Confirmed,

    /// <summary>Confirmation failed</summary>
    ConfirmationFailed,

    /// <summary>Duplicate transaction detected</summary>
    Duplicate
}
