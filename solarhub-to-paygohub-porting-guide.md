# Solarhub to PayGoHub Porting Reference Guide

## Overview

This document provides a comprehensive reference for porting **Solarhub** (Ruby on Rails 7.2) to **PayGoHub** (ASP.NET Core 10.0 MVC).

| Source | Target |
|--------|--------|
| Rails 7.2.2.2, Ruby 3.3.7 | .NET 10.0, C# 13 |
| MySQL 8.0 + MongoDB | MySQL 8.0 (retain) + MongoDB |
| 250+ ActiveRecord models | Entity Framework Core entities |
| Grape REST API | ASP.NET Core Web API |
| Sidekiq + Redis | Hangfire + Redis |

**Priority:** Payments & M-Pesa integration first

---

## 1. Technology Mapping

| Rails | .NET Equivalent | NuGet Package |
|-------|-----------------|---------------|
| ActiveRecord | EF Core | `Pomelo.EntityFrameworkCore.MySql` |
| Devise | ASP.NET Identity | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| Pundit | Policy-based auth | Built-in + custom handlers |
| Sidekiq | Hangfire | `Hangfire`, `Hangfire.MySqlStorage` |
| Grape API | Web API | Built-in |
| AASM (state machine) | Stateless | `Stateless` |
| Money-Rails | NodaMoney | `NodaMoney` |
| Twilio | Twilio SDK | `Twilio` |
| HTTParty | Refit | `Refit.HttpClientFactory` |
| RabbitMQ (Bunny) | MassTransit | `MassTransit.RabbitMQ` |
| Thinking Sphinx | Elasticsearch | `NEST` |
| PaperTrail | Audit.NET | `Audit.EntityFramework.Core` |
| Kaminari | X.PagedList | `X.PagedList.Mvc.Core` |

---

## 2. Solution Structure

```yaml
PayGoHub/
├── src/
│   ├── PayGoHub.Domain/           # Entities, Value Objects, Interfaces
│   ├── PayGoHub.Application/      # Use cases, DTOs, MediatR handlers
│   ├── PayGoHub.Infrastructure/   # EF Core, External services
│   ├── PayGoHub.Web/              # MVC (existing)
│   ├── PayGoHub.Api/              # Mobile API (Grape replacement)
│   └── PayGoHub.BackgroundJobs/   # Hangfire workers
├── tests/
└── tools/
    └── DataMigration/
```

---

## 3. Database Migration Approach

### 3.1 Strategy: Retain MySQL 8.0

Keep existing MySQL database to minimize risk. Use Pomelo EF Core provider.

### 3.2 Schema Migration Steps

```bash
# 1. Scaffold entities from existing MySQL
cd PayGoHub.Infrastructure
dotnet ef dbcontext scaffold \
  "Server=localhost;Database=solarhub_development;User=root;Password=xxx" \
  Pomelo.EntityFrameworkCore.MySql \
  -o ../PayGoHub.Domain/Entities \
  --context PayGoHubDbContext

# 2. Refine generated entities manually
# 3. Create EF migrations for any schema changes
dotnet ef migrations add InitialCreate
```

### 3.3 Data Type Mappings

| Rails | C#/EF Core | Notes |
|-------|------------|-------|
| `string` | `string` | Add `[MaxLength(n)]` |
| `text` | `string` | `.HasColumnType("text")` |
| `decimal(10,2)` | `decimal` | `.HasPrecision(10, 2)` |
| `datetime` | `DateTime` | Consider `DateTimeOffset` |
| `boolean` | `bool` | |
| `json` | `JsonDocument` or typed | MySQL JSON column |
| `enum` | `enum` | Store as string |

### 3.4 Soft Delete Pattern

```csharp
// Rails: Paranoia gem → EF Core: Global Query Filter
public abstract class SoftDeletableEntity
{
    public DateTime? DeletedAt { get; set; }
}

// In DbContext
modelBuilder.Entity<Customer>()
    .HasQueryFilter(c => c.DeletedAt == null);
```

### 3.5 Data Migration Script Template

