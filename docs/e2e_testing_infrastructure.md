# Solarhub E2E & Performance Testing

End-to-end testing infrastructure using Cucumber/Playwright BDD and K6 performance testing for the Solarhub application.

## Quick Start

```bash
cd script/eric/e2e_perf
pnpm install
npx playwright install --with-deps chromium

# Run all E2E tests
pnpm test:ci

# Run regression tests (Q4 initiative)
pnpm test:regression
```

## Directory Structure

```
e2e_perf/
├── config/
│   └── cucumber.js           # Cucumber profiles and configuration
├── tests/
│   └── features/
│       ├── step_definitions/ # Step definition implementations
│       ├── support/          # Hooks, world, page objects
│       ├── commissions/      # Commission/target epic features
│       ├── inventory/        # Transport epic features
│       ├── payments/         # Credit notes epic features
│       └── regressions/      # Q4 regression & edge-case tests
├── performance-testing/      # K6 performance test scripts
├── reports/                  # Generated test reports
└── package.json
```

## Available Scripts

### E2E Tests (Cucumber/Playwright)

| Script | Description |
|--------|-------------|
| `pnpm test:ci` | Full E2E suite for CI (excludes @wip) |
| `pnpm test:bdd` | BDD tests with default profile |
| `pnpm test:regression` | Q4 regression & edge-case tests |
| `pnpm test:regression:critical` | Critical-only regression tests |
| `pnpm test:epic` | Unmerged MR epic features |
| `pnpm cucumber:test --profile <name>` | Run with specific profile |

### K6 Performance Tests

| Script | Description |
|--------|-------------|
| `pnpm k6:health` | Health check across index pages |
| `pnpm k6:comprehensive` | Extended comprehensive test |
| `pnpm k6:moapi` | MoAPI performance test |

## Cucumber Profiles

Profiles are defined in `config/cucumber.js`:

| Profile | Tags | Use Case |
|---------|------|----------|
| `ci` | `not @wip` | CI/CD pipeline |
| `regression` | `@ad-hoc or @spec/features or @edge-case or @critical` | Q4 regression tests |
| `regression-critical` | `@critical` | Quick validation |
| `epic` | `@spec/features` | Unmerged MR features |
| `existing` | `@existing` | RSpec-converted tests |
| `headed` | - | Visible browser debugging |
| `serial` | - | Single-worker execution |

## Q4 Regression Testing Initiative

The regression tests cover three categories:

### 1. Open MR Epics (`@spec/features`)

Features from unmerged branches:
- **Commissions Epic** (`origin/commissions_epic`) - Commission calculations, eligibility services
- **Target Structure Epic** (`origin/target-structure-epic`) - Target hierarchy, draft/approval workflow
- **Hub-to-Agent Transport** (`origin/afam-1452-hub-to-agent-transport-epic`) - Transport finalization, state transitions
- **Automated Transport Cancellations** (`origin/automated_transport_cancellations_epic`) - 6-hour auto-cancellation
- **Credit Notes** (`origin/task/enable-credit-not-build-from-discount-compensation`)

### 2. QA Wiki Issues (`@ad-hoc @critical`)

Regressions from Aureliu's maintenance epic:
- Serialization issues (missing serials)
- Payment assignment (Sphinx search)
- Duplicate serials during inventorisation
- Auto-created solar controllers

### 3. Support Issues (`@ad-hoc @edge-case`)

Ad-hoc fixes documented in support tickets:
- ALMS finalization race conditions
- Loan portfolio collateral attachment
- Wrong handover correction workflow
- Duplicate component detection

## Tag Strategy

```gherkin
@ad-hoc @{priority} @{category} @edge-case
```

| Tag | Purpose |
|-----|---------|
| `@spec/features` | MR-derived tests (not yet in master) |
| `@ad-hoc` | Support issue-derived tests |
| `@critical` | Production-impacting regressions |
| `@edge-case` | Edge cases from documented issues |
| `@epic/{name}` | Links to epic branch |
| `@wip` | Work in progress (excluded from CI) |
| `@manual` | Requires manual verification |

## CI/CD Integration

The `.gitlab-ci.yml` includes these E2E jobs in the `e2e_perf` stage:

| Job | Markets | Description |
|-----|---------|-------------|
| `e2e_training_tests` | All 7 | Full E2E suite |
| `e2e_regression_tests` | All 7 | Regression & edge-case tests |
| `e2e_regression_critical` | uganda, mobisol | Critical-only quick validation |
| `k6_health_training` | All 7 | K6 health checks |
| `k6_web_training` | All 7 | K6 comprehensive tests |
| `k6_moapi_training` | All 7 | K6 MoAPI tests (requires auth) |

All E2E jobs are manual triggers with `allow_failure: true`.

### Running in CI

1. Navigate to GitLab > CI/CD > Pipelines
2. Click "Run Pipeline" on your branch
3. Manually trigger the desired E2E job
4. View artifacts for reports and screenshots

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `BASE_URL` | Target application URL | Required |
| `TEST_COUNTRY` | Country code (UG, BJ, CI, etc.) | Required |
| `TEST_USER_USERNAME` | Login username | `demo.user` |
| `TEST_USER_PASSWORD` | Login password | `123#456` |
| `HEADLESS` | Run browser headless | `true` |
| `TAGS` | Cucumber tag filter | `not @wip` |

## Writing Tests

### Feature Files

```gherkin
@ad-hoc @critical @edge-case @serialization
Feature: Support Issue - Missing Serials
  As a warehouse manager
  I want serial validation before handover
  So that BCU swaps don't fail due to missing serials

  Background:
    Given I am logged in as a warehouse manager
    And the application is running

  @REG-1201
  Scenario: Block swap for system with missing serial
    Given a system "SYS-001" exists without a serial number
    When I attempt to initiate a BCU swap
    Then the swap should be blocked
    And I should see error "System serial required for swap"
```

### Step Definitions

```typescript
import { Given, When, Then } from '@cucumber/cucumber';
import { expect } from '@playwright/test';

Given('a system {string} exists without a serial number', async function(systemId: string) {
  // Implementation
});

When('I attempt to initiate a BCU swap', async function() {
  // Implementation
});

Then('the swap should be blocked', async function() {
  // Implementation
});
```

## Reports

After test execution, reports are generated in `reports/`:

- `cucumber-ci-report.html` - HTML report with screenshots
- `cucumber-ci-report.json` - JSON for further processing
- `cucumber-junit.xml` - JUnit format for CI integration

View reports locally:
```bash
pnpm reports:serve
```

## Troubleshooting

### Tests timing out
Increase timeout in cucumber profile or use serial profile:
```bash
pnpm cucumber:test --profile serial
```

### Browser not launching
Ensure Playwright browsers are installed:
```bash
npx playwright install --with-deps chromium
```

### Step definitions not found
Check that step definition files match the pattern in cucumber.js:
```
tests/features/step_definitions/**/*.ts
```

## Contributing

1. Create feature files in appropriate domain folder
2. Tag with relevant categories (`@ad-hoc`, `@critical`, etc.)
3. Implement step definitions in `step_definitions/`
4. Run locally before pushing
5. Verify in CI with manual trigger
