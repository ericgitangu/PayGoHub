# E2E Testing Quick Start

## Prerequisites

```bash
cd script/eric/e2e_perf
pnpm install
npx playwright install --with-deps chromium
```

## Run Tests

```bash
# Full CI test suite
pnpm test:ci

# Smoke tests (5-10 min) - Critical scenarios only
pnpm test:smoke

# Fast tests (10-15 min) - Smoke + critical
pnpm test:fast

# Regression tests - Bug fixes & edge cases
pnpm test:regression
```

## Target Specific Environments

```bash
# Uganda training environment
COUNTRY=uganda BASE_URL=https://uganda-training.plugintheworld.com pnpm test:ci

# Benin training environment
COUNTRY=benin BASE_URL=https://benin-training.plugintheworld.com pnpm test:ci

# Available: uganda, kenya, benin, ivorycoast, tanzania, zambia, mozambique, nigeria
```

## Common Commands

```bash
# Run specific tags
TAGS="@commission" pnpm test:ci

# Run specific feature file
pnpm test:bdd tests/features/commissions/epic_commissions.feature

# Dry-run to check step definitions (no test execution)
pnpm test:bdd --dry-run

# Run in headed mode (visible browser)
pnpm test:headed
```

## Benchmark Testing

**What is benchmark?** Performance comparison testing that measures execution time improvements between baseline and optimized code.

```bash
# Run benchmark comparison (measures performance gains)
pnpm test:benchmark
```

**Benchmark measures:**
- Test execution time (baseline vs optimized)
- Step execution speed
- Overall suite performance
- Identifies performance regressions

## Test Profiles

| Profile | Runtime | Scenarios | Use Case |
|---------|---------|-----------|----------|
| `smoke` | 5-10 min | ~20 | Pre-commit validation |
| `fast` | 10-15 min | ~40 | CI pipeline (PRs) |
| `ci` | 30-45 min | ~530 | Full suite (merges) |
| `regression` | 20-30 min | ~150 | Bug fixes & edge cases |
| `benchmark` | Variable | All | Performance measurement |

## Configuration

**Timeouts:**
- Step timeout: 60s (reduced from 180s for faster failures)
- Retry count: 1 (reduced from 3 to prevent cascading failures)
- Parallel workers: 8

**Edit config:** `config/cucumber.js`

## Test Organization

```
tests/features/
├── authentication/     # Login, RBAC tests
├── commissions/        # Commission calculations
├── customers/          # Customer registration, KYC
├── devices/            # IoT device tests
├── inventory/          # Stock, transport logistics
├── loans/              # Loan management
├── maintenance/        # Maintenance workflows
├── payments/           # Payment processing
├── regressions/        # Bug regression tests
└── sales/              # Sales pipeline, leads
```

## Key Tags

- `@smoke` - Critical path (must always pass)
- `@critical` - Important business flows
- `@wip` - Work in progress (excluded from CI)
- `@ad-hoc` - Support issue fixes
- `@edge-case` - Edge case validations
- `@spec/features` - New features (unmerged MRs)

Domain tags: `@commission`, `@customer`, `@payment`, `@loan`, etc.

## Troubleshooting

```bash
# Clear cached auth sessions
rm -rf tests/.auth

# Clear test reports
rm -rf tests/reports

# Regenerate auth with fresh login
FORCE_AUTH=true pnpm test:smoke

# Check for undefined/ambiguous steps
pnpm test:bdd --dry-run

# View detailed debug logs
DEBUG=pw:api pnpm test:bdd
```

## CI/CD Integration

Tests run in GitLab CI pipeline (manual triggers):

```yaml
# .gitlab-ci.yml
e2e_training_tests:
  stage: e2e_perf
  script: pnpm test:ci

e2e_regression_tests:
  stage: e2e_perf
  script: pnpm test:regression
```

## Reports

After test execution:

```bash
# Open HTML report
open reports/cucumber-ci-report.html

# View JSON report
cat reports/cucumber-ci-report.json

# Check screenshots for failures
ls tests/screenshots/
```

## Documentation

- **Main README**: [script/eric/e2e_perf/README.md](README.md)
- **Features Guide**: [tests/features/README.md](tests/features/README.md)
- **QA Wiki**: https://git.plugintheworld.com/db-dev/qa/-/wikis

---

**Quick Links:**
- [Testing Strategy](https://git.plugintheworld.com/db-dev/qa/-/wikis/testing/README)
- [Business Domains](https://git.plugintheworld.com/db-dev/qa/-/wikis/business_domains/README)
- [CI/CD Setup](https://git.plugintheworld.com/db-dev/qa/-/wikis/guides/ci/ci-cd-setup)
- [Troubleshooting](https://git.plugintheworld.com/db-dev/qa/-/wikis/troubleshooting/README)