```csharp
// tools/DataMigration/Program.cs
public class DataMigrator
{
    public async Task MigrateCustomersAsync()
    {
        // Read from Rails DB
        var railsCustomers = await _railsContext.Customers.ToListAsync();

        // Transform and insert to .NET DB
        foreach (var rc in railsCustomers)
        {
            var customer = new Customer
            {
                Id = rc.Id,
                Name = rc.Name,
                // ... map all fields
                CreatedAt = rc.CreatedAt,
                UpdatedAt = rc.UpdatedAt
            };
            _paygoContext.Customers.Add(customer);
        }
        await _paygoContext.SaveChangesAsync();
    }
}
```

---

## 4. Priority Domain: Payments & M-Pesa

### 4.1 Key Rails Models to Port

```css
app/models/
├── payment.rb                 → Payment.cs
├── payment_account.rb         → PaymentAccount.cs
├── payment_details_*.rb       → PaymentDetails (polymorphic)
├── loan.rb                    → Loan.cs
├── loan_account.rb            → LoanAccount.cs
├── loan_portfolio.rb          → LoanPortfolio.cs
├── loan_reschedule_request.rb → LoanRescheduleRequest.cs
├── charge_account.rb          → ChargeAccount.cs
├── interest_account.rb        → InterestAccount.cs
└── subscription_account.rb    → SubscriptionAccount.cs
```

### 4.2 Payment Entity Example

```csharp
// PayGoHub.Domain/Entities/Payment.cs
public class Payment : SoftDeletableEntity
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? LoanAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionReference { get; set; }
    public string? MpesaReceiptNumber { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public LoanAccount? LoanAccount { get; set; }
}

public enum PaymentStatus { Pending, Completed, Failed, Reversed }
public enum PaymentMethod { Cash, MobileMoney, BankTransfer, Mpesa }
```

### 4.3 M-Pesa Integration Service

```csharp
// PayGoHub.Infrastructure/Services/MpesaService.cs
public interface IMpesaService
{
    Task<StkPushResponse> InitiateStkPush(StkPushRequest request);
    Task<B2CResponse> SendB2CPayment(B2CRequest request);
    Task<bool> ValidateCallback(string payload, string signature);
}

public class MpesaService : IMpesaService
{
    private readonly HttpClient _httpClient;
    private readonly MpesaOptions _options;

    public async Task<StkPushResponse> InitiateStkPush(StkPushRequest request)
    {
        var token = await GetAccessTokenAsync();

        var payload = new
        {
            BusinessShortCode = _options.ShortCode,
            Password = GeneratePassword(),
            Timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            TransactionType = "CustomerPayBillOnline",
            Amount = request.Amount,
            PartyA = request.PhoneNumber,
            PartyB = _options.ShortCode,
            PhoneNumber = request.PhoneNumber,
            CallBackURL = _options.CallbackUrl,
            AccountReference = request.AccountReference,
            TransactionDesc = request.Description
        };

        var response = await _httpClient.PostAsJsonAsync(
            "mpesa/stkpush/v1/processrequest", payload);

        return await response.Content.ReadFromJsonAsync<StkPushResponse>();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ConsumerKey}:{_options.ConsumerSecret}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        var response = await _httpClient.GetAsync("oauth/v1/generate?grant_type=client_credentials");
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
        return result.AccessToken;
    }
}
```

### 4.4 M-Pesa Callback Controller

```csharp
// PayGoHub.Api/Controllers/MpesaCallbackController.cs
[ApiController]
[Route("api/mpesa")]
public class MpesaCallbackController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("callback")]
    public async Task<IActionResult> StkCallback([FromBody] MpesaCallbackPayload payload)
    {
        await _mediator.Send(new ProcessMpesaPaymentCommand
        {
            MerchantRequestId = payload.Body.StkCallback.MerchantRequestID,
            CheckoutRequestId = payload.Body.StkCallback.CheckoutRequestID,
            ResultCode = payload.Body.StkCallback.ResultCode,
            ResultDesc = payload.Body.StkCallback.ResultDesc,
            CallbackMetadata = payload.Body.StkCallback.CallbackMetadata
        });

        return Ok(new { ResultCode = 0, ResultDesc = "Success" });
    }
}
```

