# External Services Analysis for Rust Lambda Migration

## Overview

This document analyzes external services used in Solarhub and PayGoHub for potential migration to Rust Lambda functions on AWS Free Tier.

---

## External Services Identified

### 1. Payment & Financial Services

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **M-Pesa** | Mobile money payments | PaymentMethod enum | HIGH - Core functionality |
| **MTN MoMo** | Mobile money (momo.rb) | lib/momo.rb | HIGH |
| **PayJoy** | Payment processing | lib/payjoy/ | MEDIUM |
| **Angaza** | Token generation | lib/angaza/, ANGAZA_API_URL | HIGH |

### 2. Device & IoT Management

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **DEDA** | Device management, GSM configs | DEDA_* env vars, lib/deda/ | HIGH |
| **Aeris SIM** | SIM card management | lib/aeris/, AERIS_SIM_API_* | MEDIUM |
| **Vodafone M2M** | SIM management | lib/vodafone/, VODAFONE_M2M_* | MEDIUM |
| **Moto Tokens** | Device tokens | lib/moto/ | HIGH |
| **Lorentz** | Solar pump controllers | LORENTZ_API_KEY | LOW |

### 3. Loan & Credit Management

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **ALMS** | Loan Management System | lib/alms/, ALMS_ENDPOINT | HIGH |
| **FenixDB** | Inventory/customer data | lib/fenixdb_api/ | MEDIUM |
| **GLP** | Global Leasing Platform | lib/glp/ | LOW |

### 4. Communication Services

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **Twilio** | SMS messaging | twilio-ruby gem | HIGH |
| **Firebase Cloud Messaging** | Push notifications | SendCloudMessage service | MEDIUM |
| **Teams Webhook** | Notifications | TEAMS_WEBHOOK_URL | LOW |

### 5. Infrastructure Services

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **AWS S3** | File storage | aws-sdk-s3 gem | HIGH |
| **Redis** | Caching, queues | redis-mutex, rpush-redis | MEDIUM |
| **Sidekiq** | Background jobs | sidekiq gems | Replaced by Lambda |

### 6. Authentication & Security

| Service | Purpose | Current Integration | Lambda Migration Priority |
|---------|---------|---------------------|---------------------------|
| **MOAuth** | Authentication | MOAUTH_UID/TOKEN | HIGH |
| **XDesk** | Internal auth | lib/xdesk_api/, XDESK_AUTH_KEY | MEDIUM |
| **CSRF/Session** | Rails security | SOLARHUB_SESSION_COOKIE | N/A (handled differently) |

---

## AWS Free Tier Lambda Architecture

### Proposed Rust Lambda Functions

```
┌─────────────────────────────────────────────────────────────────┐
│                      API Gateway                                 │
│                   (Free: 1M requests/month)                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
    ┌───────────────────────┼───────────────────────┐
    │                       │                       │
    ▼                       ▼                       ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Payment     │     │ Device      │     │ SMS         │
│ Lambda      │     │ Lambda      │     │ Lambda      │
│ (M-Pesa,    │     │ (DEDA,      │     │ (Twilio)    │
│  MoMo)      │     │  Aeris)     │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │
       ▼                   ▼                   ▼
┌─────────────────────────────────────────────────────┐
│              DynamoDB (Free: 25GB, 25 WCU/RCU)      │
└─────────────────────────────────────────────────────┘
```

### Lambda Function Specifications

#### 1. payment-service (Rust)
```rust
// Handles: M-Pesa, MTN MoMo, PayJoy, Angaza tokens
// Trigger: API Gateway, SQS
// Memory: 128MB (Free tier optimized)
// Timeout: 30s

pub async fn handle_payment(event: PaymentRequest) -> Result<PaymentResponse, Error> {
    match event.provider {
        Provider::Mpesa => mpesa::process_payment(event).await,
        Provider::MoMo => momo::process_payment(event).await,
        Provider::Angaza => angaza::generate_token(event).await,
    }
}
```

#### 2. device-service (Rust)
```rust
// Handles: DEDA, Aeris SIM, Vodafone M2M, Moto tokens
// Trigger: API Gateway, EventBridge (scheduled)
// Memory: 128MB

pub async fn handle_device(event: DeviceRequest) -> Result<DeviceResponse, Error> {
    match event.action {
        Action::GetConfig => deda::fetch_gsm_config(event).await,
        Action::UpdateSim => aeris::update_sim_status(event).await,
        Action::GenerateToken => moto::generate_token(event).await,
    }
}
```

#### 3. notification-service (Rust)
```rust
// Handles: Twilio SMS, FCM push, Teams webhook
// Trigger: SQS (async processing)
// Memory: 128MB

pub async fn handle_notification(event: SqsEvent) -> Result<(), Error> {
    for record in event.records {
        let msg: NotificationRequest = serde_json::from_str(&record.body)?;
        match msg.channel {
            Channel::Sms => twilio::send_sms(msg).await?,
            Channel::Push => fcm::send_push(msg).await?,
            Channel::Webhook => teams::post_webhook(msg).await?,
        }
    }
    Ok(())
}
```

#### 4. loan-service (Rust)
```rust
// Handles: ALMS integration, loan status updates
// Trigger: API Gateway, EventBridge
// Memory: 128MB

pub async fn handle_loan(event: LoanRequest) -> Result<LoanResponse, Error> {
    match event.action {
        Action::SyncStatus => alms::sync_loan_status(event).await,
        Action::ImportProducts => fenixdb::import_products(event).await,
    }
}
```

