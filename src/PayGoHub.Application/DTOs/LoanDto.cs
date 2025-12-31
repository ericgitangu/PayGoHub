namespace PayGoHub.Application.DTOs;

public class LoanDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal RemainingBalance { get; set; }
    public string? Notes { get; set; }
}

public class CreateLoanDto
{
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public decimal InterestRate { get; set; }
    public int DurationMonths { get; set; } = 12;
    public string? Notes { get; set; }
}