### 4.5 Loan State Machine

```csharp
// PayGoHub.Domain/StateMachines/LoanStateMachine.cs
public class LoanStateMachine
{
    private readonly StateMachine<LoanStatus, LoanTrigger> _machine;
    private readonly Loan _loan;

    public LoanStateMachine(Loan loan)
    {
        _loan = loan;
        _machine = new StateMachine<LoanStatus, LoanTrigger>(
            () => _loan.Status,
            s => _loan.Status = s);

        _machine.Configure(LoanStatus.Pending)
            .Permit(LoanTrigger.Approve, LoanStatus.Active)
            .Permit(LoanTrigger.Reject, LoanStatus.Rejected);

        _machine.Configure(LoanStatus.Active)
            .Permit(LoanTrigger.PayOff, LoanStatus.PaidOff)
            .Permit(LoanTrigger.Default, LoanStatus.Defaulted)
            .Permit(LoanTrigger.Reschedule, LoanStatus.Rescheduled);

        _machine.Configure(LoanStatus.Defaulted)
            .Permit(LoanTrigger.WriteOff, LoanStatus.WrittenOff)
            .Permit(LoanTrigger.Recover, LoanStatus.Active);
    }

    public bool CanFire(LoanTrigger trigger) => _machine.CanFire(trigger);
    public void Fire(LoanTrigger trigger) => _machine.Fire(trigger);
}

public enum LoanStatus { Pending, Active, PaidOff, Defaulted, Rescheduled, WrittenOff, Rejected }
public enum LoanTrigger { Approve, Reject, PayOff, Default, Reschedule, WriteOff, Recover }
```

---

## 5. Authentication & Authorization

### 5.1 Identity Setup (Devise Replacement)

```csharp
// Program.cs
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<PayGoHubDbContext>()
.AddDefaultTokenProviders();

// JWT for API
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

### 5.2 Custom Privilege System (Pundit Replacement)

```csharp
// Requirement
public class PrivilegeRequirement : IAuthorizationRequirement
{
    public string Resource { get; }
    public string Action { get; }
    public PrivilegeRequirement(string resource, string action)
    {
        Resource = resource;
        Action = action;
    }
}

// Handler
public class PrivilegeHandler : AuthorizationHandler<PrivilegeRequirement>
{
    private readonly IPrivilegeService _privilegeService;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PrivilegeRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null && await _privilegeService.HasPrivilegeAsync(
            Guid.Parse(userId), requirement.Resource, requirement.Action))
        {
            context.Succeed(requirement);
        }
    }
}

// Registration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Payment.Create", policy =>
        policy.Requirements.Add(new PrivilegeRequirement("Payment", "create")));
    options.AddPolicy("Payment.Read", policy =>
        policy.Requirements.Add(new PrivilegeRequirement("Payment", "read")));
    // ... generate for all resources
});
```

---

## 6. Background Jobs (Sidekiq → Hangfire)

### 6.1 Configuration

```csharp
// Program.cs
builder.Services.AddHangfire(config => config
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        builder.Configuration.GetConnectionString("Hangfire"),
        new MySqlStorageOptions { TablesPrefix = "Hangfire_" })));

builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "critical", "payments", "sms", "default" };
});

// Dashboard (admin only)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
```

### 6.2 Job Examples

```csharp
// Sidekiq: ProcessPaymentWorker.perform_async(payment_id)
// Hangfire equivalent:
BackgroundJob.Enqueue<IPaymentProcessor>(x => x.ProcessAsync(paymentId));

// Sidekiq: ProcessPaymentWorker.perform_in(1.hour, payment_id)
BackgroundJob.Schedule<IPaymentProcessor>(
    x => x.ProcessAsync(paymentId),
    TimeSpan.FromHours(1));