---

## AWS Free Tier Resources

| Resource | Free Tier Limit | Our Usage |
|----------|-----------------|-----------|
| Lambda | 1M requests, 400K GB-s | ~500K requests |
| API Gateway | 1M requests/month | ~500K requests |
| DynamoDB | 25GB, 25 RCU/WCU | ~5GB, 10 RCU/WCU |
| S3 | 5GB, 20K GET, 2K PUT | ~2GB, 10K ops |
| SQS | 1M requests | ~200K messages |
| EventBridge | Free | Scheduling |
| CloudWatch | 5GB logs | ~1GB logs |

---

## Mock Service Architecture

For development/testing without external dependencies:

```
┌─────────────────────────────────────────────────────────────────┐
│                    LocalStack (Docker)                          │
│  - Lambda functions                                             │
│  - API Gateway                                                  │
│  - DynamoDB                                                     │
│  - SQS                                                          │
│  - S3                                                           │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────┼───────────────────────────────────┐
│                     Mock External APIs                         │
├────────────────┬──────────────────┬───────────────────────────┤
│ WireMock       │ Responses        │ Purpose                   │
├────────────────┼──────────────────┼───────────────────────────┤
│ /mpesa/*       │ mpesa_mock.json  │ M-Pesa simulation         │
│ /momo/*        │ momo_mock.json   │ MTN MoMo simulation       │
│ /deda/*        │ deda_mock.json   │ Device API simulation     │
│ /twilio/*      │ twilio_mock.json │ SMS simulation            │
│ /alms/*        │ alms_mock.json   │ Loan API simulation       │
└────────────────┴──────────────────┴───────────────────────────┘
```

### Docker Compose for Local Development

```yaml
version: '3.8'
services:
  localstack:
    image: localstack/localstack:latest
    ports:
      - "4566:4566"
    environment:
      - SERVICES=lambda,apigateway,dynamodb,sqs,s3
      - LAMBDA_EXECUTOR=docker-reuse
    volumes:
      - "/var/run/docker.sock:/var/run/docker.sock"

  wiremock:
    image: wiremock/wiremock:latest
    ports:
      - "8080:8080"
    volumes:
      - ./mocks:/home/wiremock
```

---

## Rust Crate Dependencies

```toml
[dependencies]
# AWS SDK
aws-config = "1.0"
aws-sdk-dynamodb = "1.0"
aws-sdk-sqs = "1.0"
aws-sdk-s3 = "1.0"
lambda_runtime = "0.8"
lambda_http = "0.8"

# HTTP clients
reqwest = { version = "0.11", features = ["json"] }
tokio = { version = "1", features = ["full"] }

# Serialization
serde = { version = "1.0", features = ["derive"] }
serde_json = "1.0"

# Error handling
thiserror = "1.0"
anyhow = "1.0"

# Logging
tracing = "0.1"
tracing-subscriber = "0.3"
```

---

## Implementation Roadmap

### Phase 1: Core Payment Services (Week 1-2)
- [ ] Create payment-service Lambda (M-Pesa mock)
- [ ] Set up DynamoDB tables for transactions
- [ ] Implement API Gateway endpoints
- [ ] Add mock M-Pesa/MoMo responses

### Phase 2: Device Management (Week 3-4)
- [ ] Create device-service Lambda
- [ ] Mock DEDA API responses
- [ ] Implement token generation (Moto/Angaza pattern)

### Phase 3: Notifications (Week 5)
- [ ] Create notification-service Lambda
- [ ] SQS queue for async processing
- [ ] Mock Twilio/FCM

### Phase 4: Integration (Week 6)
- [ ] Connect to PayGoHub .NET app
- [ ] End-to-end testing
- [ ] Documentation

---

## Environment Variables for Lambda

```bash
# Payment Services
MPESA_CONSUMER_KEY=mock_key
MPESA_CONSUMER_SECRET=mock_secret
MPESA_PASSKEY=mock_passkey
MOMO_API_KEY=mock_key
ANGAZA_API_URL=http://wiremock:8080/angaza

# Device Services
DEDA_API_KEY=mock_key
DEDA_BASE_URL=http://wiremock:8080/deda
AERIS_API_KEY=mock_key
MOTO_API_KEY=mock_key

# Notification Services
TWILIO_ACCOUNT_SID=mock_sid
TWILIO_AUTH_TOKEN=mock_token
FCM_SERVER_KEY=mock_key

# Loan Services
ALMS_AUTH_KEY=mock_key
ALMS_ENDPOINT=http://wiremock:8080/alms
```

---

## PayGoHub Integration Points

The PayGoHub .NET application will call these Lambda functions via:

1. **HTTP Client** - Direct API Gateway calls
2. **AWS SDK** - For SQS message publishing (async operations)

```csharp
// Example: PayGoHub calling Payment Lambda
public class PaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly string _lambdaEndpoint;

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_lambdaEndpoint}/payments",
            request
        );
        return await response.Content.ReadFromJsonAsync<PaymentResult>();
    }
}
```

---

## Summary

**Total Lambda Functions Needed:** 4
- payment-service
- device-service
- notification-service
- loan-service

**AWS Free Tier Compatibility:** YES
- All services fit within free tier limits
- 128MB Lambda memory is sufficient for Rust
- Rust cold start: ~50-100ms (very fast)

**Mock Coverage:** Complete
- All external APIs can be mocked with WireMock
- LocalStack provides AWS service mocks
- Full development environment without external dependencies
