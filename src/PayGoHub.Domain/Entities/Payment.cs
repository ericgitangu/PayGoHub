using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; } = PaymentMethod.Mpesa;
    public string? TransactionReference { get; set; }
    public string? MpesaReceiptNumber { get; set; }
    public string? ProviderKey { get; set; }
    public DateTime? PaidAt { get; set; }

    // Navigation property
    public virtual Customer Customer { get; set; } = null!;
}