// Sidekiq-Cron recurring job
RecurringJob.AddOrUpdate<IInterestCalculator>(
    "calculate-daily-interest",
    x => x.CalculateAsync(),
    "0 2 * * *"); // 2 AM daily
```

### 6.3 Key Jobs to Port

| Rails Worker | Hangfire Job |
|--------------|--------------|
| `ProcessIncomingSmsJob` | `SmsProcessorJob` |
| `CalculateInterestAndPrincipleJob` | `InterestCalculatorJob` |
| `ImportMpesaStatementJob` | `MpesaStatementImportJob` |
| `PaymentReminderJob` | `PaymentReminderJob` |
| `DeviceUnlockJob` | `DeviceUnlockJob` |

---

## 7. API Layer (Grape → Web API)

### 7.1 Controller Structure

```csharp
// PayGoHub.Api/Controllers/V1/PaymentsController.cs
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetAll(
        [FromQuery] PaymentQueryParams query)
    {
        var result = await _mediator.Send(new GetPaymentsQuery(query));
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Payment.Create")]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentRequest request)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(Guid id)
    {
        await _mediator.Send(new ProcessPaymentCommand(id));
        return NoContent();
    }
}
```

---

## 8. Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "MySQL": "Server=localhost;Database=paygohub;User=root;Password=xxx",
    "MongoDB": "mongodb://localhost:27017/paygohub",
    "Redis": "localhost:6379",
    "Hangfire": "Server=localhost;Database=paygohub_hangfire;User=root;Password=xxx"
  },
  "Mpesa": {
    "Environment": "sandbox",
    "ConsumerKey": "xxx",
    "ConsumerSecret": "xxx",
    "ShortCode": "174379",
    "Passkey": "xxx",
    "CallbackUrl": "https://yourdomain.com/api/mpesa/callback"
  },
  "Twilio": {
    "AccountSid": "xxx",
    "AuthToken": "xxx",
    "FromNumber": "+1234567890"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here",
    "Issuer": "PayGoHub",
    "Audience": "PayGoHub.Mobile",
    "ExpiryMinutes": 60
  }
}
```

---

## 9. Implementation Phases

### Phase 1: Foundation (Start Here)

- [ ] Create solution structure (Domain, Application, Infrastructure projects)
- [ ] Configure EF Core with MySQL
- [ ] Set up ASP.NET Identity
- [ ] Configure Hangfire

### Phase 2: Payments Domain (Priority)

- [ ] Port Payment, LoanAccount, PaymentAccount entities
- [ ] Implement M-Pesa service
- [ ] Create payment API endpoints
- [ ] Port payment background jobs
- [ ] Implement loan state machine

### Phase 3: Customer & Device Domain

- [ ] Port Customer, System, Device entities
- [ ] Implement device unlock services (Angaza, DEDA)
- [ ] Port customer management features

### Phase 4: Operations

- [ ] Installation cases
- [ ] Maintenance visits
- [ ] Alerts and monitoring

### Phase 5: Reporting & Admin

- [ ] Dashboard and reports
- [ ] Admin interface
- [ ] Data export (Excel, PDF)

---

## 10. Files to Create

| File | Purpose |
|------|---------|
| `PayGoHub.Domain/Entities/Payment.cs` | Payment entity |
| `PayGoHub.Domain/Entities/Loan.cs` | Loan entity |
| `PayGoHub.Domain/Entities/Customer.cs` | Customer entity |
| `PayGoHub.Infrastructure/Services/MpesaService.cs` | M-Pesa integration |
| `PayGoHub.Infrastructure/Persistence/PayGoHubDbContext.cs` | EF Core context |
| `PayGoHub.Api/Controllers/V1/PaymentsController.cs` | Payments API |
| `PayGoHub.BackgroundJobs/Jobs/InterestCalculatorJob.cs` | Daily interest calc |

---

## Next Steps

1. Create the solution structure with additional projects
2. Scaffold entities from existing MySQL database
3. Implement Payment and Loan domain first
4. Build M-Pesa integration service
5. Create API endpoints for mobile app
