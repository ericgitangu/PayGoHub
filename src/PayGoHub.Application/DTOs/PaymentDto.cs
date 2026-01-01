namespace PayGoHub.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string CustomerInitials { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? MpesaReceiptNumber { get; set; }
    public string? ProviderKey { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TimeAgo { get; set; }
}

public class CreatePaymentDto
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Method { get; set; } = "Mpesa";
    public string? Reference { get; set; }
    public string? MpesaReceiptNumber { get; set; }
    public string? ProviderKey { get; set; }
}
