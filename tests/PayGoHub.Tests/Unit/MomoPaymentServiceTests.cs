using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;
using PayGoHub.Infrastructure.Services;

namespace PayGoHub.Tests.Unit;

[Trait("Category", "Unit")]
public class MomoPaymentServiceTests
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly MomoPaymentService _service;

    public MomoPaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<PayGoHubDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new PayGoHubDbContext(options);
        var logger = Mock.Of<ILogger<MomoPaymentService>>();
        _service = new MomoPaymentService(_dbContext, logger);
    }

    [Fact]
    public async Task ValidateAsync_WhenProviderNotFound_ReturnsError()
    {
        // Arrange
        var request = new ValidationRequestDto
        {
            Reference = "123456",
            Currency = "KES",
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 20000
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("provider_not_found");
    }

    [Fact]
    public async Task ValidateAsync_WhenCurrencyMismatch_ReturnsError()
    {
        // Arrange
        await _dbContext.Providers.AddAsync(new Provider
        {
            ProviderKey = "ke_safaricom_mpesa",
            DisplayName = "M-Pesa Kenya",
            Country = "KE",
            Currency = "KES",
            IsActive = true
        });
        await _dbContext.SaveChangesAsync();

        var request = new ValidationRequestDto
        {
            Reference = "123456",
            Currency = "USD", // Wrong currency
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 20000
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("currency_mismatch");
    }

    [Fact]
    public async Task ValidateAsync_WhenAmountTooLow_ReturnsError()
    {
        // Arrange
        await _dbContext.Providers.AddAsync(new Provider
        {
            ProviderKey = "ke_safaricom_mpesa",
            DisplayName = "M-Pesa Kenya",
            Country = "KE",
            Currency = "KES",
            IsActive = true,
            MinAmountSubunit = 1000,
            MaxAmountSubunit = 10000000
        });
        await _dbContext.SaveChangesAsync();

        var request = new ValidationRequestDto
        {
            Reference = "123456",
            Currency = "KES",
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 500 // Too low
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("amount_too_low");
    }

    [Fact]
    public async Task ValidateAsync_WhenAmountTooHigh_ReturnsError()
    {
        // Arrange
        await _dbContext.Providers.AddAsync(new Provider
        {
            ProviderKey = "ke_safaricom_mpesa",
            DisplayName = "M-Pesa Kenya",
            Country = "KE",
            Currency = "KES",
            IsActive = true,
            MinAmountSubunit = 100,
            MaxAmountSubunit = 10000
        });
        await _dbContext.SaveChangesAsync();

        var request = new ValidationRequestDto
        {
            Reference = "123456",
            Currency = "KES",
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 50000 // Too high
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("amount_too_high");
    }

    [Fact]
    public async Task ValidateAsync_WhenCustomerNotFound_ReturnsReferenceNotFound()
    {
        // Arrange
        await _dbContext.Providers.AddAsync(new Provider
        {
            ProviderKey = "ke_safaricom_mpesa",
            DisplayName = "M-Pesa Kenya",
            Country = "KE",
            Currency = "KES",
            IsActive = true
        });
        await _dbContext.SaveChangesAsync();

        var request = new ValidationRequestDto
        {
            Reference = "123456",
            Currency = "KES",
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 20000
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.Error.Should().Be("reference_not_found");
    }

    [Fact]
    public async Task ValidateAsync_WhenCustomerFoundByPhone_ReturnsSuccess()
    {
        // Arrange
        await _dbContext.Providers.AddAsync(new Provider
        {
            ProviderKey = "ke_safaricom_mpesa",
            DisplayName = "M-Pesa Kenya",
            Country = "KE",
            Currency = "KES",
            IsActive = true
        });

        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "254727123456",
            Email = "john@example.com"
        };
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var request = new ValidationRequestDto
        {
            Reference = "254727123456",
            Currency = "KES",
            BusinessAccount = "544544",
            ProviderKey = "ke_safaricom_mpesa",
            AmountSubunit = 20000,
            AdditionalFields = new[] { "customer_name" }
        };

        // Act
        var result = await _service.ValidateAsync(request);

        // Assert
        result.Status.Should().Be("ok");
        result.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task ConfirmAsync_WhenDuplicateTransaction_ReturnsConflict()
    {
        // Arrange
        await _dbContext.MomoPaymentTransactions.AddAsync(new MomoPaymentTransaction
        {
            Reference = "123456",
            ProviderTx = "WG53SJ8284",
            MomoepId = "25346",
            IdempotencyKey = "WG53SJ8284:25346",
            Status = MomoTransactionStatus.Confirmed
        });
        await _dbContext.SaveChangesAsync();

        var request = new ConfirmationRequestDto
        {
            Reference = "123456",
            AmountSubunit = 20000,
            Currency = "KES",
            SenderPhoneNumber = "254727123456",
            ProviderTx = "WG53SJ8284",
            MomoepId = "25346",
            ProviderKey = "ke_safaricom_mpesa",
            BusinessAccount = "544544"
        };

        // Act
        var result = await _service.ConfirmAsync(request);

        // Assert
        result.Status.Should().Be("error");
        result.ErrorCode.Should().Be("duplicate");
    }

    [Fact]
    public async Task ConfirmAsync_WhenNewTransaction_ReturnsSuccess()
    {
        // Arrange
        var customer = new Customer
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "254727123456",
            Email = "john@example.com"
        };
        await _dbContext.Customers.AddAsync(customer);
        await _dbContext.SaveChangesAsync();

        var request = new ConfirmationRequestDto
        {
            Reference = "254727123456",
            AmountSubunit = 20000,
            Currency = "KES",
            SenderPhoneNumber = "254727123456",
            ProviderTx = "WG53SJ8284",
            MomoepId = "25346",
            ProviderKey = "ke_safaricom_mpesa",
            BusinessAccount = "544544"
        };

        // Act
        var result = await _service.ConfirmAsync(request);

        // Assert
        result.Status.Should().Be("ok");

        // Verify transaction was recorded
        var transaction = await _dbContext.MomoPaymentTransactions.FirstOrDefaultAsync();
        transaction.Should().NotBeNull();
        transaction!.Status.Should().Be(MomoTransactionStatus.Confirmed);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
