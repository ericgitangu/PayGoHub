using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Services;

namespace PayGoHub.Tests.Unit;

public class DashboardServiceTests : IDisposable
{
    private readonly PayGoHubDbContext _context;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayGoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new PayGoHubDbContext(options);
        _service = new DashboardService(_context);
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithNoData_ReturnsEmptyDashboard()
    {
        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(0);
        result.NewCustomers.Should().Be(0);
        result.ActiveLoans.Should().Be(0);
        result.Installations.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithPayments_CalculatesTotalRevenue()
    {
        // Arrange
        var customer = CreateCustomer();
        await _context.Customers.AddAsync(customer);

        var payments = new List<Payment>
        {
            CreatePayment(customer.Id, 1000, PaymentStatus.Completed),
            CreatePayment(customer.Id, 2000, PaymentStatus.Completed),
            CreatePayment(customer.Id, 500, PaymentStatus.Pending) // Should not be counted
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.TotalRevenue.Should().Be(3000);
    }

    [Fact]
    public async Task GetDashboardDataAsync_WithLoans_CountsActiveLoans()
    {
        // Arrange
        var customer = CreateCustomer();
        await _context.Customers.AddAsync(customer);

        var loans = new List<Loan>
        {
            CreateLoan(customer.Id, LoanStatus.Active),
            CreateLoan(customer.Id, LoanStatus.Active),
            CreateLoan(customer.Id, LoanStatus.PaidOff) // Should not be counted
        };
        await _context.Loans.AddRangeAsync(loans);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.ActiveLoans.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsRegionalSales()
    {
        // Arrange
        var customer1 = CreateCustomer("Nairobi");
        var customer2 = CreateCustomer("Mombasa");
        await _context.Customers.AddRangeAsync(customer1, customer2);

        var payments = new List<Payment>
        {
            CreatePayment(customer1.Id, 5000, PaymentStatus.Completed),
            CreatePayment(customer1.Id, 3000, PaymentStatus.Completed),
            CreatePayment(customer2.Id, 2000, PaymentStatus.Completed)
        };
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.RegionalSales.Should().NotBeEmpty();
        result.RegionalSales.Should().Contain(r => r.Region == "Nairobi" && r.Amount == 8000);
        result.RegionalSales.Should().Contain(r => r.Region == "Mombasa" && r.Amount == 2000);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsRecentPayments()
    {
        // Arrange
        var customer = CreateCustomer();
        await _context.Customers.AddAsync(customer);

        var payments = new List<Payment>();
        for (int i = 0; i < 10; i++)
        {
            payments.Add(CreatePayment(customer.Id, 100 * i, PaymentStatus.Completed));
        }
        await _context.Payments.AddRangeAsync(payments);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.RecentPayments.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetDashboardDataAsync_ReturnsPendingInstallations()
    {
        // Arrange
        var customer = CreateCustomer();
        var device = CreateDevice();
        await _context.Customers.AddAsync(customer);
        await _context.Devices.AddAsync(device);

        var installations = new List<Installation>
        {
            CreateInstallation(customer.Id, device.Id, InstallationStatus.Pending),
            CreateInstallation(customer.Id, device.Id, InstallationStatus.Scheduled),
            CreateInstallation(customer.Id, device.Id, InstallationStatus.Completed) // Should not be in pending list
        };
        await _context.Installations.AddRangeAsync(installations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetDashboardDataAsync();

        // Assert
        result.PendingInstallations.Should().HaveCount(2);
    }

    private static Customer CreateCustomer(string region = "Nairobi")
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Customer",
            Email = $"test{Guid.NewGuid()}@example.com",
            PhoneNumber = "+254700000000",
            Region = region,
            District = "Westlands",
            Address = "123 Test Street",
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Payment CreatePayment(Guid customerId, decimal amount, PaymentStatus status)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = amount,
            Currency = "KES",
            Status = status,
            Method = PaymentMethod.Mpesa,
            TransactionReference = Guid.NewGuid().ToString(),
            PaidAt = status == PaymentStatus.Completed ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Loan CreateLoan(Guid customerId, LoanStatus status)
    {
        return new Loan
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 10000,
            InterestRate = 15,
            Status = status,
            IssuedDate = DateTime.UtcNow.AddMonths(-1),
            DueDate = DateTime.UtcNow.AddMonths(11),
            RemainingBalance = 10000,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Device CreateDevice()
    {
        return new Device
        {
            Id = Guid.NewGuid(),
            SerialNumber = $"SN-{Guid.NewGuid().ToString()[..8]}",
            Model = "SHS-120W",
            Status = DeviceStatus.Active,
            BatteryHealth = 100,
            LastSyncDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Installation CreateInstallation(Guid customerId, Guid deviceId, InstallationStatus status)
    {
        return new Installation
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            DeviceId = deviceId,
            SystemType = "SHS-120W",
            Status = status,
            ScheduledDate = DateTime.UtcNow.AddDays(1),
            Location = "Test Location",
            TechnicianName = "Test Technician",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
