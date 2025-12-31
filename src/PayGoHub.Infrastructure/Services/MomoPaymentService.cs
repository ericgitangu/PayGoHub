using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayGoHub.Application.DTOs.MoMo;
using PayGoHub.Application.Interfaces;
using PayGoHub.Domain.Entities;
using PayGoHub.Domain.Enums;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Infrastructure.Services;

/// <summary>
/// MoMo payment validation and confirmation service implementation
/// </summary>
public class MomoPaymentService : IMomoPaymentService
{
    private readonly PayGoHubDbContext _dbContext;
    private readonly ILogger<MomoPaymentService> _logger;

    public MomoPaymentService(PayGoHubDbContext dbContext, ILogger<MomoPaymentService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ValidationResponseDto> ValidateAsync(ValidationRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating payment for reference {Reference} with provider {ProviderKey}",
            request.Reference, request.ProviderKey);

        // Validate provider exists and is active
        var provider = await _dbContext.Providers
            .FirstOrDefaultAsync(p => p.ProviderKey == request.ProviderKey && p.IsActive, cancellationToken);

        if (provider == null)
        {
            _logger.LogWarning("Provider {ProviderKey} not found or inactive", request.ProviderKey);
            return new ValidationResponseDto
            {
                Status = "error",
                Error = "provider_not_found",
                ErrorMessage = $"Provider '{request.ProviderKey}' is not configured or inactive"
            };
        }

        // Validate currency matches provider
        if (!string.Equals(provider.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResponseDto
            {
                Status = "error",
                Error = "currency_mismatch",
                ErrorMessage = $"Currency '{request.Currency}' does not match provider currency '{provider.Currency}'"
            };
        }

        // Validate amount is within provider limits (if amount provided)
        var amount = request.AmountSubunit ?? 0;
        if (request.AmountSubunit.HasValue)
        {
            if (amount < provider.MinAmountSubunit)
            {
                return new ValidationResponseDto
                {
                    Status = "error",
                    Error = "amount_too_low",
                    ErrorMessage = $"Amount {amount} is below minimum {provider.MinAmountSubunit}"
                };
            }

            if (amount > provider.MaxAmountSubunit)
            {
                return new ValidationResponseDto
                {
                    Status = "error",
                    Error = "amount_too_high",
                    ErrorMessage = $"Amount {amount} exceeds maximum {provider.MaxAmountSubunit}"
                };
            }
        }

        // Look up customer by reference - could be device serial, phone number, or ID
        var customer = await FindCustomerByReferenceAsync(request.Reference, cancellationToken);

        if (customer == null)
        {
            _logger.LogWarning("Customer with reference {Reference} not found", request.Reference);

            // Record failed validation attempt
            var failedTransaction = new MomoPaymentTransaction
            {
                Reference = request.Reference,
                AmountSubunit = amount,
                Currency = request.Currency,
                BusinessAccount = request.BusinessAccount,
                ProviderKey = request.ProviderKey,
                Status = MomoTransactionStatus.ValidationFailed,
                ErrorMessage = "reference_not_found",
                ValidatedAt = DateTime.UtcNow
            };
            _dbContext.MomoPaymentTransactions.Add(failedTransaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ValidationResponseDto
            {
                Status = "error",
                Error = "reference_not_found",
                ErrorMessage = "Customer account not found"
            };
        }

        // Create successful validation record
        var transaction = new MomoPaymentTransaction
        {
            Reference = request.Reference,
            AmountSubunit = amount,
            Currency = request.Currency,
            BusinessAccount = request.BusinessAccount,
            ProviderKey = request.ProviderKey,
            Status = MomoTransactionStatus.Validated,
            CustomerName = customer.FullName,
            ValidatedAt = DateTime.UtcNow
        };
        _dbContext.MomoPaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment validation successful for reference {Reference}, customer {CustomerName}",
            request.Reference, customer.FullName);

        // Build response with requested additional fields
        var response = new ValidationResponseDto
        {
            Status = "ok"
        };

        if (request.AdditionalFields?.Contains("customer_name") == true)
        {
            response.CustomerName = customer.FullName;
        }

        return response;
    }

    public async Task<ConfirmationResponseDto> ConfirmAsync(ConfirmationRequestDto request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Confirming payment for reference {Reference} with provider_tx {ProviderTx}",
            request.Reference, request.ProviderTx);

        // Generate idempotency key from provider_tx + momoep_id
        var idempotencyKey = $"{request.ProviderTx}:{request.MomoepId}";

        // Check for duplicate transaction
        var existingTransaction = await _dbContext.MomoPaymentTransactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, cancellationToken);

