namespace PayGoHub.Application.DTOs;

public class DashboardViewModel
{
    // KPIs
    public decimal TotalRevenue { get; set; }
    public decimal RevenueGrowth { get; set; }
    public int NewCustomers { get; set; }
    public decimal CustomerGrowth { get; set; }
    public int ActiveLoans { get; set; }
    public decimal LoanGrowth { get; set; }
    public int Installations { get; set; }
    public decimal InstallationGrowth { get; set; }

    // Lists
    public List<RegionalSalesDto> RegionalSales { get; set; } = new();
    public List<PaymentDto> RecentPayments { get; set; } = new();
    public List<InstallationDto> PendingInstallations { get; set; } = new();
    public List<ActivityDto> RecentActivity { get; set; } = new();

    // Revenue Chart Data
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Payments { get; set; }
}

public class RegionalSalesDto
{
    public string Region { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string ColorClass { get; set; } = "primary";
}

public class PaymentDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerInitials { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class InstallationDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerInitials { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string SystemType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusClass { get; set; } = string.Empty;
    public DateTime? ScheduledDate { get; set; }
    public string ScheduledDateFormatted { get; set; } = string.Empty;
}

public class ActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string ColorClass { get; set; } = string.Empty;
}
