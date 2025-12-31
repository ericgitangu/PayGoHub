using PayGoHub.Domain.Enums;

namespace PayGoHub.Domain.Entities;

public class Loan : BaseEntity
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Pending;
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal RemainingBalance { get; set; }
    public string Currency { get; set; } = "KES";
    public string? Notes { get; set; }

    // Navigation property
    public virtual Customer Customer { get; set; } = null!;
}