        if (existingTransaction != null)
        {
            _logger.LogWarning("Duplicate payment detected for idempotency key {IdempotencyKey}", idempotencyKey);
            return new ConfirmationResponseDto
            {
                Status = "error",
                Error = "Duplicate transaction",
                ErrorCode = "duplicate"
            };
        }

        // Find the validated transaction
        var transaction = await _dbContext.MomoPaymentTransactions
            .Where(t => t.Reference == request.Reference && t.Status == MomoTransactionStatus.Validated)
            .OrderByDescending(t => t.ValidatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Find customer to link payment
        var customer = await FindCustomerByReferenceAsync(request.Reference, cancellationToken);

        if (transaction == null)
        {
            _logger.LogWarning("No validated transaction found for reference {Reference}", request.Reference);

            // Create new transaction for confirmation without prior validation
            transaction = new MomoPaymentTransaction
            {
                Reference = request.Reference,
                AmountSubunit = request.AmountSubunit,
                Currency = request.Currency,
                BusinessAccount = request.BusinessAccount,
                ProviderKey = request.ProviderKey,
                CustomerName = customer?.FullName
            };
            _dbContext.MomoPaymentTransactions.Add(transaction);
        }

        // Update transaction with confirmation details
        transaction.ProviderTx = request.ProviderTx;
        transaction.MomoepId = request.MomoepId;
        transaction.SenderPhoneNumber = request.SenderPhoneNumber;
        transaction.IdempotencyKey = idempotencyKey;
        transaction.Status = MomoTransactionStatus.Confirmed;
        transaction.ConfirmedAt = DateTime.UtcNow;

        // Record payment in the Payments table if customer found
        if (customer != null)
        {
            var payment = new Payment
            {
                CustomerId = customer.Id,
                Amount = request.AmountSubunit / 100m, // Convert from subunits
                Currency = request.Currency,
                Status = PaymentStatus.Completed,
                Method = PaymentMethod.Mpesa,
                TransactionReference = request.Reference,
                MpesaReceiptNumber = request.ProviderTx,
                PaidAt = DateTime.UtcNow
            };
            _dbContext.Payments.Add(payment);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment confirmed successfully for reference {Reference}, provider_tx {ProviderTx}",
            request.Reference, request.ProviderTx);

        return new ConfirmationResponseDto
        {
            Status = "ok"
        };
    }

    /// <summary>
    /// Find customer by reference - searches by phone number, device serial, or customer ID
    /// </summary>
    private async Task<Customer?> FindCustomerByReferenceAsync(string reference, CancellationToken cancellationToken)
    {
        // Try phone number lookup first
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == reference, cancellationToken);

        if (customer != null)
            return customer;

        // Try device serial lookup through installation
        var device = await _dbContext.Devices
            .Include(d => d.Installation)
            .ThenInclude(i => i!.Customer)
            .FirstOrDefaultAsync(d => d.SerialNumber == reference, cancellationToken);

        if (device?.Installation?.Customer != null)
            return device.Installation.Customer;

        // Try customer ID as GUID
        if (Guid.TryParse(reference, out var customerId))
        {
            customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
        }

        return customer;
    }
}
